using UnityEngine;
using System.Collections.Generic;

public class BowlerScript : MonoBehaviour
{
    public enum ShotStatus
    {
        Wait,
        Hit,
        Missed,
    }

    public Animator bowlerAnimator;
    public Transform boundryPoint;
    public Transform tapPoint;
    public Transform tapPointEnd;
    public Transform tapPointShadowEnd;
    public Transform keeperPosition;
    public GameObject ball;
    public GameObject ballShadow;
    public Transform boneBall;
    public float ballSpeed = 10f;
    public float sixSpeed = 10f;
    private bool isBallMoving = false;
    private Vector3 initialScale;
    private Vector3 initialShadowScale;
    public Vector3 maxScale = new Vector3(1f, 1f, 1f);
    public Vector3 maxScaleShadow = new Vector3(1f, 1f, 1f);
    public Vector3 shadowBallInitialPosition;
    private bool reachedTapPoint = false;
    // private bool reachedPeakPoint = false;
    private float shadowSpeed;
    public float hitDistanceThreshold = 0.2f;
    public ShotStatus shotStatusEnum;
    public Transform peakPoint; // New peak point for the arc trajectory
    public Vector3 minScale = new Vector3(0.2f, 0.2f, 0.2f);
    public Vector3 minScaleShadowBoundry = new Vector3(0.08f, 0.08f, 0.08f);

    // Progress variable for parabolic movement
    private float arcProgress = 0f;
    private Vector3 startPoint;
    public List<ShotConfiguration> shotConfigurations = new List<ShotConfiguration>();
    private int currentShotIndex = 0;

    public ParticleSystem sixHitEffect; // Assign in the Inspector
private TrailRenderer ballTrailRenderer;


    void Start()
    {
        bowlerAnimator = GetComponent<Animator>();

        if (ball != null && ballShadow != null)
        {
            shotStatusEnum = ShotStatus.Wait;

            initialScale = ball.transform.localScale;
            initialShadowScale = ballShadow.transform.localScale;
            shadowBallInitialPosition = ballShadow.transform.position;

            SpriteRenderer ballRenderer = ball.GetComponent<SpriteRenderer>();
            if (ballRenderer != null)
            {
                ballRenderer.sortingLayerName = "Default";
                ballRenderer.sortingOrder = 10;
            }

            ballTrailRenderer = ball.GetComponent<TrailRenderer>();
             if (ballTrailRenderer != null)
                  {
                  ballTrailRenderer.emitting = false; // Start with trail off
             }
        }

      currentShotIndex = Random.Range(0, (shotConfigurations.Count));
      Debug.Log("LOG::" +currentShotIndex);
    }

    void Update()
    {
        if (isBallMoving && ball != null)
        {
            if (shotStatusEnum == ShotStatus.Hit)
            {
                Debug.Log("South selected");
                MoveBallInArcTowards(shotConfigurations[currentShotIndex].boundryPoint.position);
            }
            else
            {
                if (!reachedTapPoint)
                {
                    MoveBallTowards(tapPoint.position);
                }
                else
                {
                    MoveBallAfterTapTowards(tapPointEnd.position, tapPointShadowEnd.position);
                }
            }
        }
    }

    private void MoveBallTowards(Vector3 targetPosition)
    {
        ball.transform.position = Vector3.MoveTowards(ball.transform.position, targetPosition, ballSpeed * Time.deltaTime);
        ball.transform.localRotation = Quaternion.identity;

        if (shotStatusEnum == ShotStatus.Wait)
        {
            ballShadow.transform.position = Vector3.MoveTowards(ballShadow.transform.position, targetPosition, shadowSpeed * Time.deltaTime);
            ballShadow.transform.localRotation = Quaternion.identity;
        }
        else if (shotStatusEnum == ShotStatus.Hit)
        {
            ballShadow.transform.position = Vector3.MoveTowards(ballShadow.transform.position, shotConfigurations[currentShotIndex].boundryPoint.position, shadowSpeed * Time.deltaTime);
            ballShadow.transform.localRotation = Quaternion.identity;

            float distanceToTargetShadow = Vector3.Distance(ballShadow.transform.position, shotConfigurations[currentShotIndex].boundryPoint.position);
            float totalDistance = Vector3.Distance(tapPointShadowEnd.position, shotConfigurations[currentShotIndex].boundryPoint.position);
            float progress = Mathf.Clamp01(1f - (distanceToTargetShadow / totalDistance));
            ballShadow.transform.localScale = Vector3.Lerp(maxScale, minScaleShadowBoundry, progress);
        }

        float distanceToTarget = Vector3.Distance(ball.transform.position, targetPosition);

        if (distanceToTarget < 0.1f)
        {
            reachedTapPoint = true;
            float ballDistance = Vector3.Distance(ball.transform.position, tapPointEnd.position);
            float shadowDistance = Vector3.Distance(ballShadow.transform.position, tapPointShadowEnd.position);
            shadowSpeed = ballSpeed * (shadowDistance / ballDistance);
        }
    }

    private void MoveBallInArcTowards(Vector3 targetPosition)
    {
        if (arcProgress < 1f)
        {
            // Update progress along the arc
            arcProgress += sixSpeed * Time.deltaTime / Vector3.Distance(startPoint, targetPosition);

            // Calculate parabolic position
            Vector3 parabolicPosition = CalculateParabola(startPoint, shotConfigurations[currentShotIndex].peakPoint.position, targetPosition, arcProgress);
            ball.transform.position = parabolicPosition;
            ball.transform.localRotation = Quaternion.identity;
                if (sixHitEffect != null)
    {
        sixHitEffect.transform.position = ball.transform.position;
        // sixHitEffect.Play();
    }


            Debug.Log("Arc Progress:::" +arcProgress);

            // Move and scale shadow
            Vector3 shadowPosition = Vector3.Lerp(ballShadow.transform.position, shotConfigurations[currentShotIndex].boundryPoint.position, (0.01f));
            ballShadow.transform.position = shadowPosition;
            float scaleProgress = Mathf.Clamp01(arcProgress);
            ballShadow.transform.localScale = Vector3.Lerp(ballShadow.transform.localScale, minScaleShadowBoundry, scaleProgress);
            ball.transform.localScale = Vector3.Lerp(ball.transform.localScale, minScaleShadowBoundry, scaleProgress);

            if (arcProgress >= 1f)
            {
                isBallMoving = false;
                ball.SetActive(false);
                ballShadow.SetActive(false);
                sixHitEffect.Stop();
            }
        }
    }

    private Vector3 CalculateParabola(Vector3 start, Vector3 peak, Vector3 end, float t)
    {
        // Quadratic Bezier formula: B(t) = (1 - t)^2 * start + 2 * (1 - t) * t * peak + t^2 * end
        return Mathf.Pow(1 - t, 2) * start + 2 * (1 - t) * t * peak + Mathf.Pow(t, 2) * end;
    }

    private void MoveBallAfterTapTowards(Vector3 targetPosition, Vector3 targetPositionShadow)
    {
        ball.transform.position = Vector3.MoveTowards(ball.transform.position, targetPosition, ballSpeed * Time.deltaTime);
        ball.transform.localRotation = Quaternion.identity;

        ballShadow.transform.position = Vector3.MoveTowards(ballShadow.transform.position, targetPositionShadow, shadowSpeed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(ball.transform.position, targetPosition);
        float totalDistance = Vector3.Distance(tapPoint.position, tapPointEnd.position);
        float progress = Mathf.Clamp01(1f - (distanceToTarget / totalDistance));

        ball.transform.localScale = Vector3.Lerp(initialScale, maxScale, progress);

        if (distanceToTarget < 0.1f)
        {
            isBallMoving = false;
            ball.SetActive(false);
            ballShadow.SetActive(false);
        }
    }

    public void ReleaseBall()
    {
        if (ball != null && ballShadow != null)
        {
            // startPoint = ball.transform.position;
            ball.transform.SetParent(null);
            isBallMoving = true;
            reachedTapPoint = false;
            // reachedPeakPoint = false; // Reset peak flag
            arcProgress = 0f; // Reset arc progress

            ballShadow.SetActive(true);
            ballShadow.transform.position = shadowBallInitialPosition;
            float ballDistance = Vector3.Distance(ball.transform.position, tapPoint.position);
            float shadowDistance = Vector3.Distance(ballShadow.transform.position, tapPoint.position);
            shadowSpeed = ballSpeed * (shadowDistance / ballDistance);

            SpriteRenderer ballRenderer = ball.GetComponent<SpriteRenderer>();
            if (ballRenderer != null)
            {
                ballRenderer.sortingLayerName = "Default";
                ballRenderer.sortingOrder = 10;
            }

            Debug.Log("Ball released towards the tap position.");
        }
    }

    public void RestartBowling()
    {
        if (ball != null && ballShadow != null)
        {
            ball.transform.SetParent(boneBall);
            ball.transform.localPosition = Vector3.zero;
            ball.transform.localRotation = Quaternion.identity;
            ball.transform.localScale = initialScale;
            ball.SetActive(true);
            shotStatusEnum = ShotStatus.Wait;
            currentShotIndex = Random.Range(0, (shotConfigurations.Count));
            Debug.Log("LOG::" +currentShotIndex);
            

            ballShadow.transform.localScale = initialShadowScale;
            isBallMoving = false;
            reachedTapPoint = false;
            // reachedPeakPoint = false;

            bowlerAnimator.Play("BowlerAnimation", 0, 0);

            Debug.Log("Restarting bowling sequence.");
        }
    }

    public void OnHitButtonPressed()
    {
        CheckForHit();
    }

    private void CheckForHit()
    {
        if (ball != null)
        {
            float distanceToBat = Vector3.Distance(ball.transform.position, tapPointEnd.position);
            Debug.Log("Distance: :: :: : " + distanceToBat);
            if (distanceToBat <= hitDistanceThreshold)
            {
                startPoint = ball.transform.position; // Set the start point for the parabolic arc
                shotStatusEnum = ShotStatus.Hit;
                TriggerSixEffect();
                Debug.Log("Hit!");
            }
            else
            {
                shotStatusEnum = ShotStatus.Missed;
                Debug.Log("Missed!");
            }
        }
    }

    private void TriggerSixEffect()
{
    // Activate Particle System
    if (sixHitEffect != null)
    {
        sixHitEffect.transform.position = ball.transform.position;
        sixHitEffect.Play();
    }

    // Enable Ball Trail
    // if (ballTrailRenderer != null)
    // {
    //     ballTrailRenderer.emitting = true;
    // }

    // Optionally, add a sound effect or screen shake here
}
}


[System.Serializable]
public class ShotConfiguration
{
    public Transform boundryPoint;
    public Transform peakPoint;
}


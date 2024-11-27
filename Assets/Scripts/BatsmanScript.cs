using UnityEngine;

public class BatsmanScript : MonoBehaviour
{

    private BowlerScript bowlScript;
    public GameObject bowler;



    public void OnAnimationEventTriggered()
    {
        BowlerScript bowlScript = bowler.GetComponent<BowlerScript>();
       
        Debug.Log("Animation Event triggered on Character B!");
             if (bowlScript != null)
        {
            bowlScript.CheckForHit();
        }
        else
        {
            Debug.LogError("Other character reference is missing!");
        }
    }
}

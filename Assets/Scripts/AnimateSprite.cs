using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AnimateSprite : MonoBehaviour
{
    public Sprite[] sprites;
    public float framesPerSecond;
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        int index = (int)(Time.time * framesPerSecond);
        index = index % sprites.Length;
        image.sprite = sprites[index];
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.IO;

public class TestMotion : MonoBehaviour
{
    public bool isEMGControlled;
    public float userInput;
    [Range(1,3)]
    public int smoothFactor = 3;
    public float speed;
    public float smoothTime;
    Vector3 yvelocity = Vector3.zero;
    //public AudioClip[] backgroundSounds;
    //public AudioSource backgroundSound;

    // Start is called before the first frame update
    void Awake()
    {
        //backgroundSound = gameObject.AddComponent<AudioSource>();
        //backgroundSound.clip = backgroundSounds[0];
        //backgroundSound.playOnAwake = true;
        //backgroundSound.loop = true;
        //backgroundSound.volume = 0.4f;
    }

    void Start()
    {
        //backgroundSound.Play();
    }


    // Update is called once per frame
    void Update()
    {
        // Switch between keyboard or user input
        if(!isEMGControlled)
        {
            userInput = Input.GetAxis("Vertical");
        }
        else
        {
            userInput = EmgGameController.emgUserInput;
        }

        //
        if (UIManagerScript.isGameRunning)
        {
            // Clamp user input between max min
            float inputValue = Mathf.Clamp(userInput, 0, 1);
            Vector3 newScale = new Vector3(1f + (inputValue * 13), 1f + (inputValue * 13), 1f + (inputValue * 13));

            switch (smoothFactor)
            {
                case 1:
                    transform.localScale = newScale;
                    break;

                case 2:
                    transform.localScale = Vector3.Lerp(transform.localScale, newScale, speed * Time.deltaTime);
                    break;

                case 3:
                    transform.localScale = Vector3.SmoothDamp(transform.localScale, newScale, ref yvelocity, smoothTime);
                    break;

                default:
                    transform.localScale = newScale;
                    break;
            }
        }
    }
}

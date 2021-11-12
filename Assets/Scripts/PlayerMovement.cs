using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Vector2 initialTouch;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

            Debug.Log(touchPos);

            if (touch.phase == TouchPhase.Began)
            {
                initialTouch = touchPos;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                Vector2 touchDif = touchPos - initialTouch;


                if (touchDif.x > touchDif.y)
                {
                    transform.Translate(new Vector3(1, 0, 0));
                }
                if (touchDif.y > touchDif.x)
                {
                    transform.Translate(new Vector3(0, 1, 0));
                }
            }

        }

    }
} 

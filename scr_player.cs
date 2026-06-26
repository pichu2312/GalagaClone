using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class scr_player : MonoBehaviour
{
    //Movement
    const float MaxX = 3;
    const float MinX = -MaxX;
    float y;
    float z;
    [SerializeField] private float spd = 0;

    //Shooting
    [SerializeField] public List<GameObject> bullets = new List<GameObject>();
    private List<GameObject> activeBullets;
    private List<Transform> activeBulletsTransform;

    const float maxY = 1.6f;
    [SerializeField] private float bulletSpd = 0.05f;



    // Start is called before the first frame update
    void Start()
    {
        y = transform.localPosition.y;
        z = transform.localPosition.z;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessPlayerInput();
        ProcessBullets();
    }
    public void ProcessPlayerInput()
    {
        ProcessPlayerMovement();
        ProcessPlayerFiring();
    }
    public void ProcessPlayerMovement()
    {
        float move = 0;

        //Move of 0 possible by pressing both to emulate galaga
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move -= spd;
        }
        
        if (Input.GetKey(KeyCode.RightArrow))
        {
            move += spd;
        }

        transform.localPosition = new Vector3(Math.Clamp(transform.localPosition.x + move, MinX, MaxX), y, z) ;
    }

    public void ProcessPlayerFiring()
    {
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X)) {
            GameObject newBullet = bullets.Dequeue();
            Transform newBulletTransform = newBullet.transform;

            activeBullets.Add(newBullet);
            activeBulletsTransform.Add(newBulletTransform);

            //Set to ships position
            newBulletTransform.position = new Vector3(transform.position.x,y,z);
            
            
        }
    }

    public void ProcessBullets()
    {
        if (activeBullets.Count > 0)
        {
            for (int i = 0; i < activeBullets.Count; i++)
            {
                GameObject currentObject = activeBullets[i];
                Transform currentTransform = activeBulletsTransform[i];

                currentTransform.position += new Vector3(0,currentTransform.position.y + bulletSpd,0);

                //Remove the bullet if it's gone past the point of no return
                if (currentTransform.localPosition.y > maxY)
                {
                    bullets.Enqueue(currentObject);
                    currentTransform.localPosition = Vector3.zero;

                    activeBullets.RemoveAt(i);
                    activeBulletsTransform.RemoveAt(i);

                    i--;
                }
            }
        }
    }
}

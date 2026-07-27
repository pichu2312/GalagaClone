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
    [SerializeField] public List<GameObject> editorBullets = new();

    public Queue<GameObject> bullets = new();
    private List<GameObject> activeBullets = new();
    private List<Transform> activeBulletsTransform = new();

    //Shooting timers
    private bool canFire = true;
    [SerializeField] private float bulletWait;


    const float maxY = 6f;
    [SerializeField] private float bulletSpd = 0.05f;



    // Start is called before the first frame update
    void Start()
    {
        y = transform.localPosition.y;
        z = transform.localPosition.z;

        //Add the bullet items in the editor to the actually used list
        foreach (GameObject b in editorBullets)
        {
            bullets.Enqueue(b);
        }
        
        editorBullets.Clear();
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
        float speed = spd * Time.deltaTime;

        //Move of 0 possible by pressing both to emulate galaga
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move -= speed;
        }
        
        if (Input.GetKey(KeyCode.RightArrow))
        {
            move += speed;
        }

        transform.localPosition = new Vector3(Math.Clamp(transform.localPosition.x + move, MinX, MaxX), y, z) ;
    }

    public void ProcessPlayerFiring()
    {
        if (canFire)
        {
            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X)) {
                GameObject newBullet = bullets.Dequeue();
                Transform newBulletTransform = newBullet.transform;

                activeBullets.Add(newBullet);
                activeBulletsTransform.Add(newBulletTransform);

                //Set to ships position
                newBulletTransform.position = new Vector3(transform.position.x,y,z);
                
                canFire = false;
                Invoke(nameof(ResetProjectile), bulletWait);
            } 
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

                currentTransform.position += new Vector3(0, bulletSpd,0);

                //Remove the bullet if it's gone past the point of no return
                if (currentTransform.position.y > maxY)
                {
                    ResetBullet(currentObject, currentTransform);
                    i--;
                }
            }
        }
    }

    void ResetBullet(GameObject bullet, Transform bulletTrans)
    {
        bullets.Enqueue(bullet);
        bulletTrans.localPosition = Vector3.zero;

        activeBullets.Remove(bullet);
        activeBulletsTransform.Remove(bulletTrans);
    }

    void ResetProjectile()
    {
        canFire = true;
    }

    public void BulletCollision(GameObject enemy, GameObject bullet)
    {
        ResetBullet(bullet, bullet.transform);
        Destroy(enemy);
    }

}

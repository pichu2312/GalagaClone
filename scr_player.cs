using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class scr_player : MonoBehaviour
{
    //Movement
    const float MaxX = 3;
    const float MinX = -MaxX;
    float y;
    float z;
    [SerializeField] private float spd = 0;

    //Shooting timers
    private bool canFire = true;
    [SerializeField] private float bulletWait;


    const float maxY = 6f;
    [SerializeField] private float bulletSpd = 0.05f;

    public event Action EnemyDestroyed;
    public event Action PlayerDestroyed;


    public scr_enemies enemies;

    public int score = 0;

    bool active = false;

    [SerializeField] SpriteRenderer sprite;
    [SerializeField] public int lives = 2;



    // Start is called before the first frame update
    void Awake()
    {
        y = transform.localPosition.y;
        z = transform.localPosition.z;

        Invoke(nameof(ReactivatePlayer), 2);
    }

    // Update is called once per frame
    void Update()
    {
        ProcessPlayerInput();
        //ProcessBullets();
    }
    public void ProcessPlayerInput()
    {
        if (active)
        {
            ProcessPlayerMovement();
            ProcessPlayerFiring();
        }

        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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

        transform.localPosition = new Vector3(Math.Clamp(transform.localPosition.x + move, MinX, MaxX), y, z);
    }

    public void ProcessPlayerFiring()
    {
        if (canFire)
        {
            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X))
            {
                GameObject newBullet = scr_object_pool.SharedInstance.GetLaser(new Vector3(transform.position.x, y, z));

                if (newBullet != null)
                {
                    newBullet.GetComponent<Bullet>().SetParent(0);
                    canFire = false;
                    Invoke(nameof(ResetProjectile), bulletWait);
                }

            }
        }
    }

    void ResetProjectile()
    {
        canFire = true;
    }

    public void BulletCollision(GameObject enemy, GameObject bullet)
    {
        //ResetBullet(bullet, bullet.transform);
        AddScore(enemy.GetComponent<Enemy>().scoreVal);
        enemies.DestroyEnemy(enemy);

        EnemyDestroyed?.Invoke();
    }

    public void AddScore(int score)
    {
        this.score += score;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (active)
        {
            bool cont = false;

            if (other.TryGetComponent(out Enemy enemy))
            {
                cont = true;
            }
            else
            {
                other.TryGetComponent(out Bullet bullet);
                if (bullet != null)
                {
                    if (bullet.type == 1)
                    {
                        cont = true;
                    }
                }
            }

            if (cont)
            {
                //Remove the player for a little bit
                active = false;
                sprite.enabled = false;

                lives -= 1;

                if (lives <= -1)
                {
                    //GAME OVER
                }
                else
                {
                    Invoke(nameof(ReactivatePlayer), 2f);
                }


                PlayerDestroyed?.Invoke();

            }
        }
    }

    private void ReactivatePlayer()
    {
        active = true;
        sprite.enabled = true;
    }

}

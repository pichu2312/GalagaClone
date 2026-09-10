using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
    public event Action NewLevel;
    public event Action StartMoving;




    public scr_enemies enemies;

    public int score = 0;

    bool active = true;

    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Rigidbody2D body;
    [SerializeField] public int lives = 2;


    //Backgorund
    [SerializeField] public float scrollSpeed;
    [SerializeField] public Renderer renderer;
    [SerializeField] public GameObject background;

    [SerializeField] public GameObject camera;

    [SerializeField] public float transitionSpeed;

    //private float transitionDir = -1;
    private float targetRotation;

    private float currentRotation;
    private bool transitioning = false;

    [SerializeField] public scr_flappy flappy;

    [SerializeField] float flapForce = 5f;




    private enum Mode
    {
        Galaga,
        Flappy
    }

    private Mode state = Mode.Galaga;

    // Start is called before the first frame update
    void Awake()
    {
        y = transform.localPosition.y;
        z = transform.localPosition.z;

        Invoke(nameof(BeginAgain), 2);

        currentRotation = camera.transform.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessPlayerInput();
        MoveBackground();
        TransitionStates();
        //ProcessBullets();
    }
    public void ProcessPlayerInput()
    {
        if (active)
        {
            switch (state)
            {
                case Mode.Galaga:
                    ProcessPlayerMovement();
                    ProcessPlayerFiring();
                    break;
                case Mode.Flappy:
                    ProcessPlayerFlying();
                    break;

            }

        }

        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKey(KeyCode.T))
        {
            if (!transitioning)
            {
                BeginTransition();
            }

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
                GameObject newBullet = scr_object_pool.SharedInstance.GetLaser(new Vector3(transform.position.x, y, 1));

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
        //If true we've destroyed the enemy
        if (enemies.EnemyHit(enemy))
        {
            AddScore(enemy.GetComponent<Enemy>().scoreVal);
            EnemyDestroyed?.Invoke();

            //Check if everything is empty, as then it's new level time
            if (enemies.IsAllEnemiesGone())
            {
                /*
                NewLevel?.Invoke();
                Invoke(nameof(BeginAgain), 2);
                */
                BeginTransition();
            }
        }

    }

    public void ProcessPlayerFlying()
    {
        //Destroy if touching the side
        //actually its x value but shh
        float y = transform.position.x;

        if ((y < -4.5f) || (y > 4.5f))
        {
            DestroyPlayer();
        }
        else if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X))
        {
            //body.AddForce(Vector2.left * flapForce, ForceMode2D.Impulse);
            body.linearVelocity = new Vector2(-flapForce, body.linearVelocityY);
        }


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

                DestroyPlayer();
            }
        }
    }

    private void DestroyPlayer()
    {
        //Remove the player for a little bit
        DeactivePlayer();

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

    private void BeginAgain()
    {
        StartMoving?.Invoke();
    }

    private void ReactivatePlayer()
    {
        active = true;
        sprite.enabled = true;

        //Reset bodytype if it's flappy
        if (state == Mode.Flappy)
        {
            body.bodyType = RigidbodyType2D.Dynamic;

        }
    }

    private void DeactivePlayer()
    {
        active = false;
        sprite.enabled = false;
        //set x to 0
        transform.position = new Vector3(0, y, z);

        body.bodyType = RigidbodyType2D.Static;
    }

    private void MoveBackground()
    {
        float y = Mathf.Repeat(Time.time * scrollSpeed, 1);
        Vector2 offset = new UnityEngine.Vector2(0, y);
        renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
    }

    public void BeginTransition()
    {
        if (state == Mode.Galaga)
        {
            targetRotation = 90;
        }
        else
        {
            targetRotation = 0;
        }

        transitioning = true;
    }

    private void TransitionStates()
    {
        //Rotate the player and background
        if (transitioning)
        {
            currentRotation = Mathf.MoveTowards(
                currentRotation,
                targetRotation,
                transitionSpeed * Time.deltaTime
            );

            //transform.localRotation = Quaternion.Euler(0, 0, currentRotation);
            //background.transform.localRotation = Quaternion.Euler(0, 0, currentRotation);
            camera.transform.localRotation = Quaternion.Euler(0, 0, currentRotation);
            //Move the player too



            if (Mathf.Approximately(currentRotation, targetRotation))
            {
                transitioning = false;

                if (targetRotation == 90)
                {
                    EnterFlappy();
                }
                else
                {
                    EnterGalaga();
                }
            }

        }
    }

    private void EnterFlappy()
    {
        state = Mode.Flappy;

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        enemies.GetComponent<scr_enemies>().enabled = false;
        enemies.GetComponent<scr_flappy>().enabled = true;

        enemies.GetComponent<scr_flappy>().GeneratePipes(3);
    }

    private void EnterGalaga()
    {
        state = Mode.Galaga;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        enemies.GetComponent<scr_enemies>().enabled = true;
        enemies.GetComponent<scr_flappy>().enabled = false;

        //Just keep doing the same wave
        enemies.GetComponent<scr_enemies>().waveIndex--;
        enemies.GetComponent<scr_enemies>().SpawnNextWave();
        

        Invoke(nameof(BeginAgain), 2);
    }
}

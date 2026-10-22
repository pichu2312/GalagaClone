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
    [SerializeField] private float galSpd = 0;
    [SerializeField] private float breakSpd = 0;

    private float spd = 0;


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
    [SerializeField] public float scrollSpeedBase;
    private float scrollSpeed;
    [SerializeField] public Renderer renderer;
    [SerializeField] public GameObject background;

    [SerializeField] public GameObject camera;

    [SerializeField] public float transitionSpeed;
    [SerializeField] float flashLength = 0.2f;
    [SerializeField] float invLength = 1.2f;


    //private float transitionDir = -1;
    private float targetRotation;

    private float currentRotation;
    private bool transitioning = false;

    [SerializeField] public scr_flappy flappy;
    [SerializeField] public scr_breakout breakout;


    [SerializeField] float flapForce = 5f;

    public bool invulnerable = true;
    private Coroutine toggleCoroutine;

    [SerializeField] public float crossAmount = 16;
    [SerializeField] scr_crossy crossy;

    Ball ball;
    public enum Mode
    {
        Galaga,
        Flappy,
        Breakout,
        Crossy
    }

    private Mode newState;
    private Mode state  = Mode.Galaga;



    public Mode getState()
    {
        return this.state;
    }

    public void setState(Mode state)
    {
        this.state = state;
    }

    // Start is called before the first frame update
    void Awake()
    {
        y = transform.localPosition.y;
        z = transform.localPosition.z;

        Invoke(nameof(BeginAgain), 2);

        currentRotation = camera.transform.eulerAngles.z;
        spd = galSpd;

        scrollSpeed = scrollSpeedBase;
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
                case Mode.Breakout:
                    ProcessPlayerMovement();
                    ProcessPlayerFiringBall();
                    break;
                case Mode.Crossy:
                    ProcessCrossy();
                    break;

            }

        }


        DebugTransition();

    }

    private void DebugTransition()
    {
        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKey(KeyCode.T))
        {
            if (!transitioning)
            {
                BeginTransition(Mode.Galaga);
            }

        }

        if (Input.GetKey(KeyCode.Y))
        {
            if (!transitioning)
            {
                BeginTransition(Mode.Flappy);
            }

        }

        if (Input.GetKey(KeyCode.U))
        {
            if (!transitioning)
            {
                BeginTransition(Mode.Breakout);
            }

        }

        if (Input.GetKey(KeyCode.I))
        {
            if (!transitioning)
            {
                BeginTransition(Mode.Crossy);
            }

        }
    }
    public void ProcessPlayerMovement()
    {
        float move = 0;
        float speed;
        speed = (state == Mode.Galaga ? spd : -spd) * Time.deltaTime;

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
                BeginTransition(Mode.Flappy);
            }
        }

    }

    public void BallCollision(GameObject enemy, GameObject bullet)
    {
        //If true we've destroyed the enemy
        if (breakout.EnemyHit(enemy))
        {
            AddScore(enemy.GetComponent<Enemy>().scoreVal);
            EnemyDestroyed?.Invoke();

            //Check if everything is empty, as then it's new level time
            if (breakout.IsAllEnemiesGone())
            {
                /*
                NewLevel?.Invoke();
                Invoke(nameof(BeginAgain), 2);
                */
                ball.gameObject.SetActive(false);
                BeginTransition(Mode.Crossy);
            }
        }

    }

    public void ProcessPlayerFlying()
    {
        //Destroy if touching the side
        //actually its x value but shh
        float y = transform.position.x;

        if ((y < -4.5f) || (y > 4.5f) && (!transitioning))
        {
            DestroyPlayer();
        }
        else if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X))
        {
            //body.AddForce(Vector2.left * flapForce, ForceMode2D.Impulse);
            body.linearVelocity = new Vector2(-flapForce, body.linearVelocityY);
        }


    }

    public void ProcessPlayerFiringBall()
    {
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X))
        {
            ball.moving = true;
            ball.transform.SetParent(null);
        }
    }

    public void ProcessCrossy()
    {
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X))
        {
            crossy.CrossRoads();
        }

        float a = 0;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            a += 1;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            a -= 1;
        }

        transform.position = new Vector3(Math.Clamp(transform.position.x + a, -3, 3), y, z);
    }

    public void AddScore(int score)
    {
        this.score += score;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (active)
        {
            if (!invulnerable)
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
    }

    public void DestroyPlayer()
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
        else if(state == Mode.Breakout)
        {
            ResetBall();
        }

        invulnerable = true;

        toggleCoroutine = StartCoroutine(ToggleSprite());
        Invoke(nameof(DisableInv), 2f);
    }

    private void DeactivePlayer()
    {
        active = false;
        sprite.enabled = false;
        //set x to 0
        transform.position = new Vector3(0, y, z);

        body.bodyType = RigidbodyType2D.Kinematic
        ;
    }

    private void MoveBackground()
    {
        float y = Mathf.Repeat(Time.time * scrollSpeed, 1);
        Vector2 offset = new UnityEngine.Vector2(0, y);
        renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
    }

    public void BeginTransition(Mode mode)
    {
        Cleanup();

        switch (mode)
        {
            case Mode.Galaga: 
            if (currentRotation == 270) {currentRotation = -90;}
            targetRotation = 0; 
            break;
            case Mode.Flappy: targetRotation = 90; break;
            case Mode.Breakout: targetRotation = 180; break;
            case Mode.Crossy: 
            if (currentRotation == 0) {currentRotation = 360;}
            targetRotation = 270; 
            break;
        }

        newState = mode;
        transitioning = true;
    }

    //Called when transitiong to a new state removing all the old junk we no longer need
    private void Cleanup()
    {
        switch (state)
        {
            case Mode.Galaga: 
            enemies.Cleanup();
            break;    
            case Mode.Flappy:
            flappy.Cleanup();
            break;
            case Mode.Breakout:
            breakout.Cleanup();
            ball.gameObject.SetActive(false);
            break;
            case Mode.Crossy:
            crossy.Cleanup();
            break;
        }
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
                state = newState;
                EnableGameScript();

                switch (state)
                {
                    case Mode.Galaga: EnterGalaga(); break;
                    case Mode.Flappy: EnterFlappy(); break;
                    case Mode.Breakout: EnterBreakout(); break;
                    case Mode.Crossy: EnterCrossy(); break;
                }
            }

        }
    }

    private void EnterFlappy()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        enemies.GetComponent<scr_flappy>().GeneratePipes(3);
    }

    private void EnterGalaga()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

        //Just keep doing the same wave
        enemies.GetComponent<scr_enemies>().waveIndex--;
        enemies.GetComponent<scr_enemies>().SpawnNextWave();

        spd = galSpd;

        Invoke(nameof(BeginAgain), 2);
    }

    private void EnterBreakout()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

        breakout.Initialise();

        spd = breakSpd;
        
        ResetBall();
    }

    private void EnterCrossy()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        transform.position = new Vector3(0, y, z);
        crossy.GeneratePipes(3);
    }

    private void ResetBall()
    {
        GameObject b = scr_object_pool.SharedInstance.GetBall(new Vector2(transform.position.x, transform.position.y +1f));
        b.transform.SetParent(transform);
        ball = b.GetComponent<Ball>();
        breakout.ball = b.GetComponent<Ball>();
        ball.Initialise(breakout.ballMinSpd, breakout.ballMaxSpd);
    }

    private void EnableGameScript()
    {
        enemies.enabled = false;
        flappy.enabled = false;
        breakout.enabled = false;
        crossy.enabled = false;

        switch (state)
        {
            case Mode.Galaga: enemies.enabled = true; break;
            case Mode.Flappy: flappy.enabled = true; break;
            case Mode.Breakout: breakout.enabled = true; break;
            case Mode.Crossy: crossy.enabled = true; break;
        }
    }

    private void DisableInv()
    {
        invulnerable = false;
        sprite.enabled = true;
        StopCoroutine(toggleCoroutine);
    }

    IEnumerator ToggleSprite()
    {
        while (true)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(flashLength);
        }
    }
}


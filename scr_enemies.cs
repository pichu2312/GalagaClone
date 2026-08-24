using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class scr_enemies : MonoBehaviour
{
    float time = 0;

    //[SerializeField] List<Wave> waves = new();
    List<Wave> activeWaves = new();
    [SerializeField] GameObject enemyPrefab;

    [SerializeField] float enemySpacing = 1f;

    [SerializeField] GameObject backAndForth;

    //Moving back and forth
    const int enemyX = 10;
    const int enemyY = 4;
    [SerializeField] float gridSpacing = 1f;
    [SerializeField] float backAndForthSpeed = 0.1f;
    [SerializeField] float enemySpeed = 3f;

    const float yOffSet = 3.5f;
    int dir = 1;


    //Strafing
    bool strafing = false;
    const float bottomValue = -8f;



    //2D array storing enemies currentley moving back and forth at the top of the screen
    private Enemy[,] enemySpaces = new Enemy[enemyX, enemyY];

    //List that stores enemies moving towards the grid at the top of the screen
    List<Enemy> movingEnemies = new();

    [SerializeField] Scrobj_waves waveSpawner;

    [SerializeField] float bulletWaitTime;


    List<Wave> comingWaves = new();



    // Start is called before the first frame update
    void Start()
    {
        SpawnWaves();
    }

    private void SpawnWaves()
    {
        //Create every wave
        foreach (Wave w in waveSpawner.waves)
        {
            int count = 0;
            //Needed dude to serialisation
            w.Enemies = new List<Enemy>();
            foreach (int i in w.enemyTypes)
            {
                count++;
                //Enemy e = Instantiate(enemyPrefab, w.spawnPoint + (Vector2)(count * enemySpacing * Vector3.Normalize(w.spawnPoint)), Quaternion.identity).GetComponent<Enemy>();
                Enemy e = scr_object_pool.SharedInstance.GetEnemy(w.spawnPoint + (Vector2)(count * enemySpacing * Vector3.Normalize(w.spawnPoint))).GetComponent<Enemy>();

                //Initialise
                e.Initialise(count, enemySpeed, i);

                w.Enemies.Add(e);
            }
            comingWaves.Add(w);

            //Invoke the spawn, always add 2 to account for the player starting
            Invoke(nameof(SpawnWave), w.spawnTime + 2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        ControlWaves();
        ControlMovingEnemies();
        ControlBackAndForth();
        ControlStrafing();
    }

    private void SpawnWave()
    {
        //Invokes cant take parameters, so always perform the wave at the front of the list
        Wave wave = comingWaves[0];
        activeWaves.Add(wave);
        wave.Active = true;
        comingWaves.RemoveAt(0);
    }

    private void ControlWaves()
    {

        //Move every enemey aside form those inside the main chunk at the top
        for (int i = 0; i < activeWaves.Count; i++)
        {
            Wave w = activeWaves[i];

            //If greater than 0, then we have finished enemies to add to the main grid
            Enemy returnedEnemy = w.EnemyMovement();

            //An enemy in the wave is finished, begin transfer to main body
            if (returnedEnemy != null)
            {
                Vector3 enemyPosition = returnedEnemy.transform.position;

                //Check if the enemy is below the screen, if so teleport it up
                if (enemyPosition.y <= bottomValue)
                {
                    returnedEnemy.transform.position = new Vector3(enemyPosition.x, 6, enemyPosition.z);
                }

                AddToBody(returnedEnemy);


                //Check if there are no enemies in this wave to destroy it
                if (!w.CheckActive())
                {
                    activeWaves.Remove(w);
                    i--;
                }
            }
        }
    }


    private void ControlMovingEnemies()
    {
        for (int i = 0; i < movingEnemies.Count; i++)
        {
            if (movingEnemies[i].Move())
            {
                movingEnemies.RemoveAt(i);
                i--;
            }
        }
    }

    private void ControlBackAndForth()
    {
        //Make the back and forth, well, move back and forth!
        float speed = backAndForthSpeed * Time.deltaTime * dir;

        backAndForth.transform.position += new Vector3(speed, 0, 0);

        if ((backAndForth.transform.position.x < -1) || (backAndForth.transform.position.x > 1))
        {
            dir *= -1;
        }
    }

    private void ControlStrafing()
    {
        //If we have no waves left to spawn or to do their little run, begin spawning strafing enemies
        if ((comingWaves.Count == 0) && (activeWaves.Count == 0) && (movingEnemies.Count == 0))
        {
            strafing = false;
            if (!strafing)
            {
                List<Enemy> activeEnemies = new List<Enemy>();

                //Select, at most two enemies to begin strafing
                for (int i = 0; i < enemySpaces.GetLength(0); i++)
                {
                    for (int j = 0; j < enemySpaces.GetLength(1); j++)
                    {
                        Enemy e = enemySpaces[i, j];

                        if (e != null)
                        {
                            activeEnemies.Add(e);
                        }
                    }
                }

                //Check enemies are still alive to do this  
                if (activeEnemies.Count > 0)
                {
                    List<Enemy> strafingEnemies = new List<Enemy>();
                    int i1 = UnityEngine.Random.Range(0, activeEnemies.Count);
                    int i2 = -1;

                    strafingEnemies.Add(activeEnemies[i1]);

                    if (activeEnemies.Count > 1)
                    {
                        do
                        {
                            i2 = UnityEngine.Random.Range(0, activeEnemies.Count);
                        } while (i1 == i2);

                        strafingEnemies.Add(activeEnemies[i2]);
                    }

                    foreach (Enemy e in strafingEnemies)
                    {
                        e.transform.SetParent(transform);

                        List<Vector2> movePoints = new List<Vector2>
                    {
                        //Add three movepoints, one to the centre, one a bit more down, and one off the screen
                        new(0, 0),
                        new(Random.Range(-3, 3), -4),
                        new(Random.Range(-3, 3), bottomValue)

                    };


                        Wave w = new Wave(movePoints, new List<Enemy> { e });

                        activeWaves.Add(w);
                    }


                    strafingEnemies.Clear();
                    strafing = true;

                    //Invoke some firing to get bullets raining down
                    Invoke(nameof(FireBullet), 0.2f);
                    Invoke(nameof(FireBullet), 0.2f + bulletWaitTime);
                }

            }
        }
    }

    private void FireBullet()
    {
        //Fire a bullet from each enemy in the active wave
        for (int i = 0; i < activeWaves.Count; i++)
        {
            Wave w = activeWaves[i];
            foreach (Enemy e in w.Enemies)
            {
                GameObject newBullet = scr_object_pool.SharedInstance.GetLaser(e.transform.position);
                if (newBullet != null)
                {
                    newBullet.GetComponent<Bullet>().SetParent(1);
                }
            }
        }
    }

    private void AddToBody(Enemy e)
    {
        int halfX = enemyX / 2;
        int val = -1;
        int val2 = -1;
        bool finished = false;

        //Select a spot for the enemy to go
        for (int i = 0; i < halfX; i += 1)
        {
            for (int j = 0; j < enemyY; j++)
            {
                val = (halfX + i) % enemyX;
                val2 = j;

                if (enemySpaces[val, j] == null)
                {
                    finished = true;
                    break;
                }

                val = (halfX - (i + 1)) % enemyX;


                if (enemySpaces[val, j] == null)
                {
                    finished = true;
                    break;
                }
            }

            if (finished)
            {
                break;
            }
        }

        //Finished
        enemySpaces[val, val2] = e;


        //Then set position and parent
        e.SetMovePoint(new Vector3((val - halfX) * gridSpacing, (val2 - yOffSet) * gridSpacing, 0));
        e.transform.SetParent(backAndForth.transform);
        movingEnemies.Add(e);
    }

    public void DestroyEnemy(GameObject enemy)
    {
        Wave foundWave = null;
        Enemy foundEnemy = null;

        //Go through waves to find the enemy and destroy it
        foreach (Wave w in activeWaves)
        {
            foreach (Enemy e in w.Enemies)
            {
                if (e.gameObject == enemy)
                {
                    foundWave = w;
                    foundEnemy = e;
                    break;
                }
            }

            if (foundEnemy != null)
            {
                break;
            }
        }

        foreach (Enemy e in movingEnemies)
        {
            if (e.gameObject == enemy)
            {
                foundEnemy = e;
                break;
            }
        }

        //If it has 1 hit left then destroy it, elsewise, change it
        if (enemy.GetComponent<Enemy>().hits == 1)
        {
            //If it's not in a wave, it's in the grid, which we can just remove
            enemy.SetActive(false);

            if (foundEnemy != null)
            {
                if (foundWave != null)
                {
                    foundWave.Enemies.Remove(foundEnemy);

                    //Check if there are no enemies in this wave to destroy it
                    if (!foundWave.CheckActive())
                    {
                        activeWaves.Remove(foundWave);
                    }
                }
                //If there was an enemy but not a wave it must be in moving enemies
                else
                {
                    movingEnemies.Remove(foundEnemy);
                }
            }
        }
        else
        {
            enemy.GetComponent<Enemy>().DealHit();
        }




    }


}

[System.Serializable]
public class Wave
{
    public float spawnTime;
    public List<int> enemyTypes = new();

    private List<Enemy> enemies = new();

    public List<Enemy> Enemies
    {
        get { return enemies; }
        set { enemies = value; }
    }

    public List<Vector2> movePoints = new();

    public Vector2 spawnPoint;

    //private bool active = false;

    public bool Active { get; set; } = false;


    //Create a wave on the fly for strafing patterns
    public Wave(List<Vector2> movePoints, List<Enemy> enemies)
    {
        this.enemies = enemies;
        this.movePoints = movePoints;
    }

    public Enemy EnemyMovement()
    {

        Enemy finishedEnemy = null;

        foreach (Enemy e in enemies)
        {
            //First check if the enemy is still active
            if (e != null)
            {
                if (e.Move())
                {
                    e.moveIndex += 1;

                    //If the enemy has reached the end of its move points
                    if (e.moveIndex >= movePoints.Count)
                    {
                        //Should only ever been 1 at a time per wave
                        finishedEnemy = e;
                        e.moveIndex = 0;
                    }
                    else
                    {
                        e.SetMovePoint(movePoints[e.moveIndex]);
                    }
                }
            }
            else
            {
                finishedEnemy = e;
            }

        }

        //Handle removing the finished enemy
        if (finishedEnemy != null)
        {
            enemies.Remove(finishedEnemy);
        }

        return finishedEnemy;
    }

    public bool CheckActive()
    {
        if (enemies.Count == 0)
        {
            return false;
        }

        return true;
    }



}

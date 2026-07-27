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

    [SerializeField] List<Wave> waves = new();
    List<Wave> activeWaves = new();
    [SerializeField] GameObject enemyPrefab;

    [SerializeField] float enemySpacing = 1f;

    [SerializeField] GameObject backAndForth;

    //Moving back and forth
    GameObject movingBackandForth;
    const int enemyX = 10;
    const int enemyY = 4;
    [SerializeField] float gridSpacing = 1f;
    [SerializeField] float backAndForthSpeed = 0.1f;
    const float yOffSet = 3.5f;
    int dir = 1;


    //Strafing
    bool strafing = false;


    //2D array storing enemies currentley moving back and forth at the top of the screen
    public Enemy[,] enemySpaces = new Enemy[enemyX,enemyY];

    //List that stores enemies moving towards the grid at the top of the screen
    List<Enemy> movingEnemies = new();

    // Start is called before the first frame update
    void Start()
    {
        //Create every wave
        foreach (Wave w in waves)
        {
            int count = 0;
            //Needed dude to serialisation
            w.enemies = new List<Enemy>();
            foreach (int i in w.enemyTypes)
            {
                count++;
                w.enemies.Add(new Enemy(i, count - 1, Instantiate(enemyPrefab, w.spawnPoint + (Vector2) (count * enemySpacing * Vector3.Normalize(w.spawnPoint)), Quaternion.identity)));
            }

            w.enemyTypes.Clear();

            //Invoke the spawn
            Invoke(nameof(SpawnWave), w.spawnTime);
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
        Wave wave = waves[0];
        activeWaves.Add(wave);
        wave.active = true;
        waves.RemoveAt(0);
    }

    private void ControlWaves()
    {

        //Move every enemey aside form those inside the main chunk at the top
        for (int i = 0; i < activeWaves.Count; i++)
        {
            Wave w = activeWaves[i];

            //If greater than 0, then we have finished enemies to add to the main grid
            Enemy returnVal = w.EnemyMovement();

            //An enemy in the wave is finished, begin transfer to main body
            if (returnVal != null)
            {
                AddToBody(returnVal);

                //Check if there are no enemies in this wave to destroy it
                if (!w.active)
                {
                    w = null;
                    activeWaves.RemoveAt(i);
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
        if ((waves.Count == 0) && (activeWaves.Count == 0))
        {
            strafing = false;
            if (!strafing) {
                List<Enemy> activeEnemies = new List<Enemy>();

                //Select, at most two enemies to begin strafing
                for (int i = 0; i < enemySpaces.GetLength(0); i++)
                {
                    for (int j = 0; j < enemySpaces.GetLength(1); j++)
                    {
                        Enemy e = enemySpaces[i,j];

                        if (e != null)
                        {
                            activeEnemies.Add(e);
                        }
                    }
                }

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
                        new(Random.Range(-3, 3), -10)
                        
                    };


                    Wave w = new Wave(movePoints, new List<Enemy>{e});

                    activeWaves.Add(w);
                }

                strafing = true;
            }
        }
    }

    private void AddToBody(Enemy e)
    {
        int halfX = enemyX/2;
        int val = -1;
        int val2 = -1;
        bool finished = false;

        //Select a spot for the enemy to go
        for (int i = 0; i < halfX; i+=1)
        {
            for (int j = 0; j < enemyY; j++)
            {
                val = (halfX + i) % enemyX;
                val2 = j;

                if (enemySpaces[val,j] == null)
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


}

//Holds enemy script and everything
public class Enemy
{
    //Same for all
    public GameObject gameObject;

    //Different per class
    public int hits;
    public SpriteRenderer sprite;
    public Transform transform;

    public Vector3 movePoint;
    public int moveIndex = 0;

    //Set values
    const float speed = 3f;

    //public bool active = false;

    public int spawnIndex;

    public bool active = false;


    public Enemy(int type, int spawnIndex, GameObject gameObject)
    {
        this.gameObject = gameObject;
        sprite = gameObject.GetComponent<SpriteRenderer>();
        transform = gameObject.GetComponent<Transform>();
        this.spawnIndex = spawnIndex;

        switch (type)
        {
            case 0: 
            hits = 1;
            break;
        }

    }

    public bool Move()
    {
        if (transform != null)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, movePoint, speed * Time.deltaTime);

            if (transform.localPosition == movePoint)
            {
                return true;
            }

        }
        return false;
    }

    public void SetMovePoint(Vector3 m)
    {
        movePoint = m;
    }
}

[System.Serializable]
public class Wave
{
    public float spawnTime;
    public List<int> enemyTypes = new();

    public List<Enemy> enemies = new();
    public List<Vector2> movePoints = new();

    public Vector2 spawnPoint;

    public bool active = false;


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

                    if (e.moveIndex >= movePoints.Count)
                    {
                        //Should only ever been 1 at a time per wave
                        finishedEnemy = e;
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

            if (enemies.Count == 0)
            {
                active = false;
            }
        }

        return finishedEnemy;
    }

    public void activateAllEnemies()
    {
        foreach (Enemy e in enemies)
        {
            e.active = true;
        }
    }

    public int checkActiveEnemies()
    {
        int count = 0;

        foreach (Enemy e in enemies)
        {
            if (e.active) {count++;}
        }

        return count;
    }

}

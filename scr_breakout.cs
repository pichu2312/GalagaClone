    using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class scr_breakout : MonoBehaviour
{
    [SerializeField] float gridSpacing = 1f;
    const int enemyX = 5;
    const int enemyY = 4;
    private GameObject[,] enemySpaces = new GameObject[enemyX, enemyY];
    
    private int enemyNo = 0;
    private int enemyMax = enemyX * enemyY;

    public Ball ball;

    [SerializeField] public float yOffset;

    [SerializeField] public float ballMinSpd = 1f;
        [SerializeField] public float ballMaxSpd = 1f;




    // Update is called once per frame
    void Update()
    {
    }

    public void Initialise()
    {
        GenerateGrid();
        enemyNo = enemyMax;
    }

    public void GenerateGrid()
    {

        //Select a spot for the enemy to go
        for (int i = 0; i < enemyX; i += 1)
        {
            for (int j = 0; j < enemyY; j++)
            {
                GameObject e = scr_object_pool.SharedInstance.GetEnemy(new Vector3((i * gridSpacing) + (gridSpacing - 3), (j * gridSpacing) + yOffset, 0));
                enemySpaces[i, j] = e;
                e.GetComponent<Enemy>().Initialise(1, 1, 0);
            }
        }

    }

    public bool EnemyHit(GameObject enemy)
    {
        (int, int) pos = FindEnemyInGrid(enemy);
        GameObject obj = enemySpaces[pos.Item1, pos.Item2];
        Enemy e = obj.GetComponent<Enemy>();

        if (e.hits == 1)
        {
            obj.SetActive(false);
            enemySpaces[pos.Item1, pos.Item2] = null;
            enemyNo--;

            //Increase ball speed
            ball.SetSpeed(enemyNo, enemyMax);

            return true;
        }
        else
        {
            e.DealHit();
            return false;
        }
    }

    private (int, int) FindEnemyInGrid(GameObject findEnemy)
    {
        List<Enemy> activeEnemies = new List<Enemy>();

        //Select, at most two enemies to begin strafing
        for (int i = 0; i < enemySpaces.GetLength(0); i++)
        {
            for (int j = 0; j < enemySpaces.GetLength(1); j++)
            {
                GameObject e = enemySpaces[i, j];

                if (e != null)
                {
                    if (findEnemy == e)
                    {
                        return (i, j);
                    }
                }
            }
        }

        return (-1, -1);
    }

    public bool IsAllEnemiesGone()
    {
        return enemyNo <= 0;
    }

    public void Cleanup()
    {
         for (int i = 0; i < enemySpaces.GetLength(0); i++)
        {
            for (int j = 0; j < enemySpaces.GetLength(1); j++)
            {
                GameObject e = enemySpaces[i, j];

                if (e != null)
                {
                   e.SetActive(false);
                }
                else
                {
                    enemySpaces[i, j] = null;
                }
            }
        }
    }
 
}



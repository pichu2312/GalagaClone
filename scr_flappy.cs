using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class scr_flappy : MonoBehaviour
{
    private List<Pipe> pipes = new();
    [SerializeField] float startX;
    [SerializeField] float endX;
    [SerializeField] float startY;
    [SerializeField] float endY;
    [SerializeField] float spacing;
    [SerializeField] int missingEnemies;
    [SerializeField] int enemiesInPipe;

    [SerializeField] float pipeSpacing;
    [SerializeField] float pipeSpeed;

    [SerializeField] private scr_player player;






    // Update is called once per frame
    void Update()
    {
        MovePipes();
    }

    public void GeneratePipes(int num)
    {
        for (int i = 0; i < num; i++)
        {
            pipes.Add(GeneratePipe(startX + (i * pipeSpacing)));
        }
    }

    public Pipe GeneratePipe(float xPos)
    {

        int gap = Random.Range(0, enemiesInPipe - missingEnemies);
        List<float> yPos = new();

        //Generate a gap somewhere
        for (int i = 0; i < enemiesInPipe; i++)
        {
            if (i == gap)
            {
                i += missingEnemies;
            }
            else
            {
                yPos.Add(startY + (i * spacing));
            }
        }

        Pipe pipe = new()
        {
            xPos = xPos,
            yPos = yPos
        };


        return pipe;
    }

    private void MovePipes()
    {
        foreach (Pipe p in pipes)
        {
            p.xPos -= Time.deltaTime * pipeSpeed;

            if (p.active)
            {
                //Check if we should deactivate
                if (p.xPos < endX)
                {
                    p.Deactivate();
                    pipes.Remove(p);

                    //Then check if we should stop
                    if (pipes.Count == 0)
                    {
                        player.BeginTransition();
                    }

                    break;
                }
                else
                {
                    foreach (GameObject e in p.pipe)
                    {
                        e.transform.position = new Vector3( e.transform.position.x, p.xPos, e.transform.position.z);
                    }
                }

            }
            //If not active check if they need to be
            else
            {
                if (p.xPos < startX)
                {
                    p.active = true;

                    p.InitialiseEnemies();
                }
            }
        }
    }

}

public class Pipe
{
    public List<GameObject> pipe = new();
    public float xPos;
    public List<float> yPos;

    public bool active = false;

    public void InitialiseEnemies()
    {
        foreach (float f in yPos)
        {
            pipe.Add(scr_object_pool.SharedInstance.GetEnemy(new Vector3(f,xPos,1)));
        }
    }

    public void Deactivate()
    {
        foreach (GameObject e in pipe)
        {
            e.SetActive(false);
        }
    }

}

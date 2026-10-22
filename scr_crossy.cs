using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class scr_crossy : MonoBehaviour
{

    [SerializeField] scr_flappy flappy;
    private List<Pipe> pipes = new();
    [SerializeField] float startX = 9;
    [SerializeField] float endX = -8;
    float startY;
    float endY;
     float spacing;
     int missingEnemies;
     int enemiesInPipe;
     [SerializeField] float carSpeedItem1 = 0.5f;
    [SerializeField] float carSpeedItem2 = 2;


     [SerializeField] scr_player player;

    [SerializeField] int pipeSpacingRangeItem1 = 1;
    [SerializeField] int pipeSpacingRangeItem2 = 3;

    Pipe chasingPipe;
    [SerializeField] float chasingPipeSpd;

    private void Awake()
    {
        //Alot of the values here are same as flappy, so we'll just copy them
        startY = flappy.startY - 0.6f;
        endY = flappy.endY + 0.6f;
        spacing = flappy.spacing;
        missingEnemies = flappy.missingEnemies;
        enemiesInPipe = flappy.enemiesInPipe + 2;

        if (player != null)
        {
            player.PlayerDestroyed += ResetChasingPipe;
        }
    }

    void Update()
    {
        MoveCars();
        PipeChase();
    }

    public void GeneratePipes(int num)
    {
        int pos = 0;
        for (int i = 0; i < num; i++)
        {
            pos += 1 + Random.Range(pipeSpacingRangeItem1, pipeSpacingRangeItem2);
            pipes.Add(GeneratePipe(startX + pos));
        }

        //Then create the chasing pipe
        List<float> yPos = new();

        //Generate a gap somewhere
        for (int i = 0; i < enemiesInPipe; i++)
        {
            yPos.Add(startY + (i * spacing));
        }

        chasingPipe = new()
        {
            xPos = endX,
            yPos = yPos,
        };

        chasingPipe.InitialiseEnemies();
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
            yPos = yPos,
            posOrNeg = Random.Range(0,2) == 0 ? -1 : 1,
            carSpeed = Random.Range(carSpeedItem1, carSpeedItem2)
        };

        if (pipe.xPos < startX)
        {
            pipe.active = true;

            pipe.InitialiseEnemies();
        }
        return pipe;
    }

    private void MoveCars()
    {
        float moveVal;
        foreach (Pipe p in pipes)
        {
            moveVal = Time.deltaTime * p.carSpeed;

            if (p.active)
            {
                    foreach (GameObject e in p.pipe)
                    {
                        //Move them up and down
                        e.transform.position = new Vector3( e.transform.position.x + (moveVal * p.posOrNeg), e.transform.position.y, e.transform.position.z);
                        if ((e.transform.position.x > endY) && (p.posOrNeg == 1))
                        {
                            e.transform.position = new Vector3(startY, e.transform.position.y,  e.transform.position.z);
                        }
                        else if ((e.transform.position.x < startY) && (p.posOrNeg == -1))
                        {
                            e.transform.position = new Vector3(endY, e.transform.position.y,  e.transform.position.z);
                        }
                    }
            }
        }
    }

    private void PipeChase()
    {
        chasingPipe.xPos -= Time.deltaTime * chasingPipeSpd;

        chasingPipe.UpdateEnemieX();
    }

    private void ResetChasingPipe()
    {
        if (player.getState() == scr_player.Mode.Crossy)
        {
            chasingPipe.xPos = endX;

            chasingPipe.UpdateEnemieX();
        }

    }

    public void CrossRoads()
    {
        foreach (Pipe p in pipes)
        {
            p.xPos -= player.crossAmount;
            if (p.active)
            {
                foreach (GameObject e in p.pipe)
                {
                    e.transform.position = new Vector3(e.transform.position.x, p.xPos, e.transform.position.z);
                }

                //Check if we should deactivate
                if (p.xPos < endX)
                {
                    p.Deactivate();
                    pipes.Remove(p);

                    //Then check if we should stop
                    if (pipes.Count == 0)
                    {
                        player.BeginTransition(scr_player.Mode.Galaga);
                    }

                    break;
                }
            }
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

    public void Cleanup()
    {
        foreach (Pipe p in pipes)
        {
            p.Deactivate();
            pipes.Remove(p);
        }

        chasingPipe.Deactivate();

    }
}

using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Ball : Bullet
{
    float Xdir = 1;
    float Ydir = 1;

    float maxX = 3;

    public bool moving = false;

    float ballBaseSpd = 1;
    float ballMaxSpd = 5;
    float ballSpd = 1;
    
    
    void Update()
    {
        if (moving) {
            transform.position += new Vector3(Xdir * ballSpd, Ydir * ballSpd,0);

            //Remove the bullet if it's gone past the point of no return
            if (transform.position.y > maxY)
            {
                Ydir *= -1;
            }
            else if (transform.position.y < minY)
            {
                //Destroy player
                moving = false;
                gameObject.SetActive(false);
                playerScript.DestroyPlayer();
            }

            if ((transform.position.x < -maxX) || (transform.position.x > maxX))
            {
                Xdir *= -1;
            }


        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided) { return; }

        if (other.TryGetComponent(out Enemy enemy))
        {
            playerScript.BallCollision(other.gameObject, this.gameObject);
            Ydir *= -1;
        }
        else if (other.TryGetComponent(out scr_player player))
        {
            Ydir*= -1;
        }

        hasCollided = true;
    }

    public void SetSpeed(int enemyNo, int enemyMax)
    {
        ballSpd = Mathf.Lerp(ballBaseSpd, ballMaxSpd, enemyNo/enemyMax);
    }

    public void ResetSpeed()
    {
        ballSpd = ballBaseSpd;
    }

    public void ResetDir()
    {
        Ydir = Math.Abs(Ydir);
    }

    public void Initialise(float ballBaseSpd, float ballMaxSpd)
    {
        this.ballBaseSpd = ballBaseSpd;
        this.ballMaxSpd = ballMaxSpd;
        moving = false;
        ResetSpeed();
        ResetDir();
    }
}
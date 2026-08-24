using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Holds enemy script and everything
public class Enemy : MonoBehaviour
{
    //Same for all

    //Different per class
    public int hits;
    public SpriteRenderer sprite;

    public Vector3 movePoint;
    public int moveIndex = 0;

    //Set values
    float speed = 3f;

    //public bool active = false;

    public int spawnIndex;

    public bool active = false;

    public int type = 0;

    public int scoreVal = 0;

    public List<Sprite> sprites;
    public List<Sprite> otherSprites;



    public void Initialise(int spawnIndex, float speed, int type)
    {
        sprite = GetComponent<SpriteRenderer>();
        this.spawnIndex = spawnIndex;
        this.speed = speed;
        this.type = type;

        switch (type)
        {
            case 0: 
            hits = 1;
            scoreVal = 100;
            sprite.sprite = sprites[0];
            break;
            case 1:
            hits = 2;
            scoreVal = 250;
            sprite.sprite = sprites[1];
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

    public void DealHit()
    {
        hits--;
        switch (type)
        {
            case 1: 
            sprite.sprite = otherSprites[0];
            break;
        }
    }
}

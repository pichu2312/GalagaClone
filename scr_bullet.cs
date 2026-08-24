using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;

    [SerializeField] scr_player playerScript;
    [SerializeField] scr_enemies enemiesScript;
    bool hasCollided = false;
    bool destroy = false;
    public int type = 0;

    [SerializeField] float bulletBasedSpd;
    float bulletSpd;
    [SerializeField] float maxY;
    [SerializeField] float minY;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        enemy = GameObject.FindWithTag("Enemies");

        playerScript = player.GetComponent<scr_player>();
        enemiesScript = enemy.GetComponent<scr_enemies>();

    }

    //Is this a player bullet or an enemy bullet?
    public void SetParent(int type)
    {
        this.type = type;
        switch (type)
        {
            case 0:
                bulletSpd = bulletBasedSpd;
                break;
            case 1:
                bulletSpd = -bulletBasedSpd;

                break;
            case 2:

                break;
        }

    }

    void Update()
    {
        transform.position += new Vector3(0, bulletSpd,0);

        //Remove the bullet if it's gone past the point of no return
        if (transform.position.y > maxY || transform.position.y < minY)
        {
            gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided) { return; }

        if (type == 0)
        {
            if (other.TryGetComponent(out Enemy enemy))
            {
                playerScript.BulletCollision(other.gameObject, this.gameObject);
                            destroy = true;

            }

            hasCollided = true;
        }
    }

    void LateUpdate()
    {
        hasCollided = false;
        if (destroy)
        {
            destroy = false;
            gameObject.SetActive(false);

        }

    }
}

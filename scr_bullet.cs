using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    [SerializeField] GameObject parent;
    scr_player parentScript;
    bool hasCollided = false;

    void Start()
    {
        parent = GameObject.FindWithTag("Player");
        parentScript = parent.GetComponent<scr_player>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided) {return;}

        if (other.TryGetComponent(out scr_enemy enemy))
        {
            parentScript.BulletCollision(other.gameObject, this.gameObject);
        }   

        hasCollided = true;
    }

    void LateUpdate()
    {
        hasCollided = false;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class scr_enemies : MonoBehaviour
{
    float time = 0;

    [SerializeField] List<Wave> waves = new();
    List<Wave> activeWaves = new();
    [SerializeField] GameObject enemyPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //Create every wave
        foreach (Wave w in waves)
        {
            foreach (int i in w.enemyTypes)
            {
                w.enemies.Add(new Enemy(i, Instantiate(enemyPrefab)));
            }

            w.enemyTypes.Clear();
        }
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
    }

}

//Holds enemy script and everything
public class Enemy
{
    //Same for all
    GameObject gameObject;

    //Different per class
    int hits;
    SpriteRenderer sprite;
    Transform transform;


    public Enemy(int type, GameObject gameObject)
    {
        this.gameObject = gameObject;
        sprite = gameObject.GetComponent<SpriteRenderer>();
        transform = gameObject.GetComponent<Transform>();

        switch (type)
        {
            case 0: 
            hits = 1;
            break;
        }

    }
}

[System.Serializable]
public class Wave
{
    public float spawnTime;
    public List<int> enemyTypes = new();

    public List<Enemy> enemies = new();
    public List<float> movePointsX = new();
    public List<float> movePointsY = new();

    public bool active = false;
    


}

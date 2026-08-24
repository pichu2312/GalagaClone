using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


[CreateAssetMenu(fileName = "scrobj_waves", menuName = "Scriptable Objects/scrobj_waves")]
public class Scrobj_waves : ScriptableObject
{
    /*
        public Vector2[] spawnPoints;
        public float[] spawnTime;
        public List<Vector2> movePoints = new();
        public List<int> enemyTypes = new();
        */

    public List<Wave> waves = new();
}

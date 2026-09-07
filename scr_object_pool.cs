    using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


public class scr_object_pool  : MonoBehaviour
{
    public static scr_object_pool SharedInstance;

    [Tooltip("Pooling for various objects starting; enemies, enemy lasers, player lasers")]
    public List<GameObject>[] pooledObjects = new List<GameObject>[2];
    public GameObject[] objectToPool;
    public int[] amountToPool;


    void Awake()
    {
        SharedInstance = this;

        for (int i = 0; i < amountToPool.Length; i++)
        {
            pooledObjects[i] = new List<GameObject>();
            GameObject tmp;
            for (int j = 0; j < amountToPool[i]; j++)
            {
                tmp = Instantiate(objectToPool[i]);
                tmp.SetActive(false);
                pooledObjects[i].Add(tmp);
            }
        }
    }

    private GameObject GetPooledObject(int type, Vector3 position)
    {
        for(int i = 0; i < amountToPool[type]; i++)
        {
            GameObject foundObject = null;

            if(!pooledObjects[type][i].activeInHierarchy)
            {
                foundObject = pooledObjects[type][i];
                foundObject.SetActive(true);
                foundObject.transform.position = position;
                return foundObject;
            }
            }
        return null;
    }


    public GameObject GetEnemy(Vector3 position)
    {
        return GetPooledObject(0, position);
    }


    
    public GameObject GetLaser(Vector3 position)
    {
        return GetPooledObject(1, position);
    }
}

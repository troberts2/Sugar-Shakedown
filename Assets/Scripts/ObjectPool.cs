using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;
    public GameObject bulletPrefab;
    public List<GameObject> pooledObjects;
    public int countToPool = 100;
    public bool canExpand = false;

    void Awake(){
        if(instance == null){
            instance = this;
        }
        pooledObjects = new List<GameObject>();
        for(int i = 0; i < countToPool; i++){
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject(){
        for(int i = 0; i < pooledObjects.Count; i ++){
            if(!pooledObjects[i].activeInHierarchy){
                return pooledObjects[i];
            }
        }
        if(canExpand){
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            pooledObjects.Add(obj);
            return obj;
        }
        return null;
    }
}

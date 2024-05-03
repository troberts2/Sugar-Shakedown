using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;

public class NavMeshManager : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;

    public void BakeNavMesh(){
        if(navMeshSurface != null){
            navMeshSurface.BuildNavMesh();
        }
    }
}

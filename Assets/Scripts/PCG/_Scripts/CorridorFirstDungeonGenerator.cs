using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using Cinemachine;
using UnityEngine.Events;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    //PCG parameters
    [SerializeField]
    private int corridorLength = 14, corridorCount = 5;
    [SerializeField]
    [Range(0.1f,1)]
    private float roomPercent = 0.8f;

    //PCG Data
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary 
        = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    
    private HashSet<Vector2Int> floorPositions, corridorPositions, bossRoomPositions;
    private Vector2Int bossRoomCenter;

    private DungeonData data;

    //Gizmos Data
    private List<Color> roomColors = new List<Color>();
    [SerializeField]
    private bool showRoomGizmo = false, showCorridorsGizmo;

    //Events
    public UnityEvent<DungeonData> OnDungeonFloorReady;

    protected override void RunProceduralGeneration()
    {
        CorridorFirstGeneration();
        data = new DungeonData
        {
            roomsDictionary = this.roomsDictionary,
            corridorPositions = this.corridorPositions,
            floorPositions = this.floorPositions,
            bossRoomCenter = this.bossRoomCenter,
            bossRoomPositions = this.bossRoomPositions
        };
        OnDungeonFloorReady?.Invoke(data);
    }

    private void CorridorFirstGeneration()
    {
        floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();

        CreateCorridors(floorPositions, potentialRoomPositions);

        //tilemapVisualizer.PaintFloorTiles(floorPositions);
        //WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);

        GenerateRooms(potentialRoomPositions);
        //StartCoroutine(GenerateRoomsCoroutine(potentialRoomPositions));
    }

    private void GenerateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions, true);

        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        CreateRoomsAtDeadEnd(deadEnds, roomPositions);

        floorPositions.UnionWith(roomPositions);

        tilemapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }

    private IEnumerator GenerateRoomsCoroutine(HashSet<Vector2Int> potentialRoomPositions)
    {
        yield return new WaitForSeconds(2);
        tilemapVisualizer.Clear();
        GenerateRooms(potentialRoomPositions);
        DungeonData data = new DungeonData
        {
            roomsDictionary = this.roomsDictionary,
            corridorPositions = this.corridorPositions,
            floorPositions = this.floorPositions
        };
        OnDungeonFloorReady?.Invoke(data);
    }

    private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        foreach (var position in deadEnds)
        {
            if(roomFloors.Contains(position) == false)
            {
                var room = RunRandomWalk(randomWalkParameters, position);
                SaveRoomData(position, room);
                roomFloors.UnionWith(room);
            }
        }
    }

    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        foreach (var position in floorPositions)
        {
            int neighboursCount = 0;
            foreach (var direction in Direction2D.cardinalDirectionsList)
            {
                if (floorPositions.Contains(position + direction))
                    neighboursCount++;
                
            }
            if (neighboursCount == 1)
                deadEnds.Add(position);
        }
        return deadEnds;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions, bool generateBossRoom)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

        List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();
        ClearRoomData();

        foreach (var roomPosition in roomsToCreate)
        {
            // Check if boss room should be generated and if it hasn't been generated yet
            if (!generateBossRoom || roomPosition != roomsToCreate.Last())
            {
                // Generate regular room
                var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);
                SaveRoomData(roomPosition, roomFloor);
                roomPositions.UnionWith(roomFloor);
                
            }
            if(generateBossRoom){
                Vector2Int bossRoomPosition = roomsToCreate.Last();
                HashSet<Vector2Int> bossRoomFloor = GenerateBossRoom(bossRoomPosition);
                roomPositions.UnionWith(bossRoomFloor);
            }
        }
        return roomPositions;
    }
    private HashSet<Vector2Int> GenerateBossRoom(Vector2Int roomPos)
    {

        Vector2Int bossRoomCenterPos = roomPos;

        // Define the size of the boss room (e.g., 5x5)
        int roomSize = 15; // Adjust size as needed


        HashSet<Vector2Int> bossRoomFloor = new HashSet<Vector2Int>();

        // Calculate the corner positions of the boss room
        int halfSize = roomSize / 2;
        Vector2Int bossRoomBottomLeft = bossRoomCenterPos - new Vector2Int(halfSize, halfSize);
        Vector2Int bossRoomTopRight = bossRoomCenterPos + new Vector2Int(halfSize, halfSize);

        // Generate the boss room floor
        for (int x = bossRoomBottomLeft.x; x <= bossRoomTopRight.x; x++)
        {
            for (int y = bossRoomBottomLeft.y; y <= bossRoomTopRight.y; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                bossRoomFloor.Add(position);
            }
        }
        bossRoomCenter = bossRoomCenterPos;
        bossRoomPositions = bossRoomFloor;
        

        return bossRoomFloor;
    }



    private void ClearRoomData()
    {
        roomsDictionary.Clear();
        roomColors.Clear();
    }

    private void SaveRoomData(Vector2Int roomPosition, HashSet<Vector2Int> roomFloor)
    {
        roomsDictionary[roomPosition] = roomFloor;
        roomColors.Add(UnityEngine.Random.ColorHSV());
    }

    private void CreateCorridors(HashSet<Vector2Int> floorPositions, 
        HashSet<Vector2Int> potentialRoomPositions)
    {
        var currentPosition = startPosition;
        potentialRoomPositions.Add(currentPosition);

        for (int i = 0; i < corridorCount; i++)
        {
            var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
            currentPosition = corridor[corridor.Count - 1];
            potentialRoomPositions.Add(currentPosition);
            floorPositions.UnionWith(corridor);
        }
        corridorPositions = new HashSet<Vector2Int>(floorPositions);
    }

    private void OnDrawGizmosSelected()
    {
        if (showRoomGizmo)
        {
            int i = 0;
            foreach (var roomData in roomsDictionary)
            {
                Color color = roomColors[i];
                color.a = 0.5f;
                Gizmos.color = color;
                Gizmos.DrawSphere((Vector2)roomData.Key, 0.5f);
                foreach (var position in roomData.Value)
                {
                    Gizmos.DrawCube((Vector2)position + new Vector2(0.5f,0.5f), Vector3.one);
                }
                i++;
            }
        }
        if (showCorridorsGizmo && corridorPositions != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var corridorTile in corridorPositions)
            {
                Gizmos.DrawCube((Vector2)corridorTile + new Vector2(0.5f, 0.5f), Vector3.one);
            }
        }
    }
}

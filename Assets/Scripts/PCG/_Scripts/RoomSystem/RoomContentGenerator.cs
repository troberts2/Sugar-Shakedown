//using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class RoomContentGenerator : MonoBehaviour
{
    [SerializeField]
    private RoomGenerator playerRoom, defaultRoom, bossRoom, lootRoom;

    List<GameObject> spawnedObjects = new List<GameObject>();

    [SerializeField]
    private GraphTest graphTest;

    [Min(0)]
    [SerializeField] private int numOfLootRooms = 2; 
    [SerializeField]GameObject bossPrefab;


    public Transform itemParent;

    [SerializeField]
    //private CinemachineVirtualCamera cinemachineCamera;

    public UnityEvent RegenerateDungeon;
    public UnityEvent FinishedGeneratingContent;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var item in spawnedObjects)
            {
                Destroy(item);
            }
            RegenerateDungeon?.Invoke();
        }
    }
    public void GenerateRoomContent(DungeonData dungeonData)
    {
        foreach (GameObject item in spawnedObjects)
        {
            DestroyImmediate(item);
        }
        spawnedObjects.Clear();

        SelectPlayerSpawnPoint(dungeonData);
        SelectLootRooms(dungeonData);
        GenerateBossRoom(dungeonData);
        SelectEnemySpawnPoints(dungeonData);

        foreach (GameObject item in spawnedObjects)
        {
            if(item != null)
                item.transform.SetParent(itemParent, false);
        }
        FinishedGeneratingContent?.Invoke();
    }

    private void SelectPlayerSpawnPoint(DungeonData dungeonData)
    {
        int randomRoomIndex = UnityEngine.Random.Range(0, dungeonData.roomsDictionary.Count);
        Vector2Int playerSpawnPoint = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);

        graphTest.RunDijkstraAlgorithm(playerSpawnPoint, dungeonData.floorPositions);

        Vector2Int roomIndex = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);

        List<GameObject> placedPrefabs = playerRoom.ProcessRoom(
            playerSpawnPoint,
            dungeonData.roomsDictionary.Values.ElementAt(randomRoomIndex),
            dungeonData.GetRoomFloorWithoutCorridors(roomIndex)
            );

        FocusCameraOnThePlayer(placedPrefabs[placedPrefabs.Count - 1].transform);

        spawnedObjects.AddRange(placedPrefabs);

        dungeonData.roomsDictionary.Remove(playerSpawnPoint);
    }

    private void FocusCameraOnThePlayer(Transform playerTransform)
    {
        //cinemachineCamera.LookAt = playerTransform;
        //cinemachineCamera.Follow = playerTransform;
    }

    private void SelectEnemySpawnPoints(DungeonData dungeonData)
    {
        foreach (KeyValuePair<Vector2Int,HashSet<Vector2Int>> roomData in dungeonData.roomsDictionary)
        { 
            spawnedObjects.AddRange(
                defaultRoom.ProcessRoom(
                    roomData.Key,
                    roomData.Value, 
                    dungeonData.GetRoomFloorWithoutCorridors(roomData.Key)
                    )
            );

        }
    }
    private void SelectLootRooms(DungeonData dungeonData)
    {
        List<Vector2Int> lootRoomSpawnPoints = new List<Vector2Int>();
        for (int i = 0; i < numOfLootRooms; i++)
        {
            int randomRoomIndex = UnityEngine.Random.Range(0, dungeonData.roomsDictionary.Count);
            lootRoomSpawnPoints.Add(dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex));
            Vector2Int roomIndex = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);
            List<GameObject> placedPrefabs = lootRoom.ProcessRoom(
            lootRoomSpawnPoints[i],
            dungeonData.roomsDictionary.Values.ElementAt(randomRoomIndex),
            dungeonData.GetRoomFloorWithoutCorridors(roomIndex)
            );
            spawnedObjects.AddRange(placedPrefabs);
            dungeonData.roomsDictionary.Remove(roomIndex);
        }
    }

    private void GenerateBossRoom(DungeonData dungeonData)
    {
        // Choose a random room index
        int randomRoomIndex = UnityEngine.Random.Range(0, dungeonData.roomsDictionary.Count);
        Vector2Int bossRoomCenter = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);

        // Define the size of the boss room (e.g., 5x5)
        int roomSize = 5; // Adjust size as needed

        // Calculate the corner positions of the boss room
        Vector2Int bossRoomBottomLeft = new Vector2Int(bossRoomCenter.x - roomSize / 2, bossRoomCenter.y - roomSize / 2);
        Vector2Int bossRoomTopRight = new Vector2Int(bossRoomCenter.x + roomSize / 2, bossRoomCenter.y + roomSize / 2);

        // Generate the boss room floor
        HashSet<Vector2Int> bossRoomFloor = new HashSet<Vector2Int>();
        for (int x = bossRoomBottomLeft.x; x <= bossRoomTopRight.x; x++)
        {
            for (int y = bossRoomBottomLeft.y; y <= bossRoomTopRight.y; y++)
            {
                bossRoomFloor.Add(new Vector2Int(x, y));
            }
        }

        // Process the boss room
        List<GameObject> bossRoomObjects = bossRoom.ProcessRoom(
            bossRoomCenter,
            bossRoomFloor,
            bossRoomFloor // Use bossRoomFloor as both room floor and room walls
        );


        // Add boss room objects to the list of spawned objects
        spawnedObjects.AddRange(bossRoomObjects);

        // Remove the boss room from the dungeon data
        dungeonData.roomsDictionary.Remove(bossRoomCenter);
    }


}

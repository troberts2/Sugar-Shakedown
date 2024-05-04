using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : RoomGenerator
{
    public List<GameObject> bossList;

    public List<ItemPlacementData> itemData;

    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public override List<GameObject> ProcessRoom(
        Vector2Int roomCenter, 
        HashSet<Vector2Int> roomFloor, 
        HashSet<Vector2Int> roomFloorNoCorridors)
    {

        ItemPlacementHelper itemPlacementHelper = 
            new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        List<GameObject> placedObjects = 
            prefabPlacer.PlaceAllItems(itemData, itemPlacementHelper);

        Vector2Int playerSpawnPoint = roomCenter;

        GameObject bossToSpawn = bossList[UnityEngine.Random.Range(0, bossList.Count)];

        GameObject boss 
            = prefabPlacer.CreateObject(bossToSpawn, playerSpawnPoint + new Vector2(0.5f, 0.5f));
 
        placedObjects.Add(boss);

        return placedObjects;
    }
}

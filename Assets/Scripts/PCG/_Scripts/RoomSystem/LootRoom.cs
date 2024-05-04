using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootRoom : RoomGenerator
{
    public GameObject chest;

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

        GameObject lootChest 
            = prefabPlacer.CreateObject(chest, playerSpawnPoint + new Vector2(0.5f, 0.5f));
 
        placedObjects.Add(lootChest);

        return placedObjects;
    }
}



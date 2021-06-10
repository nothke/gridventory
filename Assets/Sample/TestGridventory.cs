using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Inventory;

public class TestGridventory : MonoBehaviour
{
    public float separation = 0.1f;
    public int width = 8;
    public int height = 8;

    Gridventory gridventory;

    public List<TestGridventoryItem> items;

    int rotation = 0;

    void Start()
    {
        gridventory = new Gridventory(width, height);
    }

    void Update()
    {
        Vector2 v = Gridventory.GetInventoryPositionFromRay(Camera.main.ScreenPointToRay(Input.mousePosition), transform);
        Vector2Int tile = Gridventory.GetInventoryTileFromLocalPosition(v, separation);

        if (items.Count != 0)
        {
            // Rotate
            if (Input.GetKeyDown(KeyCode.R))
                rotation = (rotation + 1) % 4;

            TestGridventoryItem testItem = items[0];

            Vector2Int itemSize = rotation % 2 == 0 ? testItem.size : new Vector2Int(testItem.size.y, testItem.size.x);

            Vector2Int itemRootTile = Gridventory.GetRootTileFromLocalPosition(v, itemSize, gridventory.size, separation);

            // Add on left click
            if (Input.GetMouseButton(0))
            {
                //gridventory.TryAdd(new RectInt(tile, itemSize));
                if (gridventory.TryInsert(testItem, itemRootTile, rotation))
                {
                    Vector3 itemPos = Gridventory.WorldPositionFromInventoryRect(
                        new RectInt(itemRootTile, itemSize), transform, separation);
                    testItem.transform.position = itemPos;

                    testItem.transform.rotation = Gridventory.WorldRotationFromInventoryRect(rotation, transform.forward, transform.right);

                    items.RemoveAt(0);
                }
            }

            // Drawing:
            Color placeColor = gridventory.IsOccupied(new RectInt(itemRootTile, itemSize)) ?
                Color.red : Color.green;

            Gridventory.DrawRect(new RectInt(itemRootTile, itemSize), transform.position, transform.forward, transform.right, separation, 0.02f, placeColor);
        }

        // Remove on right click
        if (Input.GetMouseButton(1))
        {
            if (gridventory.TryRemoveItemAt(tile, out IGridventoryItem item))
            {
                var testItem = item as TestGridventoryItem;
                items.Add(testItem);
                testItem.ReturnToOriginalLocation();
                Debug.Log("Removed at " + tile);
            }
        }

        gridventory.DebugDrawInventory(transform.position, transform.forward, transform.right, separation);
        gridventory.DrawInventoryTile(tile.x, tile.y, transform.position, transform.forward, transform.right, separation, 0.01f, Color.red);
    }
}

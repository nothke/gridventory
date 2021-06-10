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
    Stack<TestGridventoryItem> itemsStack;

    int rotation = 0;

    void Start()
    {
        gridventory = new Gridventory(width, height);

        itemsStack = new Stack<TestGridventoryItem>(items);
    }

    void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector2 invPos = Gridventory.GetInventoryPositionFromRay(mouseRay, transform);
        Vector2Int tile = Gridventory.GetInventoryTileFromLocalPosition(invPos, separation);

        if (itemsStack.Count != 0)
        {
            // Rotate
            if (Input.GetKeyDown(KeyCode.R))
                rotation = (rotation + 1) % 4;

            TestGridventoryItem testItem = itemsStack.Peek();


            Vector2Int itemSize = Gridventory.RotatedItemSize(testItem.size, rotation);

            Vector2Int itemRootTile = Gridventory.GetRootTileFromLocalPosition(invPos, itemSize, gridventory.size, separation);

            // Add on left click
            if (Input.GetMouseButton(0))
            {
                if (gridventory.TryInsert(testItem, itemRootTile, testItem.size, rotation))
                {
                    // Place the item in the inventory
                    testItem.transform.SetPositionAndRotation(
                        Gridventory.WorldPositionFromInventoryRect(
                            new RectInt(itemRootTile, itemSize), transform, separation),
                        Gridventory.GetItemWorldRotation(
                            rotation, transform.forward, transform.right));

                    itemsStack.Pop();
                }
            }

            // Drawing:
            Color placeColor = gridventory.IsOccupied(new RectInt(itemRootTile, itemSize)) ?
                Color.red : Color.green;

            Gridventory.DebugDrawRect(new RectInt(itemRootTile, itemSize), transform.position, transform.forward, transform.right, separation, 0.02f, placeColor);
        }

        // Remove on right click
        if (Input.GetMouseButton(1))
        {
            if (gridventory.TryRemoveItemAt(tile, out object item, out RectInt _, out int _rot))
            {
                var testItem = item as TestGridventoryItem;
                testItem.ReturnToOriginalLocation();
                Debug.Log("Removed at " + tile);

                rotation = _rot;
                itemsStack.Push(testItem);
            }
        }

        // Draw inventory and current tile
        gridventory.DebugDrawInventory(transform.position, transform.forward, transform.right, separation);
        gridventory.DebugDrawTile(tile.x, tile.y, transform.position, transform.forward, transform.right, separation, 0.03f, Color.yellow);
    }
}

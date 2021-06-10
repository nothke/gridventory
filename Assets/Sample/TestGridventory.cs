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
        // Create a ray from camera towards the mouse position
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Cast a ray into the inventory, and use the local inventory position to find the tile we are pointing at
        Vector2 invPos = Gridventory.GetInventoryPositionFromRay(mouseRay, transform);
        Vector2Int tile = Gridventory.GetInventoryTileFromLocalPosition(invPos, separation);

        // Draw the inventory and current tile
        gridventory.DebugDrawInventory(transform.position, transform.forward, transform.right, separation);
        gridventory.DebugDrawTile(tile.x, tile.y, transform.position, transform.forward, transform.right, separation, 0.03f, Color.yellow);

        if (itemsStack.Count != 0)
        {
            // Press R to rotate the target item rect.
            // We use the 0-3 rotation integer to pass it into inventory as a 90 degree multiplier.
            if (Input.GetKeyDown(KeyCode.R))
                rotation = (rotation + 1) % 4;

            TestGridventoryItem testItem = itemsStack.Peek();

            // Find the target item rect root tile so that we can use it for insertion calculation
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

            // Draw the rect debug lines, make it green if the space is free, or red if it's occupied
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

        
    }
}

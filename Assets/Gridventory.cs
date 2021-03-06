using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Inventory
{
    public class Gridventory
    {
        public readonly Vector2Int size;
        int[] taken; // Only used for checkup and colors, does not actually correspond to index

        List<SlottedItem> items;
        int current;

        readonly struct SlottedItem
        {
            readonly public object item;
            readonly public RectInt rect;
            readonly public int rotation;

            public SlottedItem(object item, RectInt rect, int rotation)
            {
                this.item = item;
                this.rect = rect;
                this.rotation = rotation;
            }
        }

        public Gridventory(int width, int height, int itemsCapacity = 4)
        {
            if (width <= 0 || height <= 0)
                throw new System.Exception("Gridventory does not support 0 or negative sized inventories");

            size = new Vector2Int(width, height);

            taken = new int[width * height];
            items = new List<SlottedItem>(itemsCapacity);
        }

        public bool IsOccupied(int x, int y)
        {
            return taken[y * size.x + x] > 0;
        }

        public bool IsOccupied(in Vector2Int tile)
        {
            return taken[tile.y * size.x + tile.x] > 0;
        }

        public bool IsOccupied(in RectInt rect)
        {
            for (int x = rect.x; x < rect.xMax; x++)
            {
                for (int y = rect.y; y < rect.yMax; y++)
                {
                    if (taken[y * size.x + x] > 0)
                        return true;
                }
            }

            return false;
        }

        void SetRectValue(in RectInt rect, int value)
        {
            for (int x = rect.x; x < rect.xMax; x++)
            {
                for (int y = rect.y; y < rect.yMax; y++)
                {
                    taken[y * size.x + x] = value;
                }
            }
        }

        int FindItemIndexAt(int x, int y)
        {
            var p = new Vector2Int(x, y);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].rect.Contains(p))
                    return i;
            }

            return -1;
        }

        public bool TryOccupyRect(in RectInt rect)
        {
            if (IsOccupied(rect))
                return false;

            current++;
            SetRectValue(rect, current);

            items.Add(new SlottedItem(null, rect, 0));

            return true;
        }

        public bool TryInsert(object item, in Vector2Int rootTile, in Vector2Int itemSize, int rotation)
        {
            Vector2Int size = rotation % 2 == 0 ? itemSize : new Vector2Int(itemSize.y, itemSize.x);
            if (TryOccupyRect(new RectInt(rootTile, size)))
            {
                items[items.Count - 1] = new SlottedItem(item, new RectInt(rootTile, size), rotation);
                return true;
            }

            return false;
        }

        public object FindItemAt(in Vector2Int tile)
        {
            int i = FindItemIndexAt(tile.x, tile.y);
            if (i >= 0)
            {
                return items[i].item;
            }
            else return null;
        }

        public bool TryRemoveItemAt(in Vector2Int tile, out object item, out RectInt rect, out int rotation)
        {
            int i = FindItemIndexAt(tile.x, tile.y);
            if (i >= 0)
            {
                item = items[i].item;
                rect = items[i].rect;
                rotation = items[i].rotation;
                RemoveItem(i);
                return true;
            }
            else
            {
                item = null;
                rect = default;
                rotation = 0;
                return false;
            }
        }

        public bool TryRemoveItemAt(in Vector2Int tile, out object item)
        {
            int i = FindItemIndexAt(tile.x, tile.y);
            if (i >= 0)
            {
                item = items[i].item;
                RemoveItem(i);
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }

        void RemoveItem(int i)
        {
            SetRectValue(items[i].rect, 0);
            items.RemoveAt(i);
        }

        #region Static functions

        /// <summary>
        /// Returns a world position of a rect center in an inventory
        /// </summary>
        public static Vector3 WorldPositionFromInventoryRect(in RectInt rect,
            Transform inventoryTransform, float separation)
        {
            return GetWorldPositionAtRectCenter(rect,
                inventoryTransform.position, inventoryTransform.forward, inventoryTransform.right, separation);
        }

        /// <summary>
        /// Returns a world position of a rect center in an inventory
        /// </summary>
        public static Vector3 GetWorldPositionAtRectCenter(in RectInt rect,
        in Vector3 inventoryPosition, in Vector3 inventoryUp, in Vector3 inventoryRight, float separation)
        {
            Vector2 c = rect.center * separation;
            return inventoryPosition + (inventoryRight * c.x + inventoryUp * c.y);
        }

        /// <summary>
        /// Returns invenotry world rotation, additionally rotated by a 90 degree multiplier rotation
        /// </summary>
        public static Quaternion GetItemWorldRotation(int rotation,
            in Vector3 inventoryUp, in Vector3 inventoryRight)
        {
            Vector3 normal = Vector3.Cross(inventoryRight, -inventoryUp).normalized;
            Quaternion rot = Quaternion.LookRotation(inventoryUp, normal);

            if (rotation == 0)
                return rot;
            else
                return Quaternion.AngleAxis(90 * rotation, normal) * rot;
        }

        /// <summary>
        /// Returns an inventory-space position that the ray points at. The transform pivot is in the lower left corner.
        /// Pass this value to GetInventoryTileFromLocalPosition to get the tile coordinate.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetInventoryPositionFromRay(
            in Ray ray, Transform inventoryTransform)
        {
            return GetInventoryPositionFromRay(
                ray, inventoryTransform.position, inventoryTransform.forward, inventoryTransform.right);
        }

        /// <summary>
        /// Returns an inventory-space position that the ray points at. The pivot is in the lower left corner.
        /// Pass this value to GetInventoryTileFromLocalPosition to get the tile coordinate.
        /// </summary>
        public static Vector2 GetInventoryPositionFromRay(
            in Ray ray, in Vector3 inventoryPos,
            in Vector3 inventoryUp, in Vector3 inventoryRight)
        {
            Vector3 normal = Vector3.Cross(inventoryUp, inventoryRight);
            Quaternion rot = Quaternion.LookRotation(-normal, inventoryUp);
            Matrix4x4 mat = Matrix4x4.TRS(inventoryPos, rot, Vector3.one);

            Plane plane = new Plane(normal, inventoryPos);
            plane.Raycast(ray, out float enter);

            var rayPoint = ray.GetPoint(enter);
            //Debug.DrawRay(rayPoint, normal);
            var rayPointLS = mat.inverse.MultiplyPoint3x4(rayPoint);

            return rayPointLS;
        }

        /// <summary>
        /// Returns the tile coordinate at the inventory-space position.
        /// </summary>
        /// <param name="separation">The distance between the tiles</param>
        /// <returns></returns>
        public static Vector2Int GetInventoryTileFromLocalPosition(in Vector2 position, float separation)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / separation),
                Mathf.FloorToInt(position.y / separation));
        }

        /// <summary>
        /// Returns the tile coordinate at the root of the rect, which center is at the inventory-space position.
        /// Use this coordinate for item insertion.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="inventorySize"></param>
        /// <param name="separation">The distance between the tiles</param>
        /// <param name="rotation">In 0-3 as a 90 degree multiplier</param>
        /// <returns></returns>
        public static Vector2Int GetRootTileFromLocalPosition(in Vector2 position, Vector2Int size, in Vector2Int inventorySize, float separation, int rotation = 0)
        {
            if (rotation > 0)
                size = RotatedItemSize(size, rotation);

            Vector2 extents = (Vector2)size * 0.5f * separation;
            Vector2 pos = (Vector2)position - extents;

            Vector2Int root = new Vector2Int(
                Mathf.RoundToInt(pos.x / separation),
                Mathf.RoundToInt(pos.y / separation));

            root.x = Mathf.Clamp(root.x, 0, inventorySize.x - size.x);
            root.y = Mathf.Clamp(root.y, 0, inventorySize.y - size.y);

            return root;
        }

        /// <summary>
        /// Returns a rotated rect. Rotation is 0-3 as a 90 degree multiplier
        /// </summary>
        /// <param name="size"></param>
        /// <param name="rotation">In 0-3 as a 90 degree multiplier</param>
        /// <returns></returns>
        public static Vector2Int RotatedItemSize(Vector2Int size, int rotation)
        {
            return rotation % 2 == 0 ? size : new Vector2Int(size.y, size.x);
        }

        #endregion

        #region Debug drawing
        public void DebugDrawTile(int x, int y,
            in Vector3 inventoryPos, in Vector3 inventoryUp, in Vector3 inventoryRight, float separation, float offset, Color color)
        {
            x = Mathf.Clamp(x, 0, size.x - 1);
            y = Mathf.Clamp(y, 0, size.y - 1);
            DebugDrawTile(x, y, inventoryPos, inventoryUp, inventoryRight, separation, offset, color);
        }

        public static void DebugDrawTile(int x, int y, Vector3 pos, Vector3 up, Vector3 right, float separation, float offset, Color color)
        {
            Vector3 tileCenter = pos + (up * (y + 0.5f) + right * (x + 0.5f)) * separation;
            Vector3 p0 = tileCenter + (up - right) * (separation - offset) * 0.5f;
            Vector3 p1 = tileCenter + (up + right) * (separation - offset) * 0.5f;
            Vector3 p2 = tileCenter + (-up - right) * (separation - offset) * 0.5f;
            Vector3 p3 = tileCenter + (-up + right) * (separation - offset) * 0.5f;

            Debug.DrawLine(p0, p1, color);
            Debug.DrawLine(p1, p3, color);
            Debug.DrawLine(p0, p2, color);
            Debug.DrawLine(p2, p3, color);
        }

        public static void DebugDrawRect(RectInt rect, Vector3 inventoryPos, Vector3 inventoryUp, Vector3 inventoryRight, float separation, float offset, Color color)
        {
            for (int x = rect.x; x < rect.xMax; x++)
            {
                for (int y = rect.y; y < rect.yMax; y++)
                {
                    DebugDrawTile(x, y, inventoryPos, inventoryUp, inventoryRight, separation, offset, color);
                }
            }
        }

        static Color GetColorForIndex(int i)
        {
            Random.InitState(i);
            return Random.ColorHSV(0, 1, 1, 1, 1, 1);
        }

        public void DebugDrawInventory(in Vector3 pos, in Vector3 up, in Vector3 right, float separation)
        {
            var state = Random.state;

            for (int x = 0; x < size.x + 1; x++)
            {
                for (int y = 0; y < size.y + 1; y++)
                {
                    Vector3 p = pos + (up * y + right * x) * separation;
                    if (y != size.y)
                        Debug.DrawRay(p, up * separation);
                    if (x != size.x)
                        Debug.DrawRay(p, right * separation);

                    if (x != size.x && y != size.y)
                    {
                        Vector3 mid = p + (up + right) * separation * 0.5f;

                        int index = taken[y * size.x + x];

                        if (index != 0)
                        {
                            Color color = GetColorForIndex(index);
                            DebugDrawTile(x, y, pos, up, right, separation, 0.05f, color);
                            //Debug.DrawRay(mid, Vector3.Cross(up, right) * separation, color);
                        }
                    }
                }
            }

            Random.state = state;
        }

        #endregion
    }
}
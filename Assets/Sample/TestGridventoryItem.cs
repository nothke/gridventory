using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Inventory;

public class TestGridventoryItem : MonoBehaviour
{
    public Vector2Int size = new Vector2Int(1, 1);

    Vector3 originalPosition;
    Quaternion originalRotation;

    public int gridventoryRotation { get; set; }

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void ReturnToOriginalLocation()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}

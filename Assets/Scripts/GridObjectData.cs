using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridObjectData", menuName = "GridSystem/GridObjectData")]
public class GridObjectData : ScriptableObject
{
    public GridObjectId id;
    public Sprite objectSprite;
    public GameObject objectPrefab;
}

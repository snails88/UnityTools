using UnityEngine;

[CreateAssetMenu(fileName = "New PositionData", menuName = "ScriptableObjects/PositionData")]
public class PositionData : ScriptableObject
{
    public Vector3[] positions;
}

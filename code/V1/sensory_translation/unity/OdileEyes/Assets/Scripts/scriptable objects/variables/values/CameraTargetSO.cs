
using UnityEngine;


[CreateAssetMenu(fileName = "Bool SO", menuName = "Scriptable Objects/Variables/Values/Camera Target")]
public class CameraTargetSO : ScriptableObject
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 characterControllerSpeed;

    public void SetValues(Vector3 newPos, Quaternion newRot, Vector3 newSpeed)
    {
        position = newPos;
        rotation = newRot;
        characterControllerSpeed = newSpeed;
    }
}


using UnityEngine;


public class MonoID : MonoBehaviour
{
    [SerializeField] 
    private string id;

    [ContextMenu("Generate GUID for ID")]
    private void GenerateGuid() => id = System.Guid.NewGuid().ToString();
    
    public string ID => id;
}

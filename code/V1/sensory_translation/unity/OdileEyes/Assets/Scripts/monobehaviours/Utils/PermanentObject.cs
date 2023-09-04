
using UnityEngine;


public class PermanentObject : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}

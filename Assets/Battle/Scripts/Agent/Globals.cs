using UnityEngine;

public class Globals : MonoBehaviour
{
    public static Globals main;
    public Vector3 target;

    void Awake()
    {
        if (main != null)
        {
            Debug.LogError("Globals already exists",this);
            return;
        }
        main = this;
    }

}
    

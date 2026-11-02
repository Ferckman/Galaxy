using UnityEngine;

public class LasersContainer : MonoBehaviour
{
    public static LasersContainer Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

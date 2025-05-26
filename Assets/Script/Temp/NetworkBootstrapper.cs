using UnityEngine;

public class NetworkBootstrapper : MonoBehaviour
{
    public GameObject networkManagerPrefab;
    private static bool isSpawned = false;

    void Awake()
    {
        if (!isSpawned)
        {
            GameObject manager = Instantiate(networkManagerPrefab);
            DontDestroyOnLoad(manager);
            isSpawned = true;
        }
    }
}

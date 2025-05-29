using UnityEngine;
using Unity.Netcode;

public class PipeSpawner : NetworkBehaviour
{
    public GameObject pipePrefab;  // Make sure this prefab has NetworkObject component and is registered in NetworkManager's NetworkPrefabs
    public Transform spawnPoint;
    public float spawnInterval = 2f;
    public float minY = -0.25f;
    public float maxY = 2.6f;

    private float timer;

    void Update()
    {
        if (!IsServer) return; // Only the server should spawn pipes

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPipe();
        }
    }

    private void SpawnPipe()
    {
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, randomY, 0);

        // Instantiate the pipe prefab on the server
        GameObject pipe = Instantiate(pipePrefab, spawnPos, Quaternion.identity);

        // Spawn the network object to sync it to clients
        // Passing no arguments means ownership stays with the server by default
        pipe.GetComponent<NetworkObject>().Spawn();

        Debug.Log("Pipe spawned on server at position: " + spawnPos);
    }
}

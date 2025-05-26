using UnityEngine;
using Unity.Netcode;

public class PipeMover : NetworkBehaviour
{
    public float moveSpeed = 2f;
    public float destroyX = -10f;

    void Update()
    {
        if (!IsServer) return; // Only the server moves and destroys pipes

        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;
using Unity.Netcode;

public class ScoreManager : NetworkBehaviour
{
    // Sync score across network, owned by the player who owns this ScoreManager
    public NetworkVariable<int> score = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // Method to add score, only executable by the owner
    public void AddScore()
    {
        if (!IsOwner) return;
        score.Value += 1;
    }
}

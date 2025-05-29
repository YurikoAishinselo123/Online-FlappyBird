using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public float countdownTime = 3f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(CountdownAndStartGame());
        }
    }

    private IEnumerator CountdownAndStartGame()
    {
        yield return new WaitForSeconds(0.5f); // Allow scene to finish loading

        // 🔁 Ask all clients to start their own local countdowns
        StartCountdownClientRpc(countdownTime);

        yield return new WaitForSeconds(countdownTime + 1f); // countdown + "GO!"

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                var controller = client.PlayerObject.GetComponent<BirdController>();
                controller.StartGameClientRpc();
            }
        }
    }

    [ClientRpc]
    private void StartCountdownClientRpc(float countdown)
    {
        if (TimerUI.Instance != null)
        {
            TimerUI.Instance.StartCountdown(countdown);
        }
        else
        {
            Debug.LogWarning("⚠️ TimerUI not found on client!");
        }
    }
}

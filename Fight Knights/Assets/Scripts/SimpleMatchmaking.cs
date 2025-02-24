using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

// Netcode and Relay
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

// Lobby
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class SimpleMatchmaking : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private Lobby _connectedLobby;
    private const string JoinCodeKey = "j";
    private string _playerId;

    private UnityTransport _transport;

    private void Awake()
    {
        // If your NetworkManager and UnityTransport exist in the same GameObject,
        // you can fetch the transport like this.
        _transport = FindObjectOfType<UnityTransport>();
    }

    private async void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null.");
            return;
        }

        // Subscribe to NGO callbacks
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        //CreateOrJoinLobby();
        // **Optional**: You could trigger CreateOrJoinLobby() here 
        // or use a button on your UI to call CreateOrJoinLobby.
        //await CreateOrJoinLobby();
    }

    // ------------ NGO Callbacks ------------
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected with ID: {clientId}");

        if (NetworkManager.Singleton.IsServer)
        {
            // (1) Server does a simple Instantiate
            var playerObject = Instantiate(playerPrefab);
            var networkObject = playerObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("Spawned prefab missing NetworkObject component.");
                return;
            }

            // (2) Spawn as a networked player object owned by that client
            networkObject.SpawnAsPlayerObject(clientId);

            // Optionally, if you want to ensure there's a PlayerInput on the prefab:
            var playerInput = playerObject.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null)
            {
                playerInput.ActivateInput(); // This just ensures the component is active
            }
        }
    }


    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected with ID: {clientId}");
        // Cleanup or additional logic can go here
    }

    // ------------ Lobby / Relay Flow ------------
    public async void CreateOrJoinLobby()
    {
        // Indicate we want to use WebSockets if we’re targeting WebGL
        // (Otherwise, you can omit _transport.UseWebSockets = true)
        _transport.UseWebSockets = true;

        await Authenticate();

        // Try to quick-join; if that fails, create a new lobby.
        _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
    }

    private async Task Authenticate()
    {
        var options = new InitializationOptions();
        await UnityServices.InitializeAsync(options);

        // Sign in anonymously to UGS
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _playerId = AuthenticationService.Instance.PlayerId;
    }

    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            // Attempt to join any open lobby
            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log($"Joined Lobby {lobby.Id}");

            // If successful, retrieve Relay join details via the stored join code
            var joinCode = lobby.Data[JoinCodeKey].Value;
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Setup NGO’s UnityTransport to connect via Relay
            // Using "wss" here if building for WebGL. Otherwise "dtls" is recommended.

            AllocationUtils.ToRelayServerData(joinAllocation, "wss");

            // Start as a client
            NetworkManager.Singleton.StartClient();

            return lobby;
        }
        catch (Exception e)
        {
            Debug.Log($"No lobbies available via quick join: {e.Message}");
            return null;
        }
    }

    private async Task<Lobby> CreateLobby()
    {
        try
        {
            const int maxPlayers = 16;

            // Create a Relay allocation
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            // Generate a Relay join code we can share
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Create a Lobby with the Relay join code in its data
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            var lobby = await LobbyService.Instance.CreateLobbyAsync("My Lobby", maxPlayers, options);
            Debug.Log($"Created Lobby {lobby.Id} with code: {joinCode}");

            // Keep the Lobby alive with heartbeats
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            // Setup NGO’s UnityTransport to host via Relay
            // Use "wss" if you plan on building for WebGL

            AllocationUtils.ToRelayServerData(allocation, "wss");

            /*NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(allocation, "wss");*/
            //AllocationUtils.ToRelayServerData(allocation, connectionType)
            // Start as host
            NetworkManager.Singleton.StartHost();

            return lobby;
        }
        catch (Exception e)
        {
            Debug.Log($"Failed creating lobby: {e}");
            return null;
        }
    }

    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            // Keep the lobby alive
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        try
        {
            StopAllCoroutines();

            // If we had joined or created a lobby, leave or delete it
            if (_connectedLobby != null)
            {
                if (_connectedLobby.HostId == _playerId)
                {
                    LobbyService.Instance.DeleteLobbyAsync(_connectedLobby.Id);
                }
                else
                {
                    LobbyService.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error shutting down lobby: {e}");
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

// NGO
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

// Relay & Lobby
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class SimpleMatchmaking : MonoBehaviour
{
    [Header("Assign your player prefab here")]
    [SerializeField] private GameObject playerPrefab;

    private UnityTransport _transport;

    private Lobby _connectedLobby;
    private const string JoinCodeKey = "j";  // Key used to store Relay join code in the Lobby
    private string _playerId;

    [SerializeField] private int maxPlayers = 4;

    // CHANGE THIS as needed:
    // - false => uses DTLS (secure UDP)
    // - true  => uses WSS (secure websockets)
    [SerializeField] private bool useSecure = false;

    private void Awake()
    {
        var netMgr = NetworkManager.Singleton;
        if (netMgr == null)
        {
            Debug.LogError("No NetworkManager in the scene!");
            return;
        }

        _transport = netMgr.GetComponent<UnityTransport>();
        if (_transport == null)
        {
            Debug.LogError("No UnityTransport found on NetworkManager.");
        }

        // Ensure Netcode doesn't auto-start
        netMgr.OnClientConnectedCallback += OnClientConnected;
        netMgr.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private async void Start()
    {
        // 1) Initialize Unity Services & sign in
        await InitializeUGS();

        // 2) Attempt Quick Join. If none found, create a new lobby+relay
        _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
    }

    // -----------------------------------------------
    // UGS Initialization
    // -----------------------------------------------
    private async Task InitializeUGS()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"UGS Initialized. PlayerId: {_playerId}");

            Debug.LogError($"Signed in as PlayerId: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"UGS Initialization failed: {e}");
        }
    }

    // -----------------------------------------------
    // Lobby + Relay Logic
    // -----------------------------------------------
    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            // Attempt to find any open lobby
            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log($"[Client] Quick-Joined Lobby: {lobby.Id}");

            // Retrieve the Relay join code from the Lobby data
            var relayJoinCode = lobby.Data[JoinCodeKey].Value;
            Debug.Log($"JoinCode from Lobby: {relayJoinCode}");

            // Join Relay with that code
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            // Correct order: (AllocationIdBytes, Key, ConnectionData, isSecure)
            _transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, // 16 bytes
                joinAllocation.Key,               // 32 bytes
                joinAllocation.ConnectionData           
            );

            // Now we can safely start the client
            NetworkManager.Singleton.StartClient();

            return lobby;
        }
        catch (Exception e)
        {
            Debug.Log($"No open lobby found or quick join failed: {e.Message}");
            return null;
        }
    }

    private async Task<Lobby> CreateLobby()
    {
        try
        {
            // 1) Create a Relay allocation
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"[Host] Created Relay allocation. JoinCode: {relayJoinCode}");

            // 2) Create a new Lobby with that Relay join code
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                }
            };
            var lobby = await LobbyService.Instance.CreateLobbyAsync("My Relay Lobby", maxPlayers, options);
            _connectedLobby = lobby;
            Debug.Log($"[Host] Created Lobby: {lobby.Id}");

            // Keep the lobby alive periodically
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15f));

            // 3) Configure UnityTransport for Relay (Host side)
            _transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, // 16 bytes
                allocation.Key,               // 32 bytes
                allocation.ConnectionData    // 16 bytes
            );

            // 4) Start Host AFTER Relay is set
            NetworkManager.Singleton.StartHost();

            return lobby;
        }
        catch (Exception e)
        {
            Debug.LogError($"CreateLobby or Relay Allocation failed: {e}");
            return null;
        }
    }

    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float intervalSeconds)
    {
        var delay = new WaitForSecondsRealtime(intervalSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    // -----------------------------------------------
    // NGO Callbacks (Spawning Player)
    // -----------------------------------------------
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NGO] Client connected: {clientId}");

        // If you'd like to manually spawn a player prefab:
        if (NetworkManager.Singleton.IsServer)
        {
            var playerObj = Instantiate(playerPrefab);
            var netObj = playerObj.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);

            // Optionally ensure PlayerInput is active
            var pInput = playerObj.GetComponent<PlayerInput>();
            if (pInput)
            {
                pInput.ActivateInput();
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NGO] Client disconnected: {clientId}");
    }

    // -----------------------------------------------
    // Cleanup on Destroy
    // -----------------------------------------------
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        StopAllCoroutines();

        try
        {
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
            Debug.LogError($"Error shutting down lobby: {e}");
        }
    }
}

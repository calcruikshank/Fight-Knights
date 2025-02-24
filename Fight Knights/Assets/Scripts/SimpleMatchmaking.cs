using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;
using Unity.Networking.Transport.Relay;
using UnityEngine.InputSystem;
using Unity.Android.Gradle.Manifest;



public class SimpleMatchmaking : MonoBehaviour
{
    [SerializeField] private GameObject _buttons;

    private Lobby _connectedLobby;
    private QueryResponse _lobbies;
    private UnityTransport _transport;
    private const string JoinCodeKey = "j";
    private string _playerId;
    private List<ulong> connectedClientIds = new List<ulong>();
    private void Awake() => _transport = FindObjectOfType<UnityTransport>();


    public void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null.");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        CreateOrJoinLobby();
    }
    [SerializeField] GameObject playerPrefab;
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected with ID: {clientId}");

        if (NetworkManager.Singleton.IsServer) // Check to make sure we're on the server
        {
            Debug.Log("Server: Client connected, spawning player prefab.");
            var playerObject1 = Instantiate(playerPrefab);
            var networkObject = playerObject1.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);

            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                var playerObject = Instantiate(playerPrefab);
                networkObject = playerObject.GetComponent<NetworkObject>();
                networkObject.SpawnAsPlayerObject(clientId);
                Destroy(playerObject1);
            }

            if (networkObject == null)
            {
                Debug.LogError("Spawned player object does not have a NetworkObject component.");
                return;
            }

            Debug.Log($"Player object spawned for client ID: {clientId}");

            // Log the network object owner client ID
            //networkObject.Spawn();


            networkObject.ChangeOwnership(clientId);

            Debug.Log(networkObject.GetComponent<PlayerInput>() + " player input of the network object " + clientId);

            networkObject.GetComponent<PlayerInput>().ActivateInput();
            Debug.Log($"NetworkObject's owner client ID: {networkObject.OwnerClientId}");


        }
    }


    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected with ID: {clientId}");
    }


    public async void CreateOrJoinLobby()
    {
        _transport.UseWebSockets = true;
        await Authenticate();

        _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
    }

    private async Task Authenticate()
    {
        var options = new InitializationOptions();


        await UnityServices.InitializeAsync(options);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _playerId = AuthenticationService.Instance.PlayerId;
    }

    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            options.Filter = new List<QueryFilter>()
    {
        new QueryFilter(
            field: QueryFilter.FieldOptions.MaxPlayers,
            op: QueryFilter.OpOptions.GE,
            value: "10")
    };
            // Attempt to join a lobby in progress
            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);

            // If we found one, grab the relay allocation details
            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);
            //
            // Set the details to the transform
            SetTransformAsClient(a);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(a, "wss"));

            _transport.UseWebSockets = true;
            // Join the game room as a client
            NetworkManager.Singleton.StartClient();
            return lobby;
        }
        catch (Exception e)
        {
            Debug.Log($"No lobbies available via quick join");
            return null;
        }
    }

    private async Task<Lobby> CreateLobby()
    {
        try
        {
            const int maxPlayers = 16;

            // Create a relay allocation and generate a join code to share with the lobby
            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            // Create a lobby, adding the relay join code to the lobby data
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };
            var lobby = await LobbyService.Instance.CreateLobbyAsync("Useless Lobby Name", maxPlayers, options);

            // Send a heartbeat every 15 seconds to keep the room alive
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            // Set the game room to use the relay allocation
            _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(a, "wss"));
            _transport.UseWebSockets = true;
            // Start the room. I'm doing this immediately, but maybe you want to wait for the lobby to fill up

            NetworkManager.Singleton.StartHost();
            return lobby;
        }
        catch (Exception e)
        {
            Debug.LogFormat("Failed creating a lobby");
            return null;
        }
    }

    private void SetTransformAsClient(JoinAllocation a)
    {
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(a, "wss"));

        _transport.UseWebSockets = true;
    }

    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        try
        {
            StopAllCoroutines();
            // todo: Add a check to see if you're host
            if (_connectedLobby != null)
            {
                if (_connectedLobby.HostId == _playerId) LobbyService.Instance.DeleteLobbyAsync(_connectedLobby.Id);
                else LobbyService.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error shutting down lobby: {e}");
        }
    }


}
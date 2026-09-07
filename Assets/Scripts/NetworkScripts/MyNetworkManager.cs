using UnityEngine;
using Mirror;
using System;

public struct OpponentLeftMessage : NetworkMessage { }

public class MyNetworkManager : NetworkManager
{
    [Header("Dependencies")]
    public RoomManager roomManager;

    private string currentRoomCode = string.Empty;

    public static event Action OnOpponentDisconnected;
    public static event Action OnLocalClientDisconnected;
    public static System.Action OnHostFullyStopped;

    public void SetCurrentRoomCode(string code)
    {
        currentRoomCode = code;
    }

    public string GetCurrentRoomCode()
    {
        return currentRoomCode;
    }

    #region Client Callbacks

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<OpponentLeftMessage>(OnOpponentLeftMessageReceived);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        NetworkClient.UnregisterHandler<OpponentLeftMessage>();
    }

    public override void OnClientDisconnect()
    {
        bool iAmHost = NetworkServer.active;

        base.OnClientDisconnect();

        if(!iAmHost)
        {
            Debug.Log("[NetworkManager] Соединение с сервером потеряно. Оппонент (хост) отключился.");
            OnOpponentDisconnected?.Invoke();
        }
        else
        {
            Debug.Log("[NetworkManager] Локальный хост успешно остановил клиента.");
        }
        
        OnLocalClientDisconnected?.Invoke();
    }

    private void OnOpponentLeftMessageReceived(OpponentLeftMessage msg)
    {
        Debug.Log("[NetworkManager] Получено сообщение от сервера: оппонент вышел.");
        OnOpponentDisconnected?.Invoke();
    }

    #endregion

    #region Server Callbacks

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if(NetworkServer.active && conn != NetworkServer.localConnection)
        {
            Debug.Log($"[NetworkManager] Гость {conn.connectionId} покинул матч. Оповещаем остальных.");
            
            foreach(var readyConn in NetworkServer.connections.Values)
            {
                if (readyConn != null && readyConn != conn && readyConn != NetworkServer.localConnection)
                {
                    readyConn.Send(new OpponentLeftMessage());
                }
            }
        }

        base.OnServerDisconnect(conn);

        if(NetworkServer.connections.Count <= 1)
        {
            DeleteRoomFromBackend();
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        DeleteRoomFromBackend();
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.Log("[MyNetworkManager] Хост полностью остановлен, транспорт свободен.");
        OnHostFullyStopped?.Invoke();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        if (conn != NetworkServer.localConnection)
        {
            Debug.Log("[CustomNetworkManager] Гость подключился!");

            MatchmakerScr matchmaker = FindAnyObjectByType<MatchmakerScr>();
            if (matchmaker != null)
            {
                matchmaker.OnOpponentJoinedHost();
            }
        }
    }

    private void DeleteRoomFromBackend()
    {
        if (!string.IsNullOrEmpty(currentRoomCode) && roomManager != null)
        {
            string codeToDelete = currentRoomCode;

            roomManager.DeleteRoom(codeToDelete,
                () => 
                {
                    Debug.Log($"[NetworkManager] Комната {codeToDelete} удалена.");
                    currentRoomCode = string.Empty;
                },
                (err) => 
                {
                    Debug.LogError($"[NetworkManager] Ошибка удаления {codeToDelete}: {err}");
                    currentRoomCode = string.Empty;
                }
            );
        }
    }

    #endregion
}

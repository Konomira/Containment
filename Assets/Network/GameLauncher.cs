using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using static Photon.Pun.PhotonNetwork;

public class GameLauncher : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    [SerializeField]
    private GameObject controlPanel;

    [SerializeField]
    private TMP_Text roomCode;
    
    [SerializeField]
    private GameObject progressLabel;
    
    private static string gameVersion = "1";

    private static GameLauncher instance;

    private bool isConnecting;
    
    private void Start()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    private void Awake()
    {
        instance = this;
        AutomaticallySyncScene = true;
    }

    private void DoConnect()
    {
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);
        progressLabel.GetComponent<TMP_Text>().text = "Connecting";
        if(IsConnected)
        {
            if(roomCode.text.IsNullOrEmpty())
                JoinRandomRoom();
            else
                JoinRoom(roomCode.text);
        }
        else
        {
            isConnecting = ConnectUsingSettings();
            GameVersion = gameVersion;
        }
    }

    public static void Connect() => instance.DoConnect();
    
    public override void OnConnectedToMaster()
    {
        Log("OnConnectedToMaster was called");

        if(isConnecting)
        {
            if(roomCode.text.IsNullOrEmpty())
                JoinRandomRoom();
            else
                JoinRoom(roomCode.text);
            
            isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        isConnecting = false;
        Log($"OnDisconnected was called. reason: {cause}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom(null, new RoomOptions{MaxPlayers = maxPlayersPerRoom});
    }

    public override void OnJoinedRoom()
    {
        Log("OnJoinedRoom was called");
        
        if(CurrentRoom.PlayerCount > 0)
            LoadLevel("TestScene");
    }

    private void Log(string message) => Debug.Log(message);
}

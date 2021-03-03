using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static Photon.Pun.PhotonNetwork;

public class GameLauncher : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    [SerializeField]
    private GameObject controlPanel;

    [SerializeField]
    private GameObject progressLabel;
    
    private static string gameVersion = "1";

    private static GameLauncher instance;
    
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
        
        if(IsConnected)
            JoinRandomRoom();
        else
        {
            ConnectUsingSettings();
            GameVersion = gameVersion;
        }
    }

    public static void Connect() => instance.DoConnect();
    
    public override void OnConnectedToMaster()
    {
        Log("OnConnectedToMaster was called");

        JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        
        Log($"OnDisconnected was called. reason: {cause}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom(null, new RoomOptions{MaxPlayers = maxPlayersPerRoom});
    }

    public override void OnJoinedRoom() => Log("OnJoinedRoom was called");

    private void Log(string message) => Debug.Log(message);
}

using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
	[SerializeField]
	private Button quitButton;
	
	private const string MainMenu = "MainMenu";
	private const string TestScene = "TestScene";

	private void Awake() => quitButton.onClick.AddListener(LeaveRoom);

	public override void OnLeftRoom() => SceneManager.LoadScene(MainMenu);
	
	public void LeaveRoom() => PhotonNetwork.LeaveRoom();
}

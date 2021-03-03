using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class TextChat : MonoBehaviourPunCallbacks, IOnEventCallback
{
	private const byte SendMessageEventCode = 1;
	
	
	[SerializeField]
	private GameObject messagePrefab;

	[SerializeField]
	private Transform container;
	
	[ContextMenu("Send test message")]
	private void SendTestMessage() => SendMessage("test", false);

	private void Awake() => SendLogNoNotify($"{PhotonNetwork.NickName} has entered the room");
	public override void OnPlayerEnteredRoom(Player player) => SendLogNoNotify($"{player.NickName} has entered the room");
	public override void OnPlayerLeftRoom(Player player) => SendLogNoNotify($"{player.NickName} has left the room");

	private void SendMessage(string message, bool log = false)
	{
		NotifyMessage(message,log);
	}
	
	private void SendMessageNoNotify(string message, bool log = false)
	{
		var instance = Instantiate(messagePrefab, container);
		var text = instance.GetComponent<TMP_Text>();
		text.text = message;
		
		if(log)
			text.color = Color.red;
	}
	
	private void SendLog(string message) => SendMessage(message, true);
	private void SendLogNoNotify(string message) => SendMessageNoNotify(message, true);
	
	private void NotifyMessage(string message, bool log)
	{
		var content = new object[] { message, log };
		var raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
		PhotonNetwork.RaiseEvent(SendMessageEventCode, content, raiseEventOptions, SendOptions.SendReliable);
	}

	public void OnEvent(EventData photonEvent)
	{
		var eventCode = photonEvent.Code;

		if(eventCode == SendMessageEventCode)
		{
			var data = (object[])photonEvent.CustomData;

			var message = (string)data[0];
			var log = (bool)data[1];
			
			SendMessageNoNotify(message,log);
		}
	}

	public override void OnEnable() => PhotonNetwork.AddCallbackTarget(this);

	public override void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);
}

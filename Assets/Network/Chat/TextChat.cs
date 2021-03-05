using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebSocketSharp;
using static NetworkEvent;

public class TextChat : MonoBehaviourPunCallbacks
{
	private const byte SendMessageEventCode = 1;
	
	[SerializeField]
	private GameObject messagePrefab;

	[SerializeField]
	private Transform container;

	[SerializeField]
	private TMP_InputField messageInput;

	private List<TMP_Text> messageInstances = new List<TMP_Text>();
	
	[ContextMenu("Send test message")]
	private void SendTestMessage() => SendMessage("test");

	private void Awake()
	{
		SendLogNoNotify($"{PhotonNetwork.NickName} has entered the room");
		messageInput.onSubmit.AddListener(SendInputAsMessage);
		NetworkEvent.RegisterEvent(NetworkEvent.EventCode.ChatMessage, ReceiveMessage);
	}

	private void OnDestroy() => NetworkEvent.UnregisterEvent(NetworkEvent.EventCode.ChatMessage, ReceiveMessage);

	private void Update()
	{
		var eventSystem = EventSystem.current;
		
		//TODO: Move all keybinds to a central location
		if(Input.GetKeyDown(KeyCode.T) && eventSystem.currentSelectedGameObject == null)
			eventSystem.SetSelectedGameObject(messageInput.gameObject);
	}

	private void SendInputAsMessage(string text)
	{
		if(messageInput.wasCanceled)
		{
			messageInput.text = string.Empty;
			return;
		}

		if(string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)) return;
		
		SendMessage(text);
		messageInput.text = string.Empty;

		if(EventSystem.current.currentSelectedGameObject == messageInput.gameObject)
			EventSystem.current.SetSelectedGameObject(null);
	}

	public override void OnPlayerEnteredRoom(Player player) => SendLogNoNotify($"{player.NickName} has entered the room");
	public override void OnPlayerLeftRoom(Player player) => SendLogNoNotify($"{player.NickName} has left the room");

	private void SendMessage(string message, bool log = false) => NotifyMessage(PhotonNetwork.NickName, message, log);

	private void SendMessageNoNotify(string sender, string message, bool log = false)
	{
		GameObject instance;

		if(messageInstances.Count < 5)
		{
			instance = Instantiate(messagePrefab, container);
			messageInstances.Add(instance.GetComponent<TMP_Text>());
		}
		else
		{
			instance = messageInstances.Last().gameObject;
			MoveMessagesUpOnce();
		}

		var text = instance.GetComponent<TMP_Text>();

		if(log)
		{
			text.color = Color.red;
			text.text = string.Empty;
		}
		else
			text.text = $"[{sender}] ";
		
		text.text += message;
		
		messageInput.transform.SetAsLastSibling();
	}

	private void MoveMessagesUpOnce()
	{
		for(var index = 0; index < messageInstances.Count - 1 ; index++)
		{
			var instance = messageInstances[index];
			var nextInstance = messageInstances[index + 1];
			
			instance.text = nextInstance.text;
			instance.color = nextInstance.color;
		}
	}

	private void SendLog(string message) => SendMessage(message, true);
	private void SendLogNoNotify(string message) => SendMessageNoNotify(string.Empty, message, true);
	
	private void NotifyMessage(string nickname, string message, bool log)
	{
		var content = new object[] { nickname, message, log };
		
		RaiseEvent(NetworkEvent.EventCode.ChatMessage, content);
	}

	public void ReceiveMessage(object[] data)
	{
		var sender = (string)data[0];
		var message = (string)data[1];
		var log = (bool)data[2];
			
		SendMessageNoNotify(sender, message, log);
	}
}

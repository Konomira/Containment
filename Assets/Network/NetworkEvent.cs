using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkEvent : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public enum EventCode {__RESERVED__, Movement, ChatMessage}

    private static Dictionary<EventCode, Action<object[]>> RegisteredEvents = new Dictionary<EventCode, Action<object[]>>();

    private static NetworkEvent instance;

    private void Awake() => instance = this;

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(instance);
    }

    public override void OnDisable() => PhotonNetwork.RemoveCallbackTarget(instance);

    public static void RegisterEvent(EventCode eventCode, Action<object[]> callback)
    {
        if(callback == null)
            Debug.LogWarning("Attempted to register event with no callback, ignoring");
        
        if(RegisteredEvents.ContainsKey(eventCode))
            RegisteredEvents[eventCode] += callback;
        else
            RegisteredEvents.Add(eventCode, callback);
    }

    public static void UnregisterEvent(EventCode eventCode, Action<object[]> callback)
    {
        if(!RegisteredEvents.ContainsKey(eventCode))
        {
            Debug.LogWarning("Attempted to unregister event that does not exist, ignoring");
            return;
        }
        RegisteredEvents[eventCode] -= callback;

        if(RegisteredEvents[eventCode] == null)
            RegisteredEvents.Remove(eventCode);
    }

    private void Raise(EventCode eventCode, object[] data)
    {
        PhotonNetwork.RaiseEvent(
            (byte)eventCode, 
            data, 
            new RaiseEventOptions{Receivers = ReceiverGroup.All}, 
            SendOptions.SendReliable);
    }

    public static void RaiseEvent(EventCode eventCode, object[] data) => instance.Raise(eventCode, data);

    public void OnEvent(EventData eventData)
    {
        var eventCode = (EventCode)eventData.Code;
        if(!RegisteredEvents.ContainsKey(eventCode)) return;

        RegisteredEvents[eventCode]?.Invoke((object[])eventData.CustomData);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public static class NetworkManager
{
    private static readonly Dictionary<string, Action<byte[]>> protocolCallbacks =
        new Dictionary<string, Action<byte[]>>();

    public static event Action<string, byte[]> OnSend;

    public static void Send(string protocolName, byte[] payload = null)
    {
        if (string.IsNullOrEmpty(protocolName))
        {
            Debug.LogWarning("Send failed: protocolName is null or empty.");
            return;
        }

        OnSend?.Invoke(protocolName, payload ?? Array.Empty<byte>());
    }

    public static void Register(string protocolName, Action<byte[]> callback)
    {
        if (string.IsNullOrEmpty(protocolName) || callback == null)
        {
            return;
        }

        if (protocolCallbacks.TryGetValue(protocolName, out Action<byte[]> handlers))
        {
            handlers += callback;
            protocolCallbacks[protocolName] = handlers;
            return;
        }

        protocolCallbacks.Add(protocolName, callback);
    }

    public static void Unregister(string protocolName, Action<byte[]> callback)
    {
        if (string.IsNullOrEmpty(protocolName) || callback == null)
        {
            return;
        }

        if (!protocolCallbacks.TryGetValue(protocolName, out Action<byte[]> handlers))
        {
            return;
        }

        handlers -= callback;
        if (handlers == null)
        {
            protocolCallbacks.Remove(protocolName);
            return;
        }

        protocolCallbacks[protocolName] = handlers;
    }

    internal static void Notify(string protocolName, byte[] payload)
    {
        if (string.IsNullOrEmpty(protocolName))
        {
            Debug.LogWarning("Notify failed: protocolName is null or empty.");
            return;
        }

        if (!protocolCallbacks.TryGetValue(protocolName, out Action<byte[]> handlers))
        {
            Debug.LogWarning($"Unhandled protocol: {protocolName}");
            return;
        }

        handlers?.Invoke(payload ?? Array.Empty<byte>());
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public static class ViewManager
{
    private static readonly Dictionary<string, WndBase> wndMap = new Dictionary<string, WndBase>();

    public static T Show<T>(string prefabPath = null, Transform parent = null) where T : WndBase
    {
        T wnd = GetOrCreate<T>(prefabPath, parent);
        if (wnd == null)
        {
            return null;
        }

        wnd.Open();
        return wnd;
    }

    public static T GetOrCreate<T>(string prefabPath = null, Transform parent = null) where T : WndBase
    {
        string wndName = ResolveName<T>();
        if (wndMap.TryGetValue(wndName, out WndBase existingWnd) && existingWnd != null)
        {
            return existingWnd as T;
        }

        string loadPath = string.IsNullOrEmpty(prefabPath) ? wndName : prefabPath;
        GameObject prefab = Resources.Load<GameObject>(loadPath);
        if (prefab == null)
        {
            Debug.LogError($"Wnd prefab not found: {loadPath}");
            return null;
        }

        GameObject instance = parent == null
            ? UnityEngine.Object.Instantiate(prefab)
            : UnityEngine.Object.Instantiate(prefab, parent, false);

        T wnd = instance.GetComponent<T>();
        if (wnd == null)
        {
            Debug.LogError($"Wnd component missing on prefab: {wndName}");
            UnityEngine.Object.Destroy(instance);
            return null;
        }

        wndMap[wndName] = wnd;
        return wnd;
    }

    public static T GetWnd<T>() where T : WndBase
    {
        string wndName = ResolveName<T>();
        if (wndMap.TryGetValue(wndName, out WndBase wnd) && wnd != null)
        {
            return wnd as T;
        }

        return null;
    }

    public static WndBase GetWnd(string wndName)
    {
        if (string.IsNullOrEmpty(wndName))
        {
            return null;
        }

        wndMap.TryGetValue(wndName, out WndBase wnd);
        return wnd;
    }

    public static void Close<T>() where T : WndBase
    {
        T wnd = GetWnd<T>();
        if (wnd != null)
        {
            wnd.Close();
        }
    }

    public static bool RemoveWnd<T>() where T : WndBase
    {
        return RemoveWnd(ResolveName<T>());
    }

    public static bool RemoveWnd(string wndName)
    {
        if (string.IsNullOrEmpty(wndName))
        {
            return false;
        }

        if (!wndMap.TryGetValue(wndName, out WndBase wnd))
        {
            return false;
        }

        wndMap.Remove(wndName);
        if (wnd != null)
        {
            UnityEngine.Object.Destroy(wnd.gameObject);
        }

        return true;
    }

    public static void Clear()
    {
        foreach (KeyValuePair<string, WndBase> pair in wndMap)
        {
            if (pair.Value != null)
            {
                UnityEngine.Object.Destroy(pair.Value.gameObject);
            }
        }

        wndMap.Clear();
    }

    internal static void RegisterWnd(WndBase wnd)
    {
        if (wnd == null)
        {
            return;
        }

        string wndName = wnd.GetType().Name;
        if (wndMap.TryGetValue(wndName, out WndBase existingWnd) && existingWnd != null && existingWnd != wnd)
        {
            Debug.LogWarning($"Duplicate wnd instance detected: {wndName}");
            UnityEngine.Object.Destroy(wnd.gameObject);
            return;
        }

        wndMap[wndName] = wnd;
    }

    internal static void UnregisterWnd(WndBase wnd)
    {
        if (wnd == null)
        {
            return;
        }

        string wndName = wnd.GetType().Name;
        if (wndMap.TryGetValue(wndName, out WndBase existingWnd) && existingWnd == wnd)
        {
            wndMap.Remove(wndName);
        }
    }

    private static string ResolveName<T>()
    {
        string typeName = typeof(T).Name;
        if (!string.IsNullOrEmpty(typeName))
        {
            return typeName;
        }

        Debug.LogWarning("Wnd type name is invalid.");
        return Guid.NewGuid().ToString("N");
    }
}

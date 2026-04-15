using System;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager
{
    private readonly Dictionary<string, ControlBase> controlMap = new Dictionary<string, ControlBase>();

    public T RegisterControl<T>(string name = null) where T : ControlBase, new()
    {
        string controlName = ResolveName<T>(name);
        if (controlMap.TryGetValue(controlName, out ControlBase existingControl))
        {
            return existingControl as T;
        }

        T control = new T();
        control.Initialize(controlName);
        controlMap.Add(controlName, control);
        return control;
    }

    public T GetControl<T>(string name = null) where T : ControlBase
    {
        string controlName = ResolveName<T>(name);
        if (controlMap.TryGetValue(controlName, out ControlBase control))
        {
            return control as T;
        }

        return null;
    }

    public ControlBase GetControl(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        controlMap.TryGetValue(name, out ControlBase control);
        return control;
    }

    public bool RemoveControl<T>(string name = null) where T : ControlBase
    {
        string controlName = ResolveName<T>(name);
        return RemoveControl(controlName);
    }

    public bool RemoveControl(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (!controlMap.TryGetValue(name, out ControlBase control))
        {
            return false;
        }

        control.Release();
        controlMap.Remove(name);
        return true;
    }

    public void Clear()
    {
        foreach (KeyValuePair<string, ControlBase> pair in controlMap)
        {
            pair.Value.Release();
        }

        controlMap.Clear();
    }

    private static string ResolveName<T>(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return name;
        }

        string typeName = typeof(T).Name;
        if (string.IsNullOrEmpty(typeName))
        {
            Debug.LogWarning("Control type name is invalid.");
            return Guid.NewGuid().ToString("N");
        }

        return typeName;
    }
}

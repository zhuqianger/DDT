using System;
using System.Collections.Generic;
using UnityEngine;

public static class ModelManager
{
    private static readonly Dictionary<string, ModelBase> modelMap = new Dictionary<string, ModelBase>();

    public static T RegisterModel<T>(string name = null) where T : ModelBase, new()
    {
        string modelName = ResolveName<T>(name);
        if (modelMap.TryGetValue(modelName, out ModelBase existingModel))
        {
            return existingModel as T;
        }

        T model = new T();
        model.Initialize(modelName);
        modelMap.Add(modelName, model);
        return model;
    }

    public static T GetModel<T>(string name = null) where T : ModelBase
    {
        string modelName = ResolveName<T>(name);
        if (modelMap.TryGetValue(modelName, out ModelBase model))
        {
            return model as T;
        }

        return null;
    }

    public static ModelBase GetModel(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        modelMap.TryGetValue(name, out ModelBase model);
        return model;
    }

    public static bool RemoveModel<T>(string name = null) where T : ModelBase
    {
        string modelName = ResolveName<T>(name);
        return RemoveModel(modelName);
    }

    public static bool RemoveModel(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (!modelMap.TryGetValue(name, out ModelBase model))
        {
            return false;
        }

        model.Release();
        modelMap.Remove(name);
        return true;
    }

    public static void Clear()
    {
        foreach (KeyValuePair<string, ModelBase> pair in modelMap)
        {
            pair.Value.Release();
        }

        modelMap.Clear();
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
            Debug.LogWarning("Model type name is invalid.");
            return Guid.NewGuid().ToString("N");
        }

        return typeName;
    }
}

using System;

public abstract class ModelBase
{
    public string Name { get; private set; } = string.Empty;

    public bool IsInitialized { get; private set; }

    internal void Initialize(string name)
    {
        if (IsInitialized)
        {
            return;
        }

        Name = name;
        OnInit();
        IsInitialized = true;
    }

    internal void Release()
    {
        if (!IsInitialized)
        {
            return;
        }

        OnRelease();
        IsInitialized = false;
    }

    protected virtual void OnInit()
    {
    }

    protected virtual void OnRelease()
    {
    }
}

using System.Collections.Generic;
using UnityEngine;

public class WndBase : MonoBehaviour
{
    private readonly List<ViewBase> childViews = new List<ViewBase>();

    public bool IsOpened { get; private set; }

    protected virtual void Awake()
    {
        OnInit();
    }

    public virtual void Open()
    {
        if (IsOpened)
        {
            return;
        }

        gameObject.SetActive(true);
        IsOpened = true;
        OnOpen();
        SetChildrenVisible(true);
    }

    public virtual void Close()
    {
        if (!IsOpened)
        {
            return;
        }

        SetChildrenVisible(false);
        OnClose();
        IsOpened = false;
        gameObject.SetActive(false);
    }

    public virtual void Refresh()
    {
        OnRefresh();
    }

    public virtual void RegisterChildView(ViewBase view)
    {
        if (view == null || childViews.Contains(view))
        {
            return;
        }

        view.Initialize(this);
        childViews.Add(view);
    }

    public virtual void UnregisterChildView(ViewBase view)
    {
        if (view == null)
        {
            return;
        }

        childViews.Remove(view);
    }

    protected virtual void OnInit()
    {
    }

    protected virtual void OnOpen()
    {
    }

    protected virtual void OnClose()
    {
    }

    protected virtual void OnRefresh()
    {
    }

    protected virtual void OnWndDestroy()
    {
    }

    private void SetChildrenVisible(bool visible)
    {
        for (int i = 0; i < childViews.Count; i++)
        {
            ViewBase childView = childViews[i];
            if (childView == null)
            {
                continue;
            }

            if (visible)
            {
                childView.Show();
            }
            else
            {
                childView.Hide();
            }
        }
    }

    private void OnDestroy()
    {
        OnWndDestroy();
        childViews.Clear();
    }
}

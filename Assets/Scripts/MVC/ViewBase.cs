using UnityEngine;

public class ViewBase : MonoBehaviour
{
    public bool IsVisible { get; private set; }

    public WndBase OwnerWnd { get; private set; }

    public virtual void Initialize(WndBase ownerWnd)
    {
        OwnerWnd = ownerWnd;
        OnInit();
    }

    public virtual void Show()
    {
        if (IsVisible)
        {
            return;
        }

        gameObject.SetActive(true);
        IsVisible = true;
        OnShow();
    }

    public virtual void Hide()
    {
        if (!IsVisible)
        {
            return;
        }

        OnHide();
        IsVisible = false;
        gameObject.SetActive(false);
    }

    protected virtual void OnInit()
    {
    }

    protected virtual void OnShow()
    {
    }

    protected virtual void OnHide()
    {
    }

    protected virtual void OnViewDestroy()
    {
    }

    private void OnDestroy()
    {
        OnViewDestroy();
    }
}

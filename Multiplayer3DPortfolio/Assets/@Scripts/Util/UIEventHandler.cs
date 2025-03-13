using UnityEngine;
using UnityEngine.EventSystems;
using System;

public static class UIEventHandler
{
    public static void BindEvent(this GameObject go, Action action = null, Action<PointerEventData> dragAction = null, EUIEvent type = EUIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case EUIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case EUIEvent.Pressed:
                evt.OnPressedHandler -= action;
                evt.OnPressedHandler += action;
                break;
            case EUIEvent.PointerDown:
                evt.OnPointerDownHandler -= action;
                evt.OnPointerDownHandler += action;
                break;
            case EUIEvent.PointerUp:
                evt.OnPointerUpHandler -= action;
                evt.OnPointerUpHandler += action;
                break;
            case EUIEvent.Drag:
                evt.OnDragHandler -= dragAction;
                evt.OnDragHandler += dragAction;
                break;
            case EUIEvent.BeginDrag:
                evt.OnBeginDragHandler -= dragAction;
                evt.OnBeginDragHandler += dragAction;
                break;
            case EUIEvent.EndDrag:
                evt.OnEndDragHandler -= dragAction;
                evt.OnEndDragHandler += dragAction;
                break;
        }
    }
    
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }
}

public enum EUIEvent
{
        Click,
        Pressed,
        PointerDown,
        PointerUp,
        Drag,
        BeginDrag,
        EndDrag,
}

public class UI_EventHandler : MonoBehaviour,  IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Action OnClickHandler = null;
    public Action OnPressedHandler = null;
    public Action OnPointerDownHandler = null;
    public Action OnPointerUpHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public Action<PointerEventData> OnBeginDragHandler = null;
    public Action<PointerEventData> OnEndDragHandler = null;

    private bool _pressed = false;

    private void Update()
    {
        if (_pressed)
            OnPressedHandler?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClickHandler != null)
        {
            OnClickHandler.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        OnPointerDownHandler?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = true;
        OnPointerUpHandler?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        _pressed = true;
        OnDragHandler?.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragHandler?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragHandler?.Invoke(eventData);
    }
}
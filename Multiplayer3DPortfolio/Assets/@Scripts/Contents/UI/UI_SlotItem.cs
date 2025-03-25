using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SlotItem : UI_Base
{
    [SerializeField] protected ScrollRect _parentScrollRect;

    protected override void OnInit()
    {
        _parentScrollRect = gameObject.FindAncestor<ScrollRect>(gameObject);

        BindEvent(gameObject, null, OnBeginDrag, EUIEvent.BeginDrag);
        BindEvent(gameObject, null, OnDrag, EUIEvent.Drag);
        BindEvent(gameObject, null, OnEndDrag, EUIEvent.EndDrag);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        _parentScrollRect.OnBeginDrag(eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        _parentScrollRect.OnDrag(eventData);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        _parentScrollRect.OnEndDrag(eventData);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SlotItem : UI_Base
{
    [SerializeField] protected ScrollRect _parentScrollRect;

    protected override void OnInit()
    {
        _parentScrollRect = Util.FindAncestor<ScrollRect>(gameObject);

        gameObject.BindEvent(null, OnBeginDrag, EUIEvent.BeginDrag);
        gameObject.BindEvent(null, OnDrag, EUIEvent.Drag);
        gameObject.BindEvent(null, OnEndDrag, EUIEvent.EndDrag);
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

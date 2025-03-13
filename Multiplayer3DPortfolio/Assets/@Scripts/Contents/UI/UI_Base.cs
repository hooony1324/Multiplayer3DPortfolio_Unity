using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum EUIEvent
{
        Click,
        PointerDown,
        PointerUp,
        Drag,
        BeginDrag,
        EndDrag,
}


public class UI_Base : InitializeBase
{
    protected override void OnInit() {}
    
    protected Dictionary<Type, Dictionary<string, UnityEngine.Object>> _objects = new Dictionary<Type, Dictionary<string, UnityEngine.Object>>();

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);

        if (_objects.ContainsKey(typeof(T)))
            _objects.Remove(typeof(T));

        Dictionary<string, UnityEngine.Object> dict = new Dictionary<string, UnityEngine.Object>();
        _objects.Add(typeof(T), dict);

        foreach (string name in names)
        {
            UnityEngine.Object obj = transform.FindChild<T>(name);
            if (obj == null)
            {
                Debug.LogError($"Failed to bind({name})");
                continue;
            }
            dict.Add(name, obj);
        }
    }

    protected T Get<T>(Enum enumType) where T : UnityEngine.Object
    {
        if (enumType == null)
        {
            Debug.LogError("Enum is null");
            return null;
        }

        string enumName = enumType.ToString();
        
        Dictionary<string, UnityEngine.Object> dict = null;
        if (!_objects.TryGetValue(typeof(T), out dict))
        {
            Debug.LogError($"No objects found for type {typeof(T)}");
            return null;
        }

        UnityEngine.Object obj = null;
        if (!dict.TryGetValue(enumName, out obj))
        {
            Debug.LogError($"Object not found for enum {enumName}");
            return null;
        }

        return obj as T;
    }

    public void BindEvent(GameObject go, Action action = null, Action<PointerEventData> dragAction = null, EUIEvent type = EUIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case EUIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
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
} 
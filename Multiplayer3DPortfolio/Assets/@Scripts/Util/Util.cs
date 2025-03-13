using System;
using UnityEngine;
using Object = UnityEngine.Object;
public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            Transform transform = go.transform.Find(name);
            if (transform != null)
                return transform.GetComponent<T>();
        }
        else
        {
            return go.transform.FindChild<T>(name);
        }

        return null;
    }

    public static T FindAncestor<T>(GameObject go) where T : Object
    {
        Transform t = go.transform;
        while (t != null)
        {
            T component = t.GetComponent<T>();
            if (component != null)
                return component;
            t = t.parent;
        }
        return null;
    }
}
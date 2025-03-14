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

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false, bool includeInactive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive, includeInactive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false, bool includeInactive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (includeInactive == false)
        {
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
        }
        else
        {
            // 비활성화 되어 있어도 찾을 수 있도록
            Transform[] childrenTransforms = go.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in childrenTransforms)
            {
                if (name == null || child.name == name)
                {
                    T component = child.GetComponent<T>();
                    if (component != null)
                        return component;
                    
                    if (!recursive && child.parent != go.transform)
                        continue;
                }
            }
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
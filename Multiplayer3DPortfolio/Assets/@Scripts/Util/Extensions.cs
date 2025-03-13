using UnityEngine;

public static class Extensions
{
    public static T FindChild<T>(this Transform transform, string name, bool recursive = true) where T : UnityEngine.Object
    {
        Transform found = recursive ? transform.RecursiveFindChild(name) : 
                                    transform.Find(name);
        if (found == null) return null;
        
        if (typeof(T) == typeof(GameObject))
            return found.gameObject as T;
        
        return found.GetComponent<T>();
    }

    public static Transform RecursiveFindChild(this Transform transform, string childName)
    {
        if (transform == null) return null;

        foreach (Transform child in transform)
        {
            if (child.name == childName)
                return child;

            Transform found = child.RecursiveFindChild(childName);
            if (found != null)
                return found;
        }

        return null;
    }
    
    public static T FindComponentInChildren<T>(this Transform transform, string name, bool includeInactive = false) where T : Component
    {
        // 비활성화된 오브젝트도 검색할지 여부에 따라 다른 메서드 사용
        if (includeInactive)
        {
            // 모든 자식 Transform을 한 번에 가져옴
            Transform[] children = transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == name)
                {
                    T component = child.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            // 활성화된 오브젝트만 검색
            return FindComponentInChildrenActive<T>(transform, name);
        }

        return null;
    }

    private static T FindComponentInChildrenActive<T>(Transform transform, string name) where T : Component
    {
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf)
                continue;

            if (child.name == name)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                    return component;
            }

            T childComponent = FindComponentInChildrenActive<T>(child, name);
            if (childComponent != null)
                return childComponent;
        }

        return null;
    }
} 
using UnityEngine;

public static class Extensions
{
    public static T FindChild<T>(this Transform transform, string name) where T : UnityEngine.Object
    {
        return FindChild<T>(transform, name, includeInactive: true, recursive: true);
    }

    public static T FindChild<T>(this Transform transform, string name, bool includeInactive = false, bool recursive = false) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(name))
            return null;
        
        // 첫 번째 경우: 재귀적으로 검색
        if (recursive)
        {
            // 모든 자식 Transform을 검색
            Transform[] childTransforms = transform.GetComponentsInChildren<Transform>(includeInactive);
            
            foreach (Transform childTransform in childTransforms)
            {
                // 자기 자신은 제외
                if (childTransform == transform)
                    continue;
                    
                // 이름이 일치하는지 확인
                if (childTransform.name == name)
                {
                    if (typeof(T) == typeof(GameObject))
                        return childTransform.gameObject as T;
                    else
                        return childTransform.GetComponent<T>();
                }
            }
        }
        // 두 번째 경우: 직접적인 자식만 검색
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform childTransform = transform.GetChild(i);
                
                // 비활성화 상태 확인
                if (!includeInactive && !childTransform.gameObject.activeSelf)
                    continue;
                    
                // 이름이 일치하는지 확인
                if (childTransform.name == name)
                {
                    if (typeof(T) == typeof(GameObject))
                        return childTransform.gameObject as T;
                    else
                        return childTransform.GetComponent<T>();
                }
            }
        }
        
        return null;
    }


    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static T FindAncestor<T>(this GameObject gameObject, GameObject go) where T : Object
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
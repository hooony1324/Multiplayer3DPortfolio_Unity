using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public delegate void ClickEvent(Vector3 position, GameObject hitObject);
    
    public event ClickEvent OnLeftClick = (position, hitObject) => {};
    public event ClickEvent OnRightClick = (position, hitObject) => {};

    private Camera _mainCamera;
    
    public void Init()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // 충돌 지점의 월드 좌표
                Vector3 hitPoint = hit.point;
                //Debug.Log("월드 좌표: " + hitPoint);
                
                // 충돌한 객체
                GameObject hitObject = hit.collider.gameObject;
                //Debug.Log("충돌한 객체: " + hitObject.name);

                OnRightClick.Invoke(hitPoint, hitObject);
            }
            
        }
    }
}

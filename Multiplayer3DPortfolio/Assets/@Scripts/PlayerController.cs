using Pathfinding;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    IAstarAI _ai;

    void Awake()
    {
        _ai = GetComponent<IAstarAI>();
        

        
        Managers.InputManager.OnRightClick += Move;
    }

    void Move(Vector3 position, GameObject hitObject)
    {
        _ai.destination = position;
        _ai.isStopped = false;
        _ai.SearchPath();
    }



}

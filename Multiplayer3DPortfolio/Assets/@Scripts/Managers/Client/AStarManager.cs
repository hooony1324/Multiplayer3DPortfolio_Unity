using UnityEngine;
using Pathfinding;

// https://www.arongranberg.com/astar/docs/graphupdates.html#what
public class AStarManager : MonoBehaviour
{
    private ProceduralGraphMover _proceduralGraphmover;

    private void Awake()
    {
        _proceduralGraphmover = GetComponent<ProceduralGraphMover>();
        _proceduralGraphmover.enabled = false;
    }

    public void Init()
    {
        
    }

    private void ActivateGraphMover()
    {
        AstarPath.active.Scan();

        // TODO : NetPlayer중 Owner가 있는 플레이어의 위치를 찾아서 타겟으로 설정
        _proceduralGraphmover.target = FindFirstObjectByType<PlayerController>().transform;
        _proceduralGraphmover.enabled = true;
    }




}

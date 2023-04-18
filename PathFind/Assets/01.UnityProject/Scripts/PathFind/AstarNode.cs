using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarNode
{
    public TerrainControler Terrain { get; private set; }
    public GameObject DestinationObj { get; private set; }

    // A star algorithm
    public float AstarF { get; private set; } = float.MaxValue;
    public float AstarG { get; private set; } = float.MaxValue;
    public float AstarH { get; private set; } = float.MaxValue;

    public AstarNode AstarPrevNode { get; private set; } = default;

    public AstarNode(TerrainControler terrain_, GameObject destinationObj_)
    {
        Terrain = terrain_;
        DestinationObj = destinationObj_;
    }

    //! Astar �˰����� ����� ����� �����Ѵ�.
    public void UpdateCost_Astar(float gCost, float heuristic,
        AstarNode prevNode)
    {
        float aStarF = gCost + heuristic;

        if(aStarF < AstarF)
        {
            AstarG = gCost;
            AstarH = heuristic;
            AstarF = aStarF;

            AstarPrevNode = prevNode;
        }       // if: ����� �� ���� ��쿡�� ������Ʈ �Ѵ�.
        else { /* Do nothing */ }

    }       // UpdateCost_Astar()

    //! ������ ����� ����Ѵ�.
    public void ShowCost_Astar()
    {
        GFunc.Log("TileIdx1D: {0}, 2D: {1}, F: {2}, G: {3}, H: {4}", 
            Terrain.TileIdx1D, Terrain.TileIdx2D, AstarF, AstarG, AstarH);
    }       // ShowCost_Astar()
}
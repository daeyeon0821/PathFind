using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JpsNode : AstarNode
{
    public bool IsJumpPoint { get; private set; } = false;
    public JpsNode JpsPrevNode { get; private set; } = default;

    public JpsNode(TerrainControler terrain_, GameObject destinationObj_, bool isJumpPoint_) :
        base(terrain_, destinationObj_)
    {
        IsJumpPoint = isJumpPoint_;
    }

    public override void UpdateCost_Astar<Node>(float gCost, float heuristic, 
        Node prevNode)
    {
        float aStarF = gCost + heuristic;

        if (aStarF < AstarF)
        {
            this.AstarG = gCost;
            AstarH = heuristic;
            AstarF = aStarF;

            JpsPrevNode = (prevNode as JpsNode);
        }       // if: 비용이 더 작은 경우에만 업데이트 한다.
        else { /* Do nothing */ }
    }
}

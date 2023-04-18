using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JpsNode : AstarNode
{
    public bool IsJumpPoint { get; private set; } = false;
    public JpsNode(TerrainControler terrain_, GameObject destinationObj_, bool isJumpPoint_) :
        base(terrain_, destinationObj_)
    {
        IsJumpPoint = isJumpPoint_;
    }
}

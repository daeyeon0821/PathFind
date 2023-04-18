using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class GData
{
    //! Ž�� ������ ����
    public enum GridDirection
    {
        NONE = -1,
        EAST, WEST, SOUTH, NORTH
    }
}

//! ������ �Ӽ��� �����ϱ� ���� Ÿ��
public enum TerrainType
{
    NONE = -1, 
    PLAIN_PASS,
    OCEAN_N_PASS
}       // TerrainType

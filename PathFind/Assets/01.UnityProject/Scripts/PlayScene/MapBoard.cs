using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBoard : MonoBehaviour
{
    private const string TERRAIN_MAP_OBJ_NAME = "TerrainMap";
    private const string OBSTACLE_MAP_OBJ_NAME = "ObstacleMap";

    public Vector2Int MapCellSize { get; private set; } = default;
    public Vector2 MapCellGap { get; private set; } = default;

    public float pathFindDelay = 1.0f;      /**< brief Delay for looking path find slowly */

    private TerrainMap terrainMap = default;
    private ObstacleMap obstacleMap = default;

    private void Awake()
    {
        // { ���� �Ŵ����� ��� �ʱ�ȭ�Ѵ�.
        ResManager.Instance.Create();
        PathFinder.Instance.Create();
        // } ���� �Ŵ����� ��� �ʱ�ȭ�Ѵ�.

        // PathFinder �� �� ���� ��Ʈ�ѷ��� ĳ���Ѵ�.
        PathFinder.Instance.mapBoard = this;

        // �ʿ� ������ �ʱ�ȭ�Ͽ� ��ġ�Ѵ�.
        terrainMap = gameObject.FindChildComponent<TerrainMap>(TERRAIN_MAP_OBJ_NAME);
        terrainMap.InitAwake(this);
        MapCellSize = terrainMap.GetCellSize();
        MapCellGap = terrainMap.GetCellGap();

        // �ʿ� ������ �ʱ�ȭ�Ͽ� ��ġ�Ѵ�.
        obstacleMap = gameObject.FindChildComponent<ObstacleMap>(OBSTACLE_MAP_OBJ_NAME);
        obstacleMap.InitAwake(this);
    }       // Awake()

    //! ���� ���带 �����ϴ� �Լ�
    public void ResetMapBoard()
    {
        terrainMap.ShuffleTileMap();
        obstacleMap.ResetObstacleMap();
    }

    //! Ÿ�� �ε����� �޾Ƽ� �ش� Ÿ���� �����ϴ� �Լ�
    public TerrainControler GetTerrain(int idx1D)
    {
        return terrainMap.GetTile(idx1D);
    }       // GetTerrain()

    //! ���� x ��ǥ�� �޾Ƽ� �ش� ���� Ÿ���� ����Ʈ�� �������� �Լ�
    public List<TerrainControler> GetTerrains_Colum(int xIdx2D)
    {
        return GetTerrains_Colum(xIdx2D, false);
    }       // GetTerrains_Colum()

    //! ���� x ��ǥ�� �޾Ƽ� �ش� ���� Ÿ���� ����Ʈ�� �������� �Լ�
    public List<TerrainControler> GetTerrains_Colum(int xIdx2D, bool isSortReverse)
    {
        List<TerrainControler> terrains = new List<TerrainControler>();
        TerrainControler tempTile = default;
        int tileIdx1D = 0;
        for(int y = 0; y < MapCellSize.y; y++)
        {
            tileIdx1D = y * MapCellSize.x + xIdx2D;

            // �ش� �ε����� Ÿ���� �����ϴ� ��� ����Ʈ�� �߰��Ѵ�.
            tempTile = terrainMap.GetTile(tileIdx1D);
            if (tempTile.IsValid() == false) { continue; }

            terrains.Add(tempTile);
        }       // loop: y ���� ũ�⸸ŭ ��ȸ�ϴ� ����

        if(terrains.IsValid())
        {
            // �Ʒ����� ���� ĳ���� ������, ������ �Ʒ��� �������̴�.
            if(isSortReverse) { terrains.Reverse(); }
            else { /* Do nothing */ }

            return terrains;
        }
        else { return default; }
    }       // GetTerrains_Colum()

    //! ���� y ��ǥ�� �޾Ƽ� �ش� ���� Ÿ���� ����Ʈ�� �������� �Լ�
    public List<TerrainControler> GetTerrains_Row(int yIdx2D)
    {
        return GetTerrains_Row(yIdx2D, false);
    }       // GetTerrains_Row()

    //! ���� y ��ǥ�� �޾Ƽ� �ش� ���� Ÿ���� ����Ʈ�� �������� �Լ�
    public List<TerrainControler> GetTerrains_Row(int yIdx2D, bool isSortReverse)
    {
        List<TerrainControler> terrains = new List<TerrainControler>();
        TerrainControler tempTile = default;
        int tileIdx1D = 0;
        for (int x = 0; x < MapCellSize.x; x++)
        {
            tileIdx1D = yIdx2D * MapCellSize.x + x;

            // �ش� �ε����� Ÿ���� �����ϴ� ��� ����Ʈ�� �߰��Ѵ�.
            tempTile = terrainMap.GetTile(tileIdx1D);
            if (tempTile.IsValid() == false) { continue; }

            terrains.Add(tempTile);
        }       // loop: x ���� ũ�⸸ŭ ��ȸ�ϴ� ����

        if (terrains.IsValid())
        {
            // ���ʿ��� ������ ĳ���� ������, �����ʿ��� ������ �������̴�.
            if (isSortReverse) { terrains.Reverse(); }
            else { /* Do nothing */ }

            return terrains;
        }
        else { return default; }
    }       // GetTerrains_Row()

    //! ������ �ε����� 2D ��ǥ�� �����ϴ� �Լ�
    public Vector2Int GetTileIdx2D(int idx1D)
    {
        Vector2Int tileIdx2D = Vector2Int.zero;
        tileIdx2D.x = idx1D % MapCellSize.x;
        tileIdx2D.y = idx1D / MapCellSize.x;

        return tileIdx2D;
    }       // GetTileIdx2D()

    //! ������ 2D ��ǥ�� �ε����� �����ϴ� �Լ�
    public int GetTileIdx1D(Vector2Int idx2D)
    {
        int tileIdx1D = -1;
        if(idx2D.x < 0 || idx2D.y < 0) { return tileIdx1D; }
        if(MapCellSize.x <= idx2D.x || MapCellSize.y <= idx2D.y) { return tileIdx1D; }

        tileIdx1D = (idx2D.y * MapCellSize.x) + idx2D.x;
        return tileIdx1D;
    }       // GetTileIdx1D()

    //! �� ���� ������ Ÿ�� �Ÿ��� �����ϴ� �Լ�
    public Vector2Int GetDistance2D(GameObject targetTerrainObj,
        GameObject destTerrainObj)
    {
        Vector2 localDistance = destTerrainObj.transform.localPosition -
            targetTerrainObj.transform.localPosition;
        
        Vector2Int distance2D = Vector2Int.zero;
        distance2D.x = Mathf.RoundToInt(localDistance.x / MapCellGap.x);
        distance2D.y = Mathf.RoundToInt(localDistance.y / MapCellGap.y);

        distance2D = GFunc.Abs(distance2D);

        return distance2D;
    }       // GetDistance2D()

    //! 2D ��ǥ�� �������� �ֺ� 4���� Ÿ���� �ε����� �����ϴ� �Լ�
    public List<int> GetTileIdx2D_Around4ways(Vector2Int targetIdx2D)
    {
        List<int> idx1D_around4ways = new List<int>();
        List<Vector2Int> idx2D_around4ways = new List<Vector2Int>();
        idx2D_around4ways.Add(new Vector2Int(targetIdx2D.x - 1, targetIdx2D.y));
        idx2D_around4ways.Add(new Vector2Int(targetIdx2D.x + 1, targetIdx2D.y));
        idx2D_around4ways.Add(new Vector2Int(targetIdx2D.x, targetIdx2D.y - 1));
        idx2D_around4ways.Add(new Vector2Int(targetIdx2D.x, targetIdx2D.y + 1));

        foreach(var idx2D in idx2D_around4ways)
        {
            // idx2D �� ��ȿ���� �˻��Ѵ�.
            if(idx2D.x.IsInRange(0, MapCellSize.x) == false) { continue; }
            if(idx2D.y.IsInRange(0, MapCellSize.y) == false) { continue; }

            idx1D_around4ways.Add(GetTileIdx1D(idx2D));
        }

        return idx1D_around4ways;
    }       // GetTileIdx2D_Around4ways()

    //! Terrain ����Ʈ�� �޾Ƽ� TargetIdx �������� 2���� ����Ʈ�� Split �ϴ� �Լ�
    public List<TerrainControler>[] SplitTerrains(List<TerrainControler> terrains_, Vector2Int targetIdx2D)
    {
        List<TerrainControler>[] resultTerrains = new List<TerrainControler>[2];

        int targetIdx = -1;
        for(int i=0; i<terrains_.Count; i++)
        {
            if (terrains_[i].TileIdx2D.Equals(targetIdx2D)) 
            { 
                targetIdx = i;
                break;
            }
        }       // loop: targetIdx �� ã�� ����
        if(targetIdx.Equals(-1)) { return default; }

        resultTerrains[0] = terrains_.GetRange(0, targetIdx);

        //// DEBUG:
        //foreach(TerrainControler terrain_ in resultTerrains[0])
        //{
        //    GFunc.Log("Result 0 idx: {0}", terrain_.TileIdx1D);
        //}

        // Target tile �� �����ϱ� ���ؼ� Range count ����  1 �� ����.
        int maxRange = terrains_.GetRangeCount(targetIdx) - 1;
        if (terrains_.IsValid(maxRange))
        {
            resultTerrains[1] = terrains_.GetRange(targetIdx + 1, maxRange);

            //// DEBUG:
            //foreach (TerrainControler terrain_ in resultTerrains[1])
            //{
            //    GFunc.Log("Result 1 idx: {0}", terrain_.TileIdx1D);
            //}
        }
        else { resultTerrains[1] = default; }

        return resultTerrains;
    }       // SplitTerrains()

    //! Terrain ����Ʈ�� �޾Ƽ� �̵� �Ұ����� ������ ������ ���� Ÿ���� �����ϴ� �Լ�
    public void RemoveTerrains_NonPassableTileAfter(ref List<TerrainControler> terrains_)
    {
        if(terrains_.IsValid() == false) { return; }

        int targetIdx = -1;
        for(int i=0; i<terrains_.Count; i++)
        {
            if (terrains_[i].IsPassable == false)
            {
                targetIdx = i;
                break;
            }
            else { continue; }
        }       // loop: �̵� �Ұ����� ������ ã�� ����

        if(terrains_.IsValid(targetIdx) == false) { return; }

        //// DEBUG:
        //foreach (TerrainControler t in terrains_)
        //{
        //    GFunc.Log("Remove before idx: {0}", t.TileIdx1D);
        //}

        int rangeCnt = terrains_.GetRangeCount(targetIdx);
        terrains_.RemoveRange(targetIdx, rangeCnt);

        //// DEBUG:
        //foreach(TerrainControler t in terrains_)
        //{
        //    GFunc.Log("Remove after idx: {0}", t.TileIdx1D);
        //}
    }       // RemoveTerrains_NonPassableTileAfter()

    #region Jps �˰���
    //! Search index �� Ž�� ������ �޾Ƽ� Jump point �� �����ϴ� �Լ�
    public TerrainControler GetJumpPoint(List<TerrainControler> searchTerrains_, GData.GridDirection direction_)
    {
        return terrainMap.GetJumpPoint(searchTerrains_, direction_);
    }       // GetJumpPoint()
    #endregion      // Jps �˰���
}

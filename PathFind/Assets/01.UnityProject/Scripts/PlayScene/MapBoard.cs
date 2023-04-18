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
        // { 각종 매니저를 모두 초기화한다.
        ResManager.Instance.Create();
        PathFinder.Instance.Create();
        // } 각종 매니저를 모두 초기화한다.

        // PathFinder 에 맵 보드 컨트롤러를 캐싱한다.
        PathFinder.Instance.mapBoard = this;

        // 맵에 지형을 초기화하여 배치한다.
        terrainMap = gameObject.FindChildComponent<TerrainMap>(TERRAIN_MAP_OBJ_NAME);
        terrainMap.InitAwake(this);
        MapCellSize = terrainMap.GetCellSize();
        MapCellGap = terrainMap.GetCellGap();

        // 맵에 지물을 초기화하여 배치한다.
        obstacleMap = gameObject.FindChildComponent<ObstacleMap>(OBSTACLE_MAP_OBJ_NAME);
        obstacleMap.InitAwake(this);
    }       // Awake()

    //! 맵의 보드를 리셋하는 함수
    public void ResetMapBoard()
    {
        terrainMap.ShuffleTileMap();
        obstacleMap.ResetObstacleMap();
    }

    //! 타일 인덱스를 받아서 해당 타일을 리턴하는 함수
    public TerrainControler GetTerrain(int idx1D)
    {
        return terrainMap.GetTile(idx1D);
    }       // GetTerrain()

    //! 맵의 x 좌표를 받아서 해당 열의 타일을 리스트로 가져오는 함수
    public List<TerrainControler> GetTerrains_Colum(int xIdx2D)
    {
        return GetTerrains_Colum(xIdx2D, false);
    }       // GetTerrains_Colum()

    //! 맵의 x 좌표를 받아서 해당 열의 타일을 리스트로 가져오는 함수
    public List<TerrainControler> GetTerrains_Colum(int xIdx2D, bool isSortReverse)
    {
        List<TerrainControler> terrains = new List<TerrainControler>();
        TerrainControler tempTile = default;
        int tileIdx1D = 0;
        for(int y = 0; y < MapCellSize.y; y++)
        {
            tileIdx1D = y * MapCellSize.x + xIdx2D;

            // 해당 인덱스의 타일이 존재하는 경우 리스트에 추가한다.
            tempTile = terrainMap.GetTile(tileIdx1D);
            if (tempTile.IsValid() == false) { continue; }

            terrains.Add(tempTile);
        }       // loop: y 열의 크기만큼 순회하는 루프

        if(terrains.IsValid())
        {
            // 아래에서 위로 캐싱이 정방향, 위에서 아래가 역방향이다.
            if(isSortReverse) { terrains.Reverse(); }
            else { /* Do nothing */ }

            return terrains;
        }
        else { return default; }
    }       // GetTerrains_Colum()

    //! 맵의 y 좌표를 받아서 해당 행의 타일을 리스트로 가져오는 함수
    public List<TerrainControler> GetTerrains_Row(int yIdx2D)
    {
        return GetTerrains_Row(yIdx2D, false);
    }       // GetTerrains_Row()

    //! 맵의 y 좌표를 받아서 해당 행의 타일을 리스트로 가져오는 함수
    public List<TerrainControler> GetTerrains_Row(int yIdx2D, bool isSortReverse)
    {
        List<TerrainControler> terrains = new List<TerrainControler>();
        TerrainControler tempTile = default;
        int tileIdx1D = 0;
        for (int x = 0; x < MapCellSize.x; x++)
        {
            tileIdx1D = yIdx2D * MapCellSize.x + x;

            // 해당 인덱스의 타일이 존재하는 경우 리스트에 추가한다.
            tempTile = terrainMap.GetTile(tileIdx1D);
            if (tempTile.IsValid() == false) { continue; }

            terrains.Add(tempTile);
        }       // loop: x 행의 크기만큼 순회하는 루프

        if (terrains.IsValid())
        {
            // 왼쪽에서 오른쪽 캐싱이 정방향, 오른쪽에서 왼쪽이 역방향이다.
            if (isSortReverse) { terrains.Reverse(); }
            else { /* Do nothing */ }

            return terrains;
        }
        else { return default; }
    }       // GetTerrains_Row()

    //! 지형의 인덱스를 2D 좌표로 리턴하는 함수
    public Vector2Int GetTileIdx2D(int idx1D)
    {
        Vector2Int tileIdx2D = Vector2Int.zero;
        tileIdx2D.x = idx1D % MapCellSize.x;
        tileIdx2D.y = idx1D / MapCellSize.x;

        return tileIdx2D;
    }       // GetTileIdx2D()

    //! 지형의 2D 좌표를 인덱스로 리턴하는 함수
    public int GetTileIdx1D(Vector2Int idx2D)
    {
        int tileIdx1D = -1;
        if(idx2D.x < 0 || idx2D.y < 0) { return tileIdx1D; }
        if(MapCellSize.x <= idx2D.x || MapCellSize.y <= idx2D.y) { return tileIdx1D; }

        tileIdx1D = (idx2D.y * MapCellSize.x) + idx2D.x;
        return tileIdx1D;
    }       // GetTileIdx1D()

    //! 두 지형 사이의 타일 거리를 리턴하는 함수
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

    //! 2D 좌표를 기준으로 주변 4방향 타일의 인덱스를 리턴하는 함수
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
            // idx2D 가 유효한지 검사한다.
            if(idx2D.x.IsInRange(0, MapCellSize.x) == false) { continue; }
            if(idx2D.y.IsInRange(0, MapCellSize.y) == false) { continue; }

            idx1D_around4ways.Add(GetTileIdx1D(idx2D));
        }

        return idx1D_around4ways;
    }       // GetTileIdx2D_Around4ways()

    //! Terrain 리스트를 받아서 TargetIdx 기준으로 2개의 리스트로 Split 하는 함수
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
        }       // loop: targetIdx 를 찾는 루프
        if(targetIdx.Equals(-1)) { return default; }

        resultTerrains[0] = terrains_.GetRange(0, targetIdx);

        //// DEBUG:
        //foreach(TerrainControler terrain_ in resultTerrains[0])
        //{
        //    GFunc.Log("Result 0 idx: {0}", terrain_.TileIdx1D);
        //}

        // Target tile 을 제외하기 위해서 Range count 에서  1 을 뺀다.
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

    //! Terrain 리스트를 받아서 이동 불가능한 지역을 포함한 뒤의 타일을 제거하는 함수
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
        }       // loop: 이동 불가능한 지역을 찾는 루프

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

    #region Jps 알고리즘
    //! Search index 와 탐색 방향을 받아서 Jump point 를 리턴하는 함수
    public TerrainControler GetJumpPoint(List<TerrainControler> searchTerrains_, GData.GridDirection direction_)
    {
        return terrainMap.GetJumpPoint(searchTerrains_, direction_);
    }       // GetJumpPoint()
    #endregion      // Jps 알고리즘
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMap : TileMapControler
{
    private const string TERRAIN_TILEMAP_OBJ_NAME = "TerrainTilemap";
    private const float NON_PASSABLE_PERCENTAGE = 20.0f;

    private Vector2Int mapCellSize = default;
    private Vector2 mapCellGap = default;

    private List<TerrainControler> allTerrains = default;

    //! Awake 타임에 초기화 할 내용을 재정의한다.
    public override void InitAwake(MapBoard mapControler_)
    {
        this.tileMapObjName = TERRAIN_TILEMAP_OBJ_NAME;
        base.InitAwake(mapControler_);

        allTerrains = new List<TerrainControler>();

        // { 타일의 x축 갯수와 전체 타일의 수로 맵의 가로, 세로 사이즈를 연산한다.
        mapCellSize = Vector2Int.zero;
        float tempTileY = allTileObjs[0].transform.localPosition.y;
        for(int i=0; i < allTileObjs.Count; i++)
        {
            if (tempTileY.IsEquals(allTileObjs[i].transform.localPosition.y) == false)
            {
                mapCellSize.x = i;
                break;
            }       // if: 첫번째 타일의 y 좌표와 달라지는 지점 전까지가 맵의 가로 셀 크기이다.
        }
        // 전체 타일의 수를 맵의 가로 셀 크기로 나눈 값이 맵의 세로 셀 크기이다.
        mapCellSize.y = Mathf.FloorToInt(allTileObjs.Count / mapCellSize.x);

        // } 타일의 x축 갯수와 전체 타일의 수로 맵의 가로, 세로 사이즈를 연산한다.

        // { x 축 상의 두 타일과, y 축 상의 두 타일 사이의 로컬 포지션으로 타일 갭을 연산한다.
        mapCellGap = Vector2.zero;
        mapCellGap.x = allTileObjs[1].transform.localPosition.x -
            allTileObjs[0].transform.localPosition.x;
        mapCellGap.y = allTileObjs[mapCellSize.x].transform.localPosition.y -
            allTileObjs[0].transform.localPosition.y;
        // } x 축 상의 두 타일과, y 축 상의 두 타일 사이의 로컬 포지션으로 타일 갭을 연산한다.

    }       // InitAwake()

    private void Start()
    {
        ChangeTileObjsUsePercentage(RDefine.TERRAIN_PREF_OCEAN, NON_PASSABLE_PERCENTAGE);
        CachingAllTerrains();
    }       // Start()

    //! 타일 맵을 새로 섞는다
    public void ShuffleTileMap()
    {
        ChangeTileObjsUsePercentage(RDefine.TERRAIN_PREF_PLAIN, 100.0f);
        ChangeTileObjsUsePercentage(RDefine.TERRAIN_PREF_OCEAN, NON_PASSABLE_PERCENTAGE);
        CachingAllTerrains();

        //GFunc.Log("TerrainMap shuffled ok");
    }       // ShuffleTileMap()

    //! 초기화된 타일의 정보로 연산한 맵의 가로, 세로 크기를 리턴한다.
    public Vector2Int GetCellSize() { return mapCellSize; }

    //! 초기화된 타일의 정보로 연산한 타일 사이의 갭을 리턴한다.
    public Vector2 GetCellGap() { return mapCellGap; }

    //! 인덱스에 해당하는 타일을 리턴한다.
    public TerrainControler GetTile(int tileIdx1D)
    {
        if (allTerrains.IsValid(tileIdx1D))
        {
            return allTerrains[tileIdx1D];
        }
        return default;
    }       // GetTile()

    //! 타일맵의 일부를 일정 확률로 다른 타일로 교체하는 로직
    private void ChangeTileObjsUsePercentage(string changeTerrainType_, float changePercentage_)
    {
        GameObject changeTilePrefab = ResManager.Instance.
            terrainPrefabs[changeTerrainType_];
        // 타일맵 중에 어느 정도를 교체할 것인지 결정한다.
        float correctChangePercentage =
            allTileObjs.Count * (changePercentage_ / 100.0f);
        //// DEBUG:
        //GFunc.Log("correctChangePercentage: {0}, allTileObjs.Count: {1}", 
        //    correctChangePercentage, allTileObjs.Count);

        // 교체할 타일의 정보를 리스트 형태로 생성해서 섞는다.
        List<int> changedTileResult = GFunc.CreateList(allTileObjs.Count, 1);
        if(changePercentage_.IsEquals(100.0f) == false)
        {
            changedTileResult.Shuffle();
        }
        else { /* Passed shuffle logic */ }

        GameObject tempChangeTile = default;
        for (int i = 0; i < allTileObjs.Count; i++)
        {
            if (correctChangePercentage < changedTileResult[i]) { continue; }

            // 프리팹을 인스턴스화해서 교체할 타일의 트랜스폼을 카피한다.
            tempChangeTile = Instantiate(
                changeTilePrefab, tileMap.transform);
            tempChangeTile.name = changeTilePrefab.name;
            tempChangeTile.SetLocalScale(allTileObjs[i].transform.localScale);
            tempChangeTile.SetLocalPos(allTileObjs[i].transform.localPosition);

            allTileObjs.Swap(ref tempChangeTile, i);
            tempChangeTile.DestroyObj();
        }       // loop: 위에서 연산한 정보로 현재 타일맵에 바꿀 타일을 적용하는 루프
    }       // ChangeTerrainsUsePercentage()

    //! 기존에 존재하는 타일의 정렬 순서를 조정하고, 컨트롤러를 캐싱하는 로직
    private void CachingAllTerrains()
    {
        // 캐싱 하기 전에 allTerrains 를 클리어한다.
        allTerrains.Clear();

        TerrainControler tempTerrain = default;
        TerrainType terrainType = TerrainType.NONE;

        int loopCnt = 0;
        foreach (GameObject tile_ in allTileObjs)
        {
            tempTerrain = tile_.GetComponentMust<TerrainControler>();
            switch (tempTerrain.name)
            {
                case RDefine.TERRAIN_PREF_PLAIN:
                    terrainType = TerrainType.PLAIN_PASS;
                    break;
                case RDefine.TERRAIN_PREF_OCEAN:
                    terrainType = TerrainType.OCEAN_N_PASS;
                    break;
                default:
                    terrainType = TerrainType.NONE;
                    break;
            }       // switch: 지형별로 다른 설정을 한다.

            tempTerrain.SetupTerrain(mapControler, terrainType, loopCnt);
            tempTerrain.transform.SetAsFirstSibling();
            allTerrains.Add(tempTerrain);
            loopCnt += 1;
        }       // loop: 타일의 이름과 렌더링 순서대로 정렬하는 루프
    }       // CachingAllTerrains()

    #region Jps 알고리즘
    //! Search index list 와 탐색 방향을 받아서 Jump point 를 리턴하는 함수
    public TerrainControler GetJumpPoint(List<TerrainControler> searchTerrains_, GData.GridDirection direction_)
    {
        TerrainControler jumpPoint = default;
        foreach(TerrainControler searchTerrain_ in searchTerrains_)
        {
            jumpPoint = GetJumpPoint(searchTerrain_.TileIdx2D, direction_);
            if(jumpPoint.IsValid() && jumpPoint.IsPassable) { return jumpPoint; }
            else { continue; }
        }   // loop: 탐색할 리스트를 순회하는 루프

        jumpPoint = default;
        return jumpPoint;
    }       // GetJumpPoint()

    //! Search index 와 탐색 방향을 받아서 Jump point 를 리턴하는 함수
    private TerrainControler GetJumpPoint(Vector2Int searchIdx, GData.GridDirection direction_)
    {
        Vector2Int nonPassableIdx2D_Left = default;
        Vector2Int nonPassableIdx2D_Right = default;
        Vector2Int searchCornerIdx2D_Left = default;
        Vector2Int searchCornerIdx2D_Right = default;
        Vector2Int jumpPointIdx2D = default;
        
        TerrainControler nonPassableTile_Left = default;
        TerrainControler nonPassableTile_Right = default;
        TerrainControler searchCornerTile_Left = default;
        TerrainControler searchCornerTile_Right = default;
        TerrainControler jumpPoint = default;

        // { 탐색할 타일의 인덱스를 정의한다.
        switch (direction_)
        {
            case GData.GridDirection.SOUTH:
                nonPassableIdx2D_Left = new Vector2Int(searchIdx.x + 1, searchIdx.y);
                nonPassableIdx2D_Right = new Vector2Int(searchIdx.x - 1, searchIdx.y);
                searchCornerIdx2D_Left = new Vector2Int(nonPassableIdx2D_Left.x, nonPassableIdx2D_Left.y - 1);
                searchCornerIdx2D_Right = new Vector2Int(nonPassableIdx2D_Right.x, nonPassableIdx2D_Right.y - 1);
                jumpPointIdx2D = new Vector2Int(searchIdx.x, searchIdx.y - 1);
                break;
            case GData.GridDirection.NORTH:
                nonPassableIdx2D_Left = new Vector2Int(searchIdx.x - 1, searchIdx.y);
                nonPassableIdx2D_Right = new Vector2Int(searchIdx.x + 1, searchIdx.y);
                searchCornerIdx2D_Left = new Vector2Int(nonPassableIdx2D_Left.x, nonPassableIdx2D_Left.y + 1);
                searchCornerIdx2D_Right = new Vector2Int(nonPassableIdx2D_Right.x, nonPassableIdx2D_Right.y + 1);
                jumpPointIdx2D = new Vector2Int(searchIdx.x, searchIdx.y + 1);
                break;
            case GData.GridDirection.WEST:
                nonPassableIdx2D_Left = new Vector2Int(searchIdx.x, searchIdx.y - 1);
                nonPassableIdx2D_Right = new Vector2Int(searchIdx.x, searchIdx.y + 1);
                searchCornerIdx2D_Left = new Vector2Int(nonPassableIdx2D_Left.x - 1, nonPassableIdx2D_Left.y);
                searchCornerIdx2D_Right = new Vector2Int(nonPassableIdx2D_Right.x - 1, nonPassableIdx2D_Right.y);
                jumpPointIdx2D = new Vector2Int(searchIdx.x - 1, searchIdx.y);
                break;
            case GData.GridDirection.EAST:
                nonPassableIdx2D_Left = new Vector2Int(searchIdx.x, searchIdx.y + 1);
                nonPassableIdx2D_Right = new Vector2Int(searchIdx.x, searchIdx.y - 1);
                searchCornerIdx2D_Left = new Vector2Int(nonPassableIdx2D_Left.x + 1, nonPassableIdx2D_Left.y);
                searchCornerIdx2D_Right = new Vector2Int(nonPassableIdx2D_Right.x + 1, nonPassableIdx2D_Right.y);
                jumpPointIdx2D = new Vector2Int(searchIdx.x + 1, searchIdx.y);
                break;
            default:
                // 정해지지 않은 방향은 탐색하지 않는다.
                jumpPoint = default;
                return jumpPoint;
        }       // switch: 탐색 방향에 따라서 Jump point 를 찾는다.
        // } 탐색할 타일의 인덱스를 정의한다.

        // { 캐싱한 인덱스로 코너를 검사하는 로직
        nonPassableTile_Left = GetTile(mapControler.GetTileIdx1D(nonPassableIdx2D_Left));
        if (nonPassableTile_Left.IsValid() && nonPassableTile_Left.IsPassable == false)
        {
            searchCornerTile_Left = GetTile(mapControler.GetTileIdx1D(searchCornerIdx2D_Left));
            jumpPoint = GetTile(mapControler.GetTileIdx1D(jumpPointIdx2D));

            // 직진 후 갈 수 없는 타일 방향으로 꺾을 수 있다면 코너로 간주한다.
            if ((searchCornerTile_Left.IsValid() && searchCornerTile_Left.IsPassable) &&
                (jumpPoint.IsValid() && jumpPoint.IsPassable))
            {
                //// DEBUG:
                //GFunc.Log("[{0}] Search idx: {1}, Exists left corner: {2}, {3} Jump point: {4}",
                //    direction_, searchIdx, nonPassableIdx2D_Left, nonPassableTile_Left.TileIdx2D, jumpPoint.TileIdx2D);
                //searchCornerTile_Left.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);
                //jumpPoint.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);

                return jumpPoint;
            }
        }       // if: 갈 수 없는 타일이 존재하는 경우 코너를 검사한다.

        nonPassableTile_Right = GetTile(mapControler.GetTileIdx1D(nonPassableIdx2D_Right));
        if (nonPassableTile_Right.IsValid() && nonPassableTile_Right.IsPassable == false)
        {
            searchCornerTile_Right = GetTile(mapControler.GetTileIdx1D(searchCornerIdx2D_Right));
            jumpPoint = GetTile(mapControler.GetTileIdx1D(jumpPointIdx2D));

            // 직진 후 갈 수 없는 타일 방향으로 꺾을 수 있다면 코너로 간주한다.
            if ((searchCornerTile_Right.IsValid() && searchCornerTile_Right.IsPassable) &&
                (jumpPoint.IsValid() && jumpPoint.IsPassable))
            {
                //// DEBUG:
                //GFunc.Log("[{0}] Search idx: {1}, Exists right corner: {2}, {3} Jump point: {4}",
                //    direction_, searchIdx, nonPassableIdx2D_Right, nonPassableTile_Right.TileIdx2D, jumpPoint.TileIdx2D);
                //searchCornerTile_Right.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);
                //jumpPoint.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);

                return jumpPoint;
            }
        }       // if: 갈 수 없는 타일이 존재하는 경우 코너를 검사한다.
        // } 캐싱한 인덱스로 코너를 검사하는 로직

        jumpPoint = default;
        return jumpPoint;
    }       // GetJumpPoint()
    #endregion      // Jps 알고리즘
}

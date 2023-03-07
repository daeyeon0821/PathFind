using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMap : TileMapControler
{
    private const string OBSTACLE_TILEMAP_OBJ_NAME = "ObstacleTilemap";
    private GameObject[] castleObjs = default;      //!< 길찾기 알고리즘을 테스트 할 출발지와 목적지를 캐싱한 오브젝트 배열

    //! Awake 타임에 초기화 할 내용을 재정의한다.
    public override void InitAwake(MapBoard mapControler_)
    {
        this.tileMapObjName = OBSTACLE_TILEMAP_OBJ_NAME;
        base.InitAwake(mapControler_);
    }       // InitAwake()

    private void Start()
    {
        StartCoroutine(DelayStart(0f));
    }       // Start()

    private IEnumerator DelayStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        DoStart();
    }       // DelayStart()

    private void DoStart()
    {
        // { 출발지와 목적지를 설정해서 타일을 배치한다.
        castleObjs = new GameObject[2];
        TerrainControler[] passableTerrains = new TerrainControler[2];

        List<TerrainControler> searchTerrains = default;
        int searchIdx = 0;
        TerrainControler foundTile = default;

        // 출발지는 좌측에서 우측으로 y축을 서치해서 빈 지형을 받아온다.
        searchIdx = 0;
        foundTile = default;
        while (foundTile == null || foundTile == default)
        {
            // 위에서 아래로 서치한다.
            searchTerrains = mapControler.GetTerrains_Colum(searchIdx, true);
            GFunc.Log($"Search terrains count: {searchTerrains.Count}");
            foreach (var searchTerrain in searchTerrains)
            {
                GFunc.Log($"Search terrain is null?? {searchTerrain}");
                if (searchTerrain.IsPassable)
                {
                    foundTile = searchTerrain;
                    break;
                }
                else { /* Do nothing */ }
            }

            if (foundTile != null || foundTile != default) { break; }
            if (mapControler.MapCellSize.x - 1 <= searchIdx) { break; }
            searchIdx++;
        }       // loop: 출발지를 찾는 루프
        passableTerrains[0] = foundTile;

        // 목적지는 우측에서 좌측으로 y 축을 서치해서 빈 지형을 받아온다.
        searchIdx = mapControler.MapCellSize.x - 1;
        foundTile = default;
        while (foundTile == null || foundTile == default)
        {
            // 아래에서 위로 서치한다.
            searchTerrains = mapControler.GetTerrains_Colum(searchIdx);
            foreach (var searchTerrain in searchTerrains)
            {
                if (searchTerrain.IsPassable)
                {
                    foundTile = searchTerrain;
                    break;
                }
                else { /* Do nothing */ }
            }

            if (foundTile != null || foundTile != default) { break; }
            if (searchIdx <= 0) { break; }
            searchIdx--;
        }       // loop: 목적지를 찾는 루프
        passableTerrains[1] = foundTile;

        // } 출발지와 목적지를 설정해서 타일을 배치한다.

        // { 출발지와 목적지에 지물을 추가한다.
        GameObject changeTilePrefab = ResManager.Instance.
            obstaclePrefabs[RDefine.OBSTACLE_PREF_PLAIN_CASTLE];
        GameObject tempChangeTile = default;
        for (int i = 0; i < 2; i++)
        {
            tempChangeTile = Instantiate(changeTilePrefab, tileMap.transform);
            tempChangeTile.name = string.Format("{0}_{1}",
                changeTilePrefab.name, passableTerrains[i].TileIdx1D);

            tempChangeTile.SetLocalScale(passableTerrains[i].transform.localScale);
            tempChangeTile.SetLocalPos(passableTerrains[i].transform.localPosition);

            // 출발지와 목적지를 캐싱한다.
            castleObjs[i] = tempChangeTile;
            Add_Obstacle(tempChangeTile);

            tempChangeTile = default;
        }       // loop: 출발지와 목적지를 인스턴스화 해서 캐싱하는 루프
        // } 출발지와 목적지에 지물을 추가한다.
    }       // DoStart()

    //! 지물을 추가한다.
    public void Add_Obstacle(GameObject obstacle_)
    {
        allTileObjs.Add(obstacle_);
    }       // Add_Obstacle()
}

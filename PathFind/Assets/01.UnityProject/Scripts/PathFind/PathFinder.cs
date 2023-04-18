using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : GSingleton<PathFinder>
{
    #region 지형 탐색을 위한 변수
    public GameObject sourceObj = default;
    public GameObject destinationObj = default;
    public MapBoard mapBoard = default;
    #endregion      // 지형 탐색을 위한 변수

    #region A star 알고리즘으로 최단거리를 찾기 위한 변수
    private List<AstarNode> aStarResultPath = default;
    private List<AstarNode> aStarOpenPath = default;
    private List<AstarNode> aStarClosePath = default;
    #endregion      // A star 알고리즘으로 최단거리를 찾기 위한 변수

    #region Jps 알고리즘으로 최단거리를 찾기 위한 변수
    private List<JpsNode> jpsResultPath = default;
    private List<JpsNode> jpsOpenPath = default;
    private List<JpsNode> jpsClosePath = default;
    #endregion      // Jps 알고리즘으로 최단거리를 찾기 위한 변수

    private Coroutine currentRunningAlgorithm = default;


    #region A star 알고리즘
    
    //! 출발지와 목적지 정보로 길을 찾는 함수
    public void FindPath_Astar()
    {
        float findDelay = mapBoard.pathFindDelay;
        FindPath_Astar(findDelay, sourceObj, destinationObj);
    }       // FindPath_Astar()

    private void FindPath_Astar(float delay_, GameObject sourObj_, GameObject destObj_)
    {
        Clear_Astar();

        currentRunningAlgorithm = StartCoroutine(
            DelayFindPath_Astar(delay_, sourObj_, destObj_));
    }       // FindPath_Astar()

    //! A star 알고리즘을 멈추고 노드를 정리하는 함수
    public void Clear_Astar()
    {
        // 실행중인 루틴이 없는 경우 Clear 할 것이 없다.
        if (currentRunningAlgorithm.IsValid() == false) { return; }
        
        // 루틴을 멈추고 Open list 와 Close list 를 정리한다.
        StopCoroutine(currentRunningAlgorithm);
        if (aStarOpenPath.IsValid())
        {
            foreach (AstarNode node_ in aStarOpenPath)
            {
                node_.Terrain.SetTileActiveColor(RDefine.TileStatusColor.DEFAULT);
            }
            aStarOpenPath.Clear();
        }       // if: Open list 를 정리한다.

        if(aStarClosePath.IsValid())
        {
            foreach(AstarNode node_ in aStarClosePath)
            {
                node_.Terrain.SetTileActiveColor(RDefine.TileStatusColor.DEFAULT);
            }
            aStarClosePath.Clear();
        }       // if: Close list 를 정리한다.
    }       // Stop_Astar()

    //! 탐색 알고리즘에 딜레이를 건다.
    private IEnumerator DelayFindPath_Astar(float delay_, GameObject sourceObj_, GameObject destObj_)
    {
        // A star 알고리즘을 사용하기 위해서 패스 리스트를 초기화한다.
        aStarOpenPath = new List<AstarNode>();
        aStarClosePath = new List<AstarNode>();
        aStarResultPath = new List<AstarNode>();

        TerrainControler targetTerrain = default;

        // 출발지의 인덱스를 구해서, 출발지 노드를 찾아온다.
        int sourceIdx1D = GetTileIdx1DFromObjName(sourceObj_);
        targetTerrain = mapBoard.GetTerrain(sourceIdx1D);
        // 찾아온 출발지 노드를 Open 리스트에 추가한다.
        AstarNode targetNode = new AstarNode(targetTerrain, destObj_);
        Add_AstarOpenList(targetNode);

        int loopIdx = 0;
        bool isFoundDestination = false;
        bool isNowayToGo = false;

        //// DEBUG:
        //while (loopIdx < 10)

        while (isFoundDestination == false && isNowayToGo == false)
        {
            // Open 리스트를 순회해서 가장 코스트가 낮은 노드를 선택한다.
            AstarNode minCostNode = GetMinCostNodeFromList(aStarOpenPath);

            // 선택한 노드가 목적지에 도달했는지 확인한다.
            bool isArriveDest = mapBoard.GetDistance2D(
                minCostNode.Terrain.gameObject, destObj_).
                Equals(Vector2Int.zero);

            if (isArriveDest)
            {
                // { 목적지에 도착 했다면 aStarResultPath 리스트를 설정한다.
                AstarNode resultNode = minCostNode;
                bool isSet_aStarResultPathOk = false;
                while (isSet_aStarResultPathOk == false)
                {
                    if (resultNode.AstarPrevNode == default ||
                        resultNode.AstarPrevNode == null)
                    {
                        isSet_aStarResultPathOk = true;
                        break;
                    }
                    else { /* Do nothing */ }

                    // 목적지까지의 aStarResultPath 결과를 초록색으로 표시한다.
                    resultNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SELECTED);
                    aStarResultPath.Add(resultNode);
                    resultNode = resultNode.AstarPrevNode;
                }       // loop: 이전 노드를 찾지 못할 때까지 순회하는 루프
                // } 목적지에 도착 했다면 aStarResultPath 리스트를 설정한다.

                // Open list 와 Close list 를 정리한다.
                aStarOpenPath.Clear();
                aStarClosePath.Clear();
                isFoundDestination = true;
                break;
            }       // if: 선택한 노드가 목적지에 도착한 경우
            else
            {
                // { 도착하지 않았다면 현재 타일을 기준으로 4 방향 노드를 찾아온다.
                List<int> nextSearchIdx1Ds = mapBoard.
                    GetTileIdx2D_Around4ways(minCostNode.Terrain.TileIdx2D);

                // 찾아온 노드 중에서 이동 가능한 노드는 Open list 에 추가한다.
                AstarNode nextNode = default;
                foreach (var nextIdx1D in nextSearchIdx1Ds)
                {
                    nextNode = new AstarNode(
                        mapBoard.GetTerrain(nextIdx1D), destObj_);

                    if (nextNode.Terrain.IsPassable == false) { continue; }

                    Add_AstarOpenList(nextNode, minCostNode);
                    nextNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);
                }       // loop: 이동 가능한 노드를 Open list 에 추가하는 루프

                minCostNode.ShowCost_Astar();
                minCostNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SELECTED);

                // } 도착하지 않았다면 현재 타일을 기준으로 4 방향 노드를 찾아온다.

                // 탐색이 끝난 노드는 Close list 에 추가하고, Open list 에서 제거한다.
                // 이 때, Open list 가 비어 있다면 더 이상 탐색할 수 있는 길이
                // 존재하지 않는 것이다.
                aStarClosePath.Add(minCostNode);
                aStarOpenPath.Remove(minCostNode);
                if (aStarOpenPath.IsValid() == false)
                {
                    GFunc.LogWarning("[Warning] There are no more tiles to explore.");
                    isNowayToGo = true;
                }       // if: 목적지에 도착하지 못했는데, 더 이상 탐색할 수 있는 길이 없는 경우

            }       // else: 선택한 노드가 목적지에 도착하지 못한 경우

            loopIdx++;
            yield return new WaitForSeconds(delay_);

            // 다음 루프 전에 지나간 탐색에 사용한 색을 정리한다.
            foreach (AstarNode openNode in aStarOpenPath)
            {
                openNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.DEFAULT);
            }
            foreach (AstarNode closeNode in aStarClosePath)
            {
                closeNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.INACTIVE);
            }
        }       // loop: A star 알고리즘으로 길을 찾는 메인 루프
    }       // DelayFindPath_Astar()

    //! 탐색 알고리즘을 딜레이 없이 사용한다.
    private void DoFindPath_Astar(GameObject sourceObj_, GameObject destObj_)
    {
        // A star 알고리즘을 사용하기 위해서 패스 리스트를 초기화한다.
        aStarOpenPath = new List<AstarNode>();
        aStarClosePath = new List<AstarNode>();
        aStarResultPath = new List<AstarNode>();

        TerrainControler targetTerrain = default;

        // 출발지의 인덱스를 구해서, 출발지 노드를 찾아온다.
        int sourceIdx1D = GetTileIdx1DFromObjName(sourceObj_);
        targetTerrain = mapBoard.GetTerrain(sourceIdx1D);
        // 찾아온 출발지 노드를 Open 리스트에 추가한다.
        AstarNode targetNode = new AstarNode(targetTerrain, destObj_);
        Add_AstarOpenList(targetNode, null, destObj_);

        int loopIdx = 0;
        bool isFoundDestination = false;
        bool isNowayToGo = false;

        //// DEBUG:
        //while (loopIdx < 4)

        while (isFoundDestination == false && isNowayToGo == false)
        {
            // Open 리스트를 순회해서 가장 코스트가 낮은 노드를 선택한다.
            AstarNode minCostNode = GetMinCostNodeFromList(aStarOpenPath);

            // 선택한 노드가 목적지에 도달했는지 확인한다.
            bool isArriveDest = mapBoard.GetDistance2D(
                minCostNode.Terrain.gameObject, destObj_).
                Equals(Vector2Int.zero);

            if (isArriveDest)
            {
                // { 목적지에 도착 했다면 aStarResultPath 리스트를 설정한다.
                AstarNode resultNode = minCostNode;
                bool isSet_aStarResultPathOk = false;
                while (isSet_aStarResultPathOk == false)
                {
                    if (resultNode.AstarPrevNode == default ||
                        resultNode.AstarPrevNode == null)
                    {
                        isSet_aStarResultPathOk = true;
                        break;
                    }
                    else { /* Do nothing */ }

                    //// DEBUG:
                    //// 목적지까지의 aStarResultPath 결과를 초록색으로 표시한다.
                    //resultNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);

                    aStarResultPath.Add(resultNode);
                    resultNode = resultNode.AstarPrevNode;
                }       // loop: 이전 노드를 찾지 못할 때까지 순회하는 루프
                // } 목적지에 도착 했다면 aStarResultPath 리스트를 설정한다.

                // Open list 와 Close list 를 정리한다.
                aStarOpenPath.Clear();
                aStarClosePath.Clear();
                isFoundDestination = true;
                break;
            }       // if: 선택한 노드가 목적지에 도착한 경우
            else
            {
                // { 도착하지 않았다면 현재 타일을 기준으로 4 방향 노드를 찾아온다.
                List<int> nextSearchIdx1Ds = mapBoard.
                    GetTileIdx2D_Around4ways(minCostNode.Terrain.TileIdx2D);

                // 찾아온 노드 중에서 이동 가능한 노드는 Open list 에 추가한다.
                AstarNode nextNode = default;
                foreach (var nextIdx1D in nextSearchIdx1Ds)
                {
                    nextNode = new AstarNode(
                        mapBoard.GetTerrain(nextIdx1D), destObj_);

                    if (nextNode.Terrain.IsPassable == false) { continue; }

                    Add_AstarOpenList(nextNode, minCostNode, destObj_);
                }       // loop: 이동 가능한 노드를 Open list 에 추가하는 루프

                minCostNode.ShowCost_Astar();

                // } 도착하지 않았다면 현재 타일을 기준으로 4 방향 노드를 찾아온다.

                // 탐색이 끝난 노드는 Close list 에 추가하고, Open list 에서 제거한다.
                // 이 때, Open list 가 비어 있다면 더 이상 탐색할 수 있는 길이
                // 존재하지 않는 것이다.
                aStarClosePath.Add(minCostNode);
                aStarOpenPath.Remove(minCostNode);
                if (aStarOpenPath.IsValid() == false)
                {
                    GFunc.LogWarning("[Warning] There are no more tiles to explore. A star searches.");
                    isNowayToGo = true;
                }       // if: 목적지에 도착하지 못했는데, 더 이상 탐색할 수 있는 길이 없는 경우

            }       // else: 선택한 노드가 목적지에 도착하지 못한 경우

            loopIdx++;
        }       // loop: A star 알고리즘으로 길을 찾는 메인 루프
    }       // DelayFindPath_Astar()

    //! 리스트에서 가장 비용이 낮은 노드를 리턴한다.
    private Node GetMinCostNodeFromList<Node>(List<Node> searchList_) where Node : AstarNode
    {
        Node minCostNode = default;

        foreach (var searchNode_ in searchList_)
        {
            if (minCostNode == default)
            {
                minCostNode = searchNode_;
            }       // if: 가장 작은 코스트의 노드가 비어 있는 경우
            else
            {
                // searchNode_ 가 더 작은 코스트를 가지는 경우
                // minCostNode 를 업데이트 한다.
                if (searchNode_.AstarF < minCostNode.AstarF)
                {
                    minCostNode = searchNode_;
                }
                else { continue; }
            }       // else: 가장 작은 코스트의 노드가 캐싱되어 있는 경우
        }       // loop: 가장 코스트가 낮은 노드를 찾는 루프

        return minCostNode;
    }       // GetMinCostNodeFromList()

    //! 비용을 설정한 노드를 Open 리스트에 추가한다.
    private void Add_AstarOpenList(
        AstarNode targetTerrain_, AstarNode prevNode = default, GameObject destObj_ = default)
    {
        AddNode_OpenList(ref aStarOpenPath, ref aStarClosePath, 
            targetTerrain_, prevNode, destObj_);
    }       // Add_AstarOpenList()

    //! 비용을 설정한 노드를 Open 리스트에 추가한다.
    private void AddNode_OpenList<Node>(
        ref List<Node> openlist_, ref List<Node> closelist_,
        Node targetTerrain_, Node prevNode = default, GameObject destObj_ = default) where Node : AstarNode
    {
        // Open 리스트에 추가하기 전에 알고리즘 비용을 설정한다.
        if(destObj_.IsValid() == false) { destObj_ = destinationObj; }
        Update_AstarCostToTerrain(targetTerrain_, prevNode, destObj_);

        Node closeNode = closelist_.FindNode(targetTerrain_);
        if (closeNode != default && closeNode != null)
        {
            // 이미 탐색이 끝난 좌표의 노드가 존재하는 경우에는 
            // Open list 에 추가하지 않는다.
            /* Do nothing */
        }       // if: Close list 에 이미 탐색이 끝난 좌표의 노드가 존재하는 경우
        else
        {
            Node openedNode = openlist_.FindNode(targetTerrain_);
            if (openedNode != default && openedNode != null)
            {
                // 타겟 노드의 코스트가 더 작은 경우에는 Open list 에서 노드를 교체한다.
                // 타겟 노드의 코스트가 더 큰 경우에는 Open list 에 추가하지 않는다.
                if (targetTerrain_.AstarF < openedNode.AstarF)
                {
                    openlist_.Remove(openedNode);
                    openlist_.Add(targetTerrain_);
                }
                else { /* Do nothing */ }
            }       // if: Open list 에 현재 추가할 노드와 같은 좌표의 노드가 존재하는 경우
            else
            {
                openlist_.Add(targetTerrain_);
            }       // else: Open list 에 현재 추가할 노드와 같은 좌표의 노드가 없는 경우
        }       // else: 아직 탐색이 끝나지 않은 노드인 경우
    }       // Add_AstarOpenList()

    //! Target 지형 정보와 Destination 지형 정보로 Distance 와 Heuristic 을 설정하는 함수
    private void Update_AstarCostToTerrain(
        AstarNode targetNode, AstarNode prevNode, GameObject destObj_)
    {
        // { Target 지형에서 Destination 까지의 2D 타일 거리를 계산하는 로직
        Vector2Int distance2D = mapBoard.GetDistance2D(
            targetNode.Terrain.gameObject, destObj_);
        int totalDistance2D = distance2D.x + distance2D.y;

        // Heuristic 은 직선거리로 고정한다.
        Vector2 localDistance = destObj_.transform.localPosition -
            targetNode.Terrain.transform.localPosition;
        float heuristic = Mathf.Abs(localDistance.magnitude);
        // } Target 지형에서 Destination 까지의 2D 타일 거리를 계산하는 로직

        // { 이전 노드가 존재하는 경우 이전 노드의 코스트를 추가해서 연산한다.
        if (prevNode == default || prevNode == null) { /* Do nothing */ }
        else
        {
            totalDistance2D = Mathf.RoundToInt(prevNode.AstarG + 1.0f);
        }
        targetNode.UpdateCost_Astar(
            totalDistance2D, heuristic, prevNode);
        // } 이전 노드가 존재하는 경우 이전 노드의 코스트를 추가해서 연산한다.
    }       // Update_AstarCostToTerrain()

    #endregion      // A star 알고리즘

    #region Jps 알고리즘
    //! 출발지와 목적지 정보로 길을 찾는 함수
    public void FindPath_JPS()
    {
        // Jps 알고리즘을 사용하기 위해서 패스 리스트를 초기화한다.
        jpsOpenPath = new List<JpsNode>();
        jpsClosePath = new List<JpsNode>();
        jpsResultPath = new List<JpsNode>();

        TerrainControler targetTerrain = default;

        // 출발지의 인덱스를 구해서, 출발지 노드를 찾아온다.
        int sourceIdx1D = GetTileIdx1DFromObjName(sourceObj);
        targetTerrain = mapBoard.GetTerrain(sourceIdx1D);
        // 찾아온 출발지 노드를 Open 리스트에 추가한다.
        JpsNode targetNode = new JpsNode(targetTerrain, destinationObj, false);
        Add_JpsOpenList(targetNode);


        int loopIdx = 0;
        bool isFoundDestination = false;
        bool isNowayToGo = false;
        /* 
         * 1. Open list 에서 f 값이 가장 작은 노드 n 을 선택
         * 2. n 이 목적지인지 체크
         * 3. n 이 Jump point 아닌 경우 점프 포인트 서치
         * 4. 점프 포인트 찾았는지 아닌지에 따라서 로직이 갈린다.
         */
        JpsNode minCostNode = GetMinCostNodeFromList(jpsOpenPath);

        // 선택한 노드가 목적지에 도달했는지 확인한다.
        bool isArriveDest = mapBoard.GetDistance2D(
            minCostNode.Terrain.gameObject, destinationObj).
            Equals(Vector2Int.zero);
        if(isArriveDest)
        {
            isFoundDestination = true;
            GFunc.Log("Destination found. End jps search. {0}", isFoundDestination);
        }       // if: 목적지에 도착한 경우
        else
        {
            // 목적지에 도착하지 못한 경우 다음에 탐색할 노드를 찾아서 Open list 에 추가한다.
            TerrainControler jumpPoint = Find_JumpPoint(sourceIdx1D);

            if (jumpPoint.IsValid())
            {
                /*
                 * 1. Node 에서 jumpPoint 까지 최단거리를 A star 알고리즘으로 연산한다.
                 * 2. A star 알고리즘의 Result path 를 가져와서 jumpPoint 의 f 값을 얻는다.
                 * 3. Open list 에 추가한다.
                 */

                //// DEBUG:
                //GFunc.Log("Jump point found. MinCost idx2D: {0} / JumpPoint idx2D: {1}",
                //    minCostNode.Terrain.TileIdx2D, jumpPoint.TileIdx2D);

                // minCostNode 에서 jumpPoint 까지의 최단거리를 A star 알고리즘으로 연산한다.
                // jumpPoint 의 F 값을 구한다.
                DoFindPath_Astar(minCostNode.Terrain.gameObject, jumpPoint.gameObject);

                // { jumpPoint 를 Open list 에 추가한다.
                JpsNode nextNode = new JpsNode(jumpPoint, destinationObj, true);

                // Result 를 역순으로 탐색하면서 JpsNode 로 변환한다.
                JpsNode prevNode = minCostNode;
                JpsNode resultNode = default;
                for(int i = aStarResultPath.Count - 1; -1 < i; i--)
                {
                    if (i.Equals(0))
                    {
                        resultNode = new JpsNode(aStarResultPath[i].Terrain, destinationObj, true);
                    }   // if: jump point 인 경우
                    else
                    {
                        resultNode = new JpsNode(aStarResultPath[i].Terrain, destinationObj, false);
                    }   // else: jump point 로 향하는 다른 노드인 경우

                    //// DEBUG:
                    //GFunc.Log("Result node {0}, F: {1}", 
                    //    resultNode.Terrain.TileIdx1D, resultNode.AstarF);

                    // F 값 연산을 위해서 Cost 를 Update 한다.
                    Update_AstarCostToTerrain(resultNode, prevNode, destinationObj);

                    if(i.Equals(0))
                    {
                        // Jump point 인 경우 다음 Open list 에 추가한다.
                        Add_JpsOpenList(resultNode, prevNode);
                    }   // if: jump point 인 경우
                    else
                    {
                        // 다른 노드는 모두 Close list 에 버린다.
                        jpsClosePath.Add(resultNode);

                        // DEBUG:
                        resultNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.INACTIVE);
                    }   // else: jump point 로 향하는 다른 노드인 경우

                    prevNode = resultNode;
                    resultNode = default;
                }   // loop: aStarResultPath 를 역순으로 순회하는 루프

                // } jumpPoint 를 Open list 에 추가한다.

                jumpPoint.SetTileActiveColor(RDefine.TileStatusColor.SELECTED);
            }       // if: Jump point 가 존재하는 경우
            else
            {
                /*
                 * 1. minCostNode 의 이웃 노드를 모두 Collection 에 캐싱
                 * 2. Collection 에 캐싱한 노드를 전부 Open list 에 추가
                 */

                // { Jump point 가 존재하지 않는 경우 현재 타일을 기준으로 4 방향 노드를 찾아온다.
                List<int> nextSearchIdx1Ds = mapBoard.
                    GetTileIdx2D_Around4ways(minCostNode.Terrain.TileIdx2D);

                // 찾아온 노드 중에서 이동 가능한 노드는 Open list 에 추가한다.
                JpsNode nextNode = default;
                foreach (var nextIdx1D in nextSearchIdx1Ds)
                {
                    nextNode = new JpsNode(
                        mapBoard.GetTerrain(nextIdx1D), destinationObj, false);

                    if (nextNode.Terrain.IsPassable == false) { continue; }

                    Add_JpsOpenList(nextNode, minCostNode);
                    nextNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);
                }       // loop: 이동 가능한 노드를 Open list 에 추가하는 루프

                minCostNode.ShowCost_Astar();
                minCostNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SELECTED);

                // } Jump point 가 존재하지 않는 경우 현재 타일을 기준으로 4 방향 노드를 찾아온다.

                GFunc.Log("Jump point not found .. ");
            }       // else: Jump poin 가 존재하지 않는 경우

            // 탐색이 끝난 노드는 Close list 에 추가하고, Open list 에서 제거한다.
            // 이 때, Open list 가 비어 있다면 더 이상 탐색할 수 있는 길이
            // 존재하지 않는 것이다.
            jpsClosePath.Add(minCostNode);
            jpsOpenPath.Remove(minCostNode);
            if (jpsOpenPath.IsValid() == false)
            {
                isNowayToGo = true;

                GFunc.LogWarning("[Warning] There are no more tiles to explore. {0}", 
                    isNowayToGo);
            }       // if: 목적지에 도착하지 못했는데, 더 이상 탐색할 수 있는 길이 없는 경우

        }       // else: 목적지에 도착하지 못한 경우
        
    }       // FindPath_JPS()

    //! Target 인덱스로부터 Jump point 를 탐색해서 리턴한다.
    private TerrainControler Find_JumpPoint(int targetIdx1D)
    {
        TerrainControler jumpPoint = default;

        // { 동, 서, 남, 북 4 방향의 인덱스 중, 이동 가능한 타일의 인덱스를 모두 가져온다.
        Vector2Int targetIdx2D = mapBoard.GetTileIdx2D(targetIdx1D);

        // targetIdx 를 기준으로 아래와 위로 나눈다.
        List<TerrainControler> terrainsColum = mapBoard.GetTerrains_Colum(targetIdx2D.x);
        List<TerrainControler>[] terrainsSN = mapBoard.SplitTerrains(terrainsColum, targetIdx2D);
        // 아래에서 위로 탐색이 정방향이기 때문에, 아래는 역방향으로 탐색한다.
        terrainsSN[0].Reverse();
        mapBoard.RemoveTerrains_NonPassableTileAfter(ref terrainsSN[0]);
        jumpPoint = mapBoard.GetJumpPoint(terrainsSN[0], GData.GridDirection.SOUTH);
        if (jumpPoint.IsValid()) { return jumpPoint; }

        mapBoard.RemoveTerrains_NonPassableTileAfter(ref terrainsSN[1]);
        jumpPoint = mapBoard.GetJumpPoint(terrainsSN[1], GData.GridDirection.NORTH);
        if (jumpPoint.IsValid()) { return jumpPoint; }

        // targetIdx 를 기준으로 왼쪽과 오른쪽으로 나눈다.
        List<TerrainControler> terrainsRow = mapBoard.GetTerrains_Row(targetIdx2D.y);
        List<TerrainControler>[] terrainsWE = mapBoard.SplitTerrains(terrainsRow, targetIdx2D);
        // 왼쪽에서 오른쪽으로 탐색이 정방향이기 때문에, 왼쪽은 역방향으로 탐색한다.
        terrainsWE[0].Reverse();
        mapBoard.RemoveTerrains_NonPassableTileAfter(ref terrainsWE[0]);
        jumpPoint = mapBoard.GetJumpPoint(terrainsWE[0], GData.GridDirection.WEST);
        if (jumpPoint.IsValid()) { return jumpPoint; }

        mapBoard.RemoveTerrains_NonPassableTileAfter(ref terrainsWE[1]);
        jumpPoint = mapBoard.GetJumpPoint(terrainsWE[1], GData.GridDirection.EAST);
        if (jumpPoint.IsValid()) { return jumpPoint; }

        // } 동, 서, 남, 북 4 방향의 인덱스 중, 이동 가능한 타일의 인덱스를 모두 가져온다.
        return jumpPoint;
    }       // Find_JumpPoint()

    //! 비용을 설정한 노드를 Open 리스트에 추가한다.
    private void Add_JpsOpenList(
        JpsNode targetTerrain_, JpsNode prevNode = default)
    {
        AddNode_OpenList(ref jpsOpenPath, ref jpsClosePath, targetTerrain_, prevNode);
    }       // Add_AstarOpenList()
    #endregion      // Jps 알고리즘


    //! 타일 오브젝트의 이름으로 인덱스를 연산해서 리턴한다.
    private int GetTileIdx1DFromObjName(GameObject tileObj_)
    {
        string[] sourceObjNameParts = tileObj_.name.Split('_');
        int sourceIdx1D = -1;
        int.TryParse(
            sourceObjNameParts[sourceObjNameParts.Length - 1], out sourceIdx1D);

        return sourceIdx1D;
    }       // GetTileIdx1DFromObjName()
}

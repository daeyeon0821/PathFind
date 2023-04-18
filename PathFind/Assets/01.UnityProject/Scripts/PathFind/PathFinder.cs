using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : GSingleton<PathFinder>
{
    #region ���� Ž���� ���� ����
    public GameObject sourceObj = default;
    public GameObject destinationObj = default;
    public MapBoard mapBoard = default;
    #endregion      // ���� Ž���� ���� ����

    #region A star �˰������� �ִܰŸ��� ã�� ���� ����
    private List<AstarNode> aStarResultPath = default;
    private List<AstarNode> aStarOpenPath = default;
    private List<AstarNode> aStarClosePath = default;
    #endregion      // A star �˰������� �ִܰŸ��� ã�� ���� ����

    #region Jps �˰������� �ִܰŸ��� ã�� ���� ����
    private List<JpsNode> jpsResultPath = default;
    private List<JpsNode> jpsOpenPath = default;
    private List<JpsNode> jpsClosePath = default;
    #endregion      // Jps �˰������� �ִܰŸ��� ã�� ���� ����

    private Coroutine currentRunningAlgorithm = default;


    #region A star �˰���
    
    //! ������� ������ ������ ���� ã�� �Լ�
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

    //! A star �˰����� ���߰� ��带 �����ϴ� �Լ�
    public void Clear_Astar()
    {
        // �������� ��ƾ�� ���� ��� Clear �� ���� ����.
        if (currentRunningAlgorithm.IsValid() == false) { return; }
        
        // ��ƾ�� ���߰� Open list �� Close list �� �����Ѵ�.
        StopCoroutine(currentRunningAlgorithm);
        if (aStarOpenPath.IsValid())
        {
            foreach (AstarNode node_ in aStarOpenPath)
            {
                node_.Terrain.SetTileActiveColor(RDefine.TileStatusColor.DEFAULT);
            }
            aStarOpenPath.Clear();
        }       // if: Open list �� �����Ѵ�.

        if(aStarClosePath.IsValid())
        {
            foreach(AstarNode node_ in aStarClosePath)
            {
                node_.Terrain.SetTileActiveColor(RDefine.TileStatusColor.DEFAULT);
            }
            aStarClosePath.Clear();
        }       // if: Close list �� �����Ѵ�.
    }       // Stop_Astar()

    //! Ž�� �˰��� �����̸� �Ǵ�.
    private IEnumerator DelayFindPath_Astar(float delay_, GameObject sourceObj_, GameObject destObj_)
    {
        // A star �˰����� ����ϱ� ���ؼ� �н� ����Ʈ�� �ʱ�ȭ�Ѵ�.
        aStarOpenPath = new List<AstarNode>();
        aStarClosePath = new List<AstarNode>();
        aStarResultPath = new List<AstarNode>();

        TerrainControler targetTerrain = default;

        // ������� �ε����� ���ؼ�, ����� ��带 ã�ƿ´�.
        int sourceIdx1D = GetTileIdx1DFromObjName(sourceObj_);
        targetTerrain = mapBoard.GetTerrain(sourceIdx1D);
        // ã�ƿ� ����� ��带 Open ����Ʈ�� �߰��Ѵ�.
        AstarNode targetNode = new AstarNode(targetTerrain, destObj_);
        Add_AstarOpenList(targetNode);

        int loopIdx = 0;
        bool isFoundDestination = false;
        bool isNowayToGo = false;

        //// DEBUG:
        //while (loopIdx < 10)

        while (isFoundDestination == false && isNowayToGo == false)
        {
            // Open ����Ʈ�� ��ȸ�ؼ� ���� �ڽ�Ʈ�� ���� ��带 �����Ѵ�.
            AstarNode minCostNode = GetMinCostNodeFromList(aStarOpenPath);

            // ������ ��尡 �������� �����ߴ��� Ȯ���Ѵ�.
            bool isArriveDest = mapBoard.GetDistance2D(
                minCostNode.Terrain.gameObject, destObj_).
                Equals(Vector2Int.zero);

            if (isArriveDest)
            {
                // { �������� ���� �ߴٸ� aStarResultPath ����Ʈ�� �����Ѵ�.
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

                    // ������������ aStarResultPath ����� �ʷϻ����� ǥ���Ѵ�.
                    resultNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SELECTED);
                    aStarResultPath.Add(resultNode);
                    resultNode = resultNode.AstarPrevNode;
                }       // loop: ���� ��带 ã�� ���� ������ ��ȸ�ϴ� ����
                // } �������� ���� �ߴٸ� aStarResultPath ����Ʈ�� �����Ѵ�.

                // Open list �� Close list �� �����Ѵ�.
                aStarOpenPath.Clear();
                aStarClosePath.Clear();
                isFoundDestination = true;
                break;
            }       // if: ������ ��尡 �������� ������ ���
            else
            {
                // { �������� �ʾҴٸ� ���� Ÿ���� �������� 4 ���� ��带 ã�ƿ´�.
                List<int> nextSearchIdx1Ds = mapBoard.
                    GetTileIdx2D_Around4ways(minCostNode.Terrain.TileIdx2D);

                // ã�ƿ� ��� �߿��� �̵� ������ ���� Open list �� �߰��Ѵ�.
                AstarNode nextNode = default;
                foreach (var nextIdx1D in nextSearchIdx1Ds)
                {
                    nextNode = new AstarNode(
                        mapBoard.GetTerrain(nextIdx1D), destObj_);

                    if (nextNode.Terrain.IsPassable == false) { continue; }

                    Add_AstarOpenList(nextNode, minCostNode);
                    nextNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);
                }       // loop: �̵� ������ ��带 Open list �� �߰��ϴ� ����

                minCostNode.ShowCost_Astar();
                minCostNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SELECTED);

                // } �������� �ʾҴٸ� ���� Ÿ���� �������� 4 ���� ��带 ã�ƿ´�.

                // Ž���� ���� ���� Close list �� �߰��ϰ�, Open list ���� �����Ѵ�.
                // �� ��, Open list �� ��� �ִٸ� �� �̻� Ž���� �� �ִ� ����
                // �������� �ʴ� ���̴�.
                aStarClosePath.Add(minCostNode);
                aStarOpenPath.Remove(minCostNode);
                if (aStarOpenPath.IsValid() == false)
                {
                    GFunc.LogWarning("[Warning] There are no more tiles to explore.");
                    isNowayToGo = true;
                }       // if: �������� �������� ���ߴµ�, �� �̻� Ž���� �� �ִ� ���� ���� ���

            }       // else: ������ ��尡 �������� �������� ���� ���

            loopIdx++;
            yield return new WaitForSeconds(delay_);

            // ���� ���� ���� ������ Ž���� ����� ���� �����Ѵ�.
            foreach (AstarNode openNode in aStarOpenPath)
            {
                openNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.DEFAULT);
            }
            foreach (AstarNode closeNode in aStarClosePath)
            {
                closeNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.INACTIVE);
            }
        }       // loop: A star �˰������� ���� ã�� ���� ����
    }       // DelayFindPath_Astar()

    //! Ž�� �˰����� ������ ���� ����Ѵ�.
    private void DoFindPath_Astar(GameObject sourceObj_, GameObject destObj_)
    {
        // A star �˰����� ����ϱ� ���ؼ� �н� ����Ʈ�� �ʱ�ȭ�Ѵ�.
        aStarOpenPath = new List<AstarNode>();
        aStarClosePath = new List<AstarNode>();
        aStarResultPath = new List<AstarNode>();

        TerrainControler targetTerrain = default;

        // ������� �ε����� ���ؼ�, ����� ��带 ã�ƿ´�.
        int sourceIdx1D = GetTileIdx1DFromObjName(sourceObj_);
        targetTerrain = mapBoard.GetTerrain(sourceIdx1D);
        // ã�ƿ� ����� ��带 Open ����Ʈ�� �߰��Ѵ�.
        AstarNode targetNode = new AstarNode(targetTerrain, destObj_);
        Add_AstarOpenList(targetNode, null, destObj_);

        int loopIdx = 0;
        bool isFoundDestination = false;
        bool isNowayToGo = false;

        //// DEBUG:
        //while (loopIdx < 4)

        while (isFoundDestination == false && isNowayToGo == false)
        {
            // Open ����Ʈ�� ��ȸ�ؼ� ���� �ڽ�Ʈ�� ���� ��带 �����Ѵ�.
            AstarNode minCostNode = GetMinCostNodeFromList(aStarOpenPath);

            // ������ ��尡 �������� �����ߴ��� Ȯ���Ѵ�.
            bool isArriveDest = mapBoard.GetDistance2D(
                minCostNode.Terrain.gameObject, destObj_).
                Equals(Vector2Int.zero);

            if (isArriveDest)
            {
                // { �������� ���� �ߴٸ� aStarResultPath ����Ʈ�� �����Ѵ�.
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
                    //// ������������ aStarResultPath ����� �ʷϻ����� ǥ���Ѵ�.
                    //resultNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);

                    aStarResultPath.Add(resultNode);
                    resultNode = resultNode.AstarPrevNode;
                }       // loop: ���� ��带 ã�� ���� ������ ��ȸ�ϴ� ����
                // } �������� ���� �ߴٸ� aStarResultPath ����Ʈ�� �����Ѵ�.

                // Open list �� Close list �� �����Ѵ�.
                aStarOpenPath.Clear();
                aStarClosePath.Clear();
                isFoundDestination = true;
                break;
            }       // if: ������ ��尡 �������� ������ ���
            else
            {
                // { �������� �ʾҴٸ� ���� Ÿ���� �������� 4 ���� ��带 ã�ƿ´�.
                List<int> nextSearchIdx1Ds = mapBoard.
                    GetTileIdx2D_Around4ways(minCostNode.Terrain.TileIdx2D);

                // ã�ƿ� ��� �߿��� �̵� ������ ���� Open list �� �߰��Ѵ�.
                AstarNode nextNode = default;
                foreach (var nextIdx1D in nextSearchIdx1Ds)
                {
                    nextNode = new AstarNode(
                        mapBoard.GetTerrain(nextIdx1D), destObj_);

                    if (nextNode.Terrain.IsPassable == false) { continue; }

                    Add_AstarOpenList(nextNode, minCostNode, destObj_);
                }       // loop: �̵� ������ ��带 Open list �� �߰��ϴ� ����

                minCostNode.ShowCost_Astar();

                // } �������� �ʾҴٸ� ���� Ÿ���� �������� 4 ���� ��带 ã�ƿ´�.

                // Ž���� ���� ���� Close list �� �߰��ϰ�, Open list ���� �����Ѵ�.
                // �� ��, Open list �� ��� �ִٸ� �� �̻� Ž���� �� �ִ� ����
                // �������� �ʴ� ���̴�.
                aStarClosePath.Add(minCostNode);
                aStarOpenPath.Remove(minCostNode);
                if (aStarOpenPath.IsValid() == false)
                {
                    GFunc.LogWarning("[Warning] There are no more tiles to explore. A star searches.");
                    isNowayToGo = true;
                }       // if: �������� �������� ���ߴµ�, �� �̻� Ž���� �� �ִ� ���� ���� ���

            }       // else: ������ ��尡 �������� �������� ���� ���

            loopIdx++;
        }       // loop: A star �˰������� ���� ã�� ���� ����
    }       // DelayFindPath_Astar()

    //! ����Ʈ���� ���� ����� ���� ��带 �����Ѵ�.
    private Node GetMinCostNodeFromList<Node>(List<Node> searchList_) where Node : AstarNode
    {
        Node minCostNode = default;

        foreach (var searchNode_ in searchList_)
        {
            if (minCostNode == default)
            {
                minCostNode = searchNode_;
            }       // if: ���� ���� �ڽ�Ʈ�� ��尡 ��� �ִ� ���
            else
            {
                // searchNode_ �� �� ���� �ڽ�Ʈ�� ������ ���
                // minCostNode �� ������Ʈ �Ѵ�.
                if (searchNode_.AstarF < minCostNode.AstarF)
                {
                    minCostNode = searchNode_;
                }
                else { continue; }
            }       // else: ���� ���� �ڽ�Ʈ�� ��尡 ĳ�̵Ǿ� �ִ� ���
        }       // loop: ���� �ڽ�Ʈ�� ���� ��带 ã�� ����

        return minCostNode;
    }       // GetMinCostNodeFromList()

    //! ����� ������ ��带 Open ����Ʈ�� �߰��Ѵ�.
    private void Add_AstarOpenList(
        AstarNode targetTerrain_, AstarNode prevNode = default, GameObject destObj_ = default)
    {
        AddNode_OpenList(ref aStarOpenPath, ref aStarClosePath, 
            targetTerrain_, prevNode, destObj_);
    }       // Add_AstarOpenList()

    //! ����� ������ ��带 Open ����Ʈ�� �߰��Ѵ�.
    private void AddNode_OpenList<Node>(
        ref List<Node> openlist_, ref List<Node> closelist_,
        Node targetTerrain_, Node prevNode = default, GameObject destObj_ = default) where Node : AstarNode
    {
        // Open ����Ʈ�� �߰��ϱ� ���� �˰��� ����� �����Ѵ�.
        if(destObj_.IsValid() == false) { destObj_ = destinationObj; }
        Update_AstarCostToTerrain(targetTerrain_, prevNode, destObj_);

        Node closeNode = closelist_.FindNode(targetTerrain_);
        if (closeNode != default && closeNode != null)
        {
            // �̹� Ž���� ���� ��ǥ�� ��尡 �����ϴ� ��쿡�� 
            // Open list �� �߰����� �ʴ´�.
            /* Do nothing */
        }       // if: Close list �� �̹� Ž���� ���� ��ǥ�� ��尡 �����ϴ� ���
        else
        {
            Node openedNode = openlist_.FindNode(targetTerrain_);
            if (openedNode != default && openedNode != null)
            {
                // Ÿ�� ����� �ڽ�Ʈ�� �� ���� ��쿡�� Open list ���� ��带 ��ü�Ѵ�.
                // Ÿ�� ����� �ڽ�Ʈ�� �� ū ��쿡�� Open list �� �߰����� �ʴ´�.
                if (targetTerrain_.AstarF < openedNode.AstarF)
                {
                    openlist_.Remove(openedNode);
                    openlist_.Add(targetTerrain_);
                }
                else { /* Do nothing */ }
            }       // if: Open list �� ���� �߰��� ���� ���� ��ǥ�� ��尡 �����ϴ� ���
            else
            {
                openlist_.Add(targetTerrain_);
            }       // else: Open list �� ���� �߰��� ���� ���� ��ǥ�� ��尡 ���� ���
        }       // else: ���� Ž���� ������ ���� ����� ���
    }       // Add_AstarOpenList()

    //! Target ���� ������ Destination ���� ������ Distance �� Heuristic �� �����ϴ� �Լ�
    private void Update_AstarCostToTerrain(
        AstarNode targetNode, AstarNode prevNode, GameObject destObj_)
    {
        // { Target �������� Destination ������ 2D Ÿ�� �Ÿ��� ����ϴ� ����
        Vector2Int distance2D = mapBoard.GetDistance2D(
            targetNode.Terrain.gameObject, destObj_);
        int totalDistance2D = distance2D.x + distance2D.y;

        // Heuristic �� �����Ÿ��� �����Ѵ�.
        Vector2 localDistance = destObj_.transform.localPosition -
            targetNode.Terrain.transform.localPosition;
        float heuristic = Mathf.Abs(localDistance.magnitude);
        // } Target �������� Destination ������ 2D Ÿ�� �Ÿ��� ����ϴ� ����

        // { ���� ��尡 �����ϴ� ��� ���� ����� �ڽ�Ʈ�� �߰��ؼ� �����Ѵ�.
        if (prevNode == default || prevNode == null) { /* Do nothing */ }
        else
        {
            totalDistance2D = Mathf.RoundToInt(prevNode.AstarG + 1.0f);
        }
        targetNode.UpdateCost_Astar(
            totalDistance2D, heuristic, prevNode);
        // } ���� ��尡 �����ϴ� ��� ���� ����� �ڽ�Ʈ�� �߰��ؼ� �����Ѵ�.
    }       // Update_AstarCostToTerrain()

    #endregion      // A star �˰���

    #region Jps �˰���
    //! ������� ������ ������ ���� ã�� �Լ�
    public void FindPath_JPS()
    {
        // Jps �˰����� ����ϱ� ���ؼ� �н� ����Ʈ�� �ʱ�ȭ�Ѵ�.
        jpsOpenPath = new List<JpsNode>();
        jpsClosePath = new List<JpsNode>();
        jpsResultPath = new List<JpsNode>();

        TerrainControler targetTerrain = default;

        // ������� �ε����� ���ؼ�, ����� ��带 ã�ƿ´�.
        int sourceIdx1D = GetTileIdx1DFromObjName(sourceObj);
        targetTerrain = mapBoard.GetTerrain(sourceIdx1D);
        // ã�ƿ� ����� ��带 Open ����Ʈ�� �߰��Ѵ�.
        JpsNode targetNode = new JpsNode(targetTerrain, destinationObj, false);
        Add_JpsOpenList(targetNode);


        int loopIdx = 0;
        bool isFoundDestination = false;
        bool isNowayToGo = false;
        /* 
         * 1. Open list ���� f ���� ���� ���� ��� n �� ����
         * 2. n �� ���������� üũ
         * 3. n �� Jump point �ƴ� ��� ���� ����Ʈ ��ġ
         * 4. ���� ����Ʈ ã�Ҵ��� �ƴ����� ���� ������ ������.
         */
        JpsNode minCostNode = GetMinCostNodeFromList(jpsOpenPath);

        // ������ ��尡 �������� �����ߴ��� Ȯ���Ѵ�.
        bool isArriveDest = mapBoard.GetDistance2D(
            minCostNode.Terrain.gameObject, destinationObj).
            Equals(Vector2Int.zero);
        if(isArriveDest)
        {
            isFoundDestination = true;
            GFunc.Log("Destination found. End jps search. {0}", isFoundDestination);
        }       // if: �������� ������ ���
        else
        {
            // �������� �������� ���� ��� ������ Ž���� ��带 ã�Ƽ� Open list �� �߰��Ѵ�.
            TerrainControler jumpPoint = Find_JumpPoint(sourceIdx1D);

            if (jumpPoint.IsValid())
            {
                /*
                 * 1. Node ���� jumpPoint ���� �ִܰŸ��� A star �˰������� �����Ѵ�.
                 * 2. A star �˰����� Result path �� �����ͼ� jumpPoint �� f ���� ��´�.
                 * 3. Open list �� �߰��Ѵ�.
                 */

                //// DEBUG:
                //GFunc.Log("Jump point found. MinCost idx2D: {0} / JumpPoint idx2D: {1}",
                //    minCostNode.Terrain.TileIdx2D, jumpPoint.TileIdx2D);

                // minCostNode ���� jumpPoint ������ �ִܰŸ��� A star �˰������� �����Ѵ�.
                // jumpPoint �� F ���� ���Ѵ�.
                DoFindPath_Astar(minCostNode.Terrain.gameObject, jumpPoint.gameObject);

                // { jumpPoint �� Open list �� �߰��Ѵ�.
                JpsNode nextNode = new JpsNode(jumpPoint, destinationObj, true);

                // Result �� �������� Ž���ϸ鼭 JpsNode �� ��ȯ�Ѵ�.
                JpsNode prevNode = minCostNode;
                JpsNode resultNode = default;
                for(int i = aStarResultPath.Count - 1; -1 < i; i--)
                {
                    if (i.Equals(0))
                    {
                        resultNode = new JpsNode(aStarResultPath[i].Terrain, destinationObj, true);
                    }   // if: jump point �� ���
                    else
                    {
                        resultNode = new JpsNode(aStarResultPath[i].Terrain, destinationObj, false);
                    }   // else: jump point �� ���ϴ� �ٸ� ����� ���

                    //// DEBUG:
                    //GFunc.Log("Result node {0}, F: {1}", 
                    //    resultNode.Terrain.TileIdx1D, resultNode.AstarF);

                    // F �� ������ ���ؼ� Cost �� Update �Ѵ�.
                    Update_AstarCostToTerrain(resultNode, prevNode, destinationObj);

                    if(i.Equals(0))
                    {
                        // Jump point �� ��� ���� Open list �� �߰��Ѵ�.
                        Add_JpsOpenList(resultNode, prevNode);
                    }   // if: jump point �� ���
                    else
                    {
                        // �ٸ� ���� ��� Close list �� ������.
                        jpsClosePath.Add(resultNode);

                        // DEBUG:
                        resultNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.INACTIVE);
                    }   // else: jump point �� ���ϴ� �ٸ� ����� ���

                    prevNode = resultNode;
                    resultNode = default;
                }   // loop: aStarResultPath �� �������� ��ȸ�ϴ� ����

                // } jumpPoint �� Open list �� �߰��Ѵ�.

                jumpPoint.SetTileActiveColor(RDefine.TileStatusColor.SELECTED);
            }       // if: Jump point �� �����ϴ� ���
            else
            {
                /*
                 * 1. minCostNode �� �̿� ��带 ��� Collection �� ĳ��
                 * 2. Collection �� ĳ���� ��带 ���� Open list �� �߰�
                 */

                // { Jump point �� �������� �ʴ� ��� ���� Ÿ���� �������� 4 ���� ��带 ã�ƿ´�.
                List<int> nextSearchIdx1Ds = mapBoard.
                    GetTileIdx2D_Around4ways(minCostNode.Terrain.TileIdx2D);

                // ã�ƿ� ��� �߿��� �̵� ������ ���� Open list �� �߰��Ѵ�.
                JpsNode nextNode = default;
                foreach (var nextIdx1D in nextSearchIdx1Ds)
                {
                    nextNode = new JpsNode(
                        mapBoard.GetTerrain(nextIdx1D), destinationObj, false);

                    if (nextNode.Terrain.IsPassable == false) { continue; }

                    Add_JpsOpenList(nextNode, minCostNode);
                    nextNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SEARCHING);
                }       // loop: �̵� ������ ��带 Open list �� �߰��ϴ� ����

                minCostNode.ShowCost_Astar();
                minCostNode.Terrain.SetTileActiveColor(RDefine.TileStatusColor.SELECTED);

                // } Jump point �� �������� �ʴ� ��� ���� Ÿ���� �������� 4 ���� ��带 ã�ƿ´�.

                GFunc.Log("Jump point not found .. ");
            }       // else: Jump poin �� �������� �ʴ� ���

            // Ž���� ���� ���� Close list �� �߰��ϰ�, Open list ���� �����Ѵ�.
            // �� ��, Open list �� ��� �ִٸ� �� �̻� Ž���� �� �ִ� ����
            // �������� �ʴ� ���̴�.
            jpsClosePath.Add(minCostNode);
            jpsOpenPath.Remove(minCostNode);
            if (jpsOpenPath.IsValid() == false)
            {
                isNowayToGo = true;

                GFunc.LogWarning("[Warning] There are no more tiles to explore. {0}", 
                    isNowayToGo);
            }       // if: �������� �������� ���ߴµ�, �� �̻� Ž���� �� �ִ� ���� ���� ���

        }       // else: �������� �������� ���� ���
        
    }       // FindPath_JPS()

    //! Target �ε����κ��� Jump point �� Ž���ؼ� �����Ѵ�.
    private TerrainControler Find_JumpPoint(int targetIdx1D)
    {
        TerrainControler jumpPoint = default;

        // { ��, ��, ��, �� 4 ������ �ε��� ��, �̵� ������ Ÿ���� �ε����� ��� �����´�.
        Vector2Int targetIdx2D = mapBoard.GetTileIdx2D(targetIdx1D);

        // targetIdx �� �������� �Ʒ��� ���� ������.
        List<TerrainControler> terrainsColum = mapBoard.GetTerrains_Colum(targetIdx2D.x);
        List<TerrainControler>[] terrainsSN = mapBoard.SplitTerrains(terrainsColum, targetIdx2D);
        // �Ʒ����� ���� Ž���� �������̱� ������, �Ʒ��� ���������� Ž���Ѵ�.
        terrainsSN[0].Reverse();
        mapBoard.RemoveTerrains_NonPassableTileAfter(ref terrainsSN[0]);
        jumpPoint = mapBoard.GetJumpPoint(terrainsSN[0], GData.GridDirection.SOUTH);
        if (jumpPoint.IsValid()) { return jumpPoint; }

        mapBoard.RemoveTerrains_NonPassableTileAfter(ref terrainsSN[1]);
        jumpPoint = mapBoard.GetJumpPoint(terrainsSN[1], GData.GridDirection.NORTH);
        if (jumpPoint.IsValid()) { return jumpPoint; }

        // targetIdx �� �������� ���ʰ� ���������� ������.
        List<TerrainControler> terrainsRow = mapBoard.GetTerrains_Row(targetIdx2D.y);
        List<TerrainControler>[] terrainsWE = mapBoard.SplitTerrains(terrainsRow, targetIdx2D);
        // ���ʿ��� ���������� Ž���� �������̱� ������, ������ ���������� Ž���Ѵ�.
        terrainsWE[0].Reverse();
        mapBoard.RemoveTerrains_NonPassableTileAfter(ref terrainsWE[0]);
        jumpPoint = mapBoard.GetJumpPoint(terrainsWE[0], GData.GridDirection.WEST);
        if (jumpPoint.IsValid()) { return jumpPoint; }

        mapBoard.RemoveTerrains_NonPassableTileAfter(ref terrainsWE[1]);
        jumpPoint = mapBoard.GetJumpPoint(terrainsWE[1], GData.GridDirection.EAST);
        if (jumpPoint.IsValid()) { return jumpPoint; }

        // } ��, ��, ��, �� 4 ������ �ε��� ��, �̵� ������ Ÿ���� �ε����� ��� �����´�.
        return jumpPoint;
    }       // Find_JumpPoint()

    //! ����� ������ ��带 Open ����Ʈ�� �߰��Ѵ�.
    private void Add_JpsOpenList(
        JpsNode targetTerrain_, JpsNode prevNode = default)
    {
        AddNode_OpenList(ref jpsOpenPath, ref jpsClosePath, targetTerrain_, prevNode);
    }       // Add_AstarOpenList()
    #endregion      // Jps �˰���


    //! Ÿ�� ������Ʈ�� �̸����� �ε����� �����ؼ� �����Ѵ�.
    private int GetTileIdx1DFromObjName(GameObject tileObj_)
    {
        string[] sourceObjNameParts = tileObj_.name.Split('_');
        int sourceIdx1D = -1;
        int.TryParse(
            sourceObjNameParts[sourceObjNameParts.Length - 1], out sourceIdx1D);

        return sourceIdx1D;
    }       // GetTileIdx1DFromObjName()
}

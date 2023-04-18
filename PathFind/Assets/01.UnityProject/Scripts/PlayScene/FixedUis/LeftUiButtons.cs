using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftUiButtons : MonoBehaviour
{
    //! A star find path 버튼을 누른 경우
    public void OnClickAstarFindBtn()
    {
        PathFinder.Instance.FindPath_Astar();
    }       // OnClickAstarFindBtn()

    //! Jps find path 버튼을 누른 경우
    public void OnClickJpsFindBtn()
    {
        GFunc.Log("Jps find button ok");

        PathFinder.Instance.FindPath_JPS();
    }       // OnClickJpsFindBtn()

    //! Shuffle 버튼을 누른 경우
    public void OnClickShuffleBtn()
    {
        PathFinder.Instance.mapBoard.ResetMapBoard();
    }       // OnClickShuffleBtn()
}

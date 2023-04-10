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

    //! Shuffle 버튼을 누른 경우
    public void OnClickShuffleBtn()
    {
        PathFinder.Instance.mapBoard.ResetMapBoard();
    }       // OnClickShuffleBtn()
}

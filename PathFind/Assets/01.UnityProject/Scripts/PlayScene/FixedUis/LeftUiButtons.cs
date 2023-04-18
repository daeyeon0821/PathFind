using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftUiButtons : MonoBehaviour
{
    //! A star find path ��ư�� ���� ���
    public void OnClickAstarFindBtn()
    {
        PathFinder.Instance.FindPath_Astar();
    }       // OnClickAstarFindBtn()

    //! Jps find path ��ư�� ���� ���
    public void OnClickJpsFindBtn()
    {
        GFunc.Log("Jps find button ok");

        PathFinder.Instance.FindPath_JPS();
    }       // OnClickJpsFindBtn()

    //! Shuffle ��ư�� ���� ���
    public void OnClickShuffleBtn()
    {
        PathFinder.Instance.mapBoard.ResetMapBoard();
    }       // OnClickShuffleBtn()
}

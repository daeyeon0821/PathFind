using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightUiButtons : MonoBehaviour
{
    //! Stop find ��ư�� ���� ���
    public void OnClickStopFindBtn()
    {
        PathFinder.Instance.Clear_Astar();
        PathFinder.Instance.Clear_Jps();
    }       // OnClickStopFindBtn()
}

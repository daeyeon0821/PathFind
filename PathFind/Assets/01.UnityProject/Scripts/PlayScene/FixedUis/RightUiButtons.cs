using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightUiButtons : MonoBehaviour
{
    //! Stop find 버튼을 누른 경우
    public void OnClickStopFindBtn()
    {
        PathFinder.Instance.Clear_Astar();
        PathFinder.Instance.Clear_Jps();
    }       // OnClickStopFindBtn()
}

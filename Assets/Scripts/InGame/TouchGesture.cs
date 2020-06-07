using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***돌려막기 클래스 시즌 2! ^.^ ***/
/*
 스와이프와 그냥 클릭을 구분 가능하게 함
사용할 스크립트에 MonoBehaviour 대신에 TouchGesture를 상속시켜주고
버추얼 함수들을 오버로딩해서 사용하셈
*/

public class TouchGesture : MonoBehaviour
{
    //private const float SwipeDis = 10;//스와이프 허용 범위 수정하고 싶으면 이거 수정하셈
    //private Vector2 StartPos;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            GetClick();//일단 이런 거추장스러운 잡것들 없애고 이것만 남김

        //if (Input.GetMouseButtonDown(0))
        //{
        //    StartPos = Input.mousePosition;
        //}
        //if (Input.GetMouseButtonUp(0))
        //{
        //    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())//UI를 터치했으면 return
        //    {
        //        return;
        //    }

        //    Vector2 p = (Vector2)Input.mousePosition - StartPos;
            
        //    if (Mathf.Abs(p.x) < SwipeDis && Mathf.Abs(p.y) < SwipeDis)
        //        GetClick();
        //    else
        //    {
        //        if (Mathf.Abs(p.x) > Mathf.Abs(p.y))
        //            GetSwipe(StartPos, new Vector2(Mathf.Sign(p.x), 0));
        //        else if (Mathf.Abs(p.x) < Mathf.Abs(p.y))
        //            GetSwipe(StartPos, new Vector2(0, Mathf.Sign(p.y)));
        //    }
        //}
    }

    public virtual void GetClick() { }
    public virtual void GetSwipe(Vector2 StartPos, Vector2 dir) { }
}

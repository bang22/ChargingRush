using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour
{

    public float shakeAmount;
    public float rotateSpeed;
    
    public Camera cam;

    public Transform target;
    public float limit;
    public float speed;
    public float plusSpeed;

    public float curspeed;

    private IEnumerator shake;
    private IEnumerator rotate;

    //public void Update()//폴이 계속 보이도록
    //{
    //    float camsize = Camera.main.orthographicSize * Screen.height / Screen.width;
    //    if (target.position.y < transform.position.y - camsize + limit)
    //    {
    //        Vector2 pos = transform.position;
    //        pos.y -= curspeed * Time.deltaTime;
    //        transform.position = pos;
    //        curspeed += plusSpeed * Time.deltaTime;
    //    }
    //    else curspeed = speed;
    //}

    public void CamShake(float limitTime)//흔들
    {
        if (shake != null)
            StopCoroutine(shake);
        shake = CamShakeSeq(limitTime);
        StartCoroutine(shake);
    }

    public void CamRotate(float angle)//빙글
    {
        if (rotate != null)
            StopCoroutine(rotate);
        rotate = CamRotateSeq(angle);
        StartCoroutine(rotate);
    }

    private IEnumerator CamShakeSeq(float limitTime)//카메라 쉐이크
    {
        Vector3 origonPos = cam.transform.localPosition;//원래 위치
        float time = 0;
        while (true)
        {
            //개떨림
            cam.transform.localPosition = origonPos + new Vector3(Random.value-0.5f, Random.value-0.5f) * shakeAmount;
            yield return 0;
            time += Time.deltaTime;
            if (time >= limitTime) break;
        }
        cam.transform.localPosition = origonPos;
    }

    private IEnumerator CamRotateSeq(float targetAngle)//카메라 돌리기
    {
        float angle = cam.transform.localRotation.eulerAngles.z;
        float progress = 0;

        while (true)
        {
            yield return 0;
            progress += Time.deltaTime * rotateSpeed;
            cam.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(angle, targetAngle, progress));

            if (progress >= 1) break;
        }
    }

}

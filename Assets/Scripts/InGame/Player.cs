using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour//TouchGesture
{

    private bool isMove = false;//움직이는가?
    private bool isClear = false;//끝?

    //바닥 충돌체크
    [SerializeField]
    private Transform foot;
    //앞 충돌체크
    [SerializeField]
    private Transform front;

    //마스크
    [SerializeField]
    private LayerMask mask;

    private bool isJump = false;
    private bool isJumping = false;
    [SerializeField]
    private Collider2D coll;

    public Rigidbody2D rig;

    [SerializeField]
    private float StartJumpPower = 20;
    [SerializeField]
    private float plusJumpPower = 2;
    private float jumpTime;
    public Animator anim;

    [SerializeField]
    private GameObject fire;

    //*액숀*//

    public void PlayerInit()
    {
        this.enabled = true;
        coll.enabled = true;

        transform.position = new Vector2(-25, -14);
        rig.velocity = Vector2.zero;

        anim.ResetTrigger("Pushed");
        anim.ResetTrigger("Sled");
        anim.ResetTrigger("Run");
        anim.ResetTrigger("Jump");

        anim.SetTrigger("Charge");
    }

    public void SetMove(bool isStart)
    {
        fire.SetActive(!isStart);
        this.isMove = isStart;

        if (!isStart)
        {
            anim.ResetTrigger("Pushed");
            anim.ResetTrigger("Sled");
            anim.ResetTrigger("Run");
            anim.ResetTrigger("Jump");

            anim.SetTrigger("Charge");
        }
        else
            anim.SetTrigger("Run");
    }


    private void Update()
    {
        if (isClear)
        {
            transform.Translate(Vector2.right * 2 * Time.timeScale);
            return;
        }

        //죽음
        if (transform.position.x < -Camera.main.orthographicSize * Screen.width / Screen.height - 2
            || transform.position.y < -Camera.main.orthographicSize)//스크린 밖으로 나가면 게임 오버
            Die();



        if (!isJump && Physics2D.OverlapPoint(foot.position, mask))//바닥에 있을때
        {
            if (Physics2D.OverlapPoint(front.position, mask))//벽에 밀리고 있을때 그만 달려라
            {
                anim.SetTrigger("Pushed");
            }
            else if (transform.position.x < -25)
            {
                anim.SetTrigger("Run");
                Vector2 pos = transform.position;
                pos.x += 3 * Time.deltaTime;
                if (pos.x > -25)
                    pos.x = -25;
                transform.position = pos;
            }
            else
            {
                anim.SetTrigger("Run");
            }
        }

        if (!isMove) return;

        //점프 시작
        if (Input.GetMouseButtonDown(0) && Physics2D.OverlapPoint(foot.position, mask))
        {
            isJump = true;
            isJumping = true;
            rig.velocity = new Vector2(0, StartJumpPower);
            jumpTime = 0;

            coll.enabled = false;

            anim.ResetTrigger("Pushed");
            anim.ResetTrigger("Sled");
            anim.ResetTrigger("Run");
            anim.ResetTrigger("Jump");
            anim.SetTrigger("Jump");
        }

        if (isJumping && Input.GetMouseButton(0))//마우스를 누르고 있는 동안에는 계속 떠오름
        {
            jumpTime += Time.deltaTime;
            if (jumpTime > 0.1f)//0.1초부터 보너스 체공 타임 제공
            {
                float curJumpPower = rig.velocity.y + plusJumpPower * Time.deltaTime;

                rig.velocity = new Vector2(0, curJumpPower);
                if (jumpTime >= 1 || curJumpPower <= 0)
                    isJumping = false;
            }
        }

        if (Input.GetMouseButtonUp(0))//마우스 버튼 떨어지면 그만
            isJumping = false;

        //올라갈때는 무시하고 내려갈때 다시 신경씀
        if (isJump && rig.velocity.y < 0)
        {
            coll.enabled = true;
            isJump = false;
            isJumping = false;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)//충돌
    {
        switch (collision.tag)
        {
            case "Coin"://코인
                GameManager.I.GetMoney(collision.GetComponent<ObjectInfo>().cost);
                collision.gameObject.SetActive(false);//돈
                break;

            case "Fever"://피버꼬기
                GameManager.I.GetFever((int)collision.GetComponent<ObjectInfo>().type%100-1);//피버 먹기
                collision.gameObject.SetActive(false);//피버 물고기 사라짐

                break;

            case "Obstacle"://장애물
                Debug.Log("아이코!");
                collision.enabled = false;
                if (GameManager.I.GetDamage())
                    Die();//사망선고 받음 ㅠ
                break;
        }
    }

    private void Die()//죽음
    {
        anim.ResetTrigger("Pushed");
        anim.ResetTrigger("Sled");
        anim.ResetTrigger("Run");
        anim.ResetTrigger("Jump");
        anim.SetTrigger("Die");
        GameManager.I.GameOver();
        this.enabled = false;
    }

    public void Clear()
    {
        isClear = true;
    }


    //private void OnCollisionEnter2D(Collision2D collision)//이것도 개쓸모 없지만 혹시모르니깐
    //{
    //    rig.velocity = Vector2.zero;
        
    //    anim.ResetTrigger("Jump");
    //    anim.SetTrigger("Run");

    //    //OnCollisionStay2D(collision);
    //}

   

    //private void OnCollisionStay2D(Collision2D collision)//경사를 위해 사용했던 코드 이제는 쓸일이 없을 거 같지만 혹시몰라서 남겨둠
    //{
    //    float angle = -90 * Mathf.Deg2Rad;
    //    Vector2 normal = collision.contacts[0].normal;
    //    dir.x = normal.x * Mathf.Cos(angle) - normal.y * Mathf.Sin(angle);
    //    dir.y = normal.x * Mathf.Sin(angle) + normal.y * Mathf.Cos(angle);

    //    GameManager.I.dir = dir;//x축 속도 조절

    //    Quaternion v = Quaternion.FromToRotation(Vector3.up, normal);
    //    transform.localRotation = v;

    //    if (v.eulerAngles.z != 0)
    //    {
    //        anim.ResetTrigger("Run");
    //        anim.SetTrigger("Sled");
    //    }
    //    else
    //    {
    //        anim.ResetTrigger("Sled");
    //        anim.SetTrigger("Run");
    //    }
    //}
}

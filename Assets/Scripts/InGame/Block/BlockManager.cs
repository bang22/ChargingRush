using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct data
{
    public string path;
    public int size;
}


public class BlockManager : MonoBehaviour
{
    public bool testmode = false;
    public string path;

    [Header("맵 데이터들")]
    public data[] datas;
    protected int index=0;
    
    //public TextAsset[] blockTxtAsset;
    
    [Header("씬에 있는 블럭들")]
    public Block[] movingBlocks = new Block[3];//지금움직이고 있는 블록들

    private float allBlockSize;//모든 블록 사이즈

    private bool isEnd;
    private int endCount = 0;

    //오브젝트원본들
    [Header("오브젝트 원본들")]
    [SerializeField]
    private GameObject[] floorsOrigin;
    [SerializeField]
    private GameObject[] platformOrigin;
    [SerializeField]
    private GameObject[] obstacleOrigin;
    [SerializeField]
    private GameObject[] coinOrigin;
    [SerializeField]
    private GameObject feverOrigin;
    [SerializeField]
    private Sprite[] feverSkin;

    //오브젝트 풀
    private List<Transform>[] floors;
    private List<Transform>[] platforms;
    private List<Transform>[] obstacle;
    private List<Transform>[] coin;
    private List<Transform> fever;

    [SerializeField]
    protected float speed;//속도
    //public Vector2 dir;//경사


    private TextAsset GetNextBlockData()
    {
        TextAsset ta = Resources.Load(datas[0].path + index.ToString()) as TextAsset;
        if (index < datas[0].size)
            index++;
        else
            isEnd = true;
        return ta;
    }

    protected void BlockInit()
    {
        isEnd = false;
        endCount = 0;
        index = 0;

        //오브젝트 풀 초기화
        floors = new List<Transform>[floorsOrigin.Length];
        for (int i = 0; i < floorsOrigin.Length; i++)
            floors[i] = new List<Transform>();
        platforms = new List<Transform>[floorsOrigin.Length];
        for (int i = 0; i < platformOrigin.Length; i++)
            platforms[i] = new List<Transform>();
        obstacle = new List<Transform>[obstacleOrigin.Length];
        for (int i = 0; i < obstacleOrigin.Length; i++)
            obstacle[i] = new List<Transform>();
        coin = new List<Transform>[coinOrigin.Length];
        for (int i = 0; i < coinOrigin.Length; i++)
            coin[i] = new List<Transform>();
        fever = new List<Transform>();


        //블럭들 초기화
        Vector2 pos;
        float size;//블럭 너비

        float start = -Camera.main.orthographicSize * Screen.width / Screen.height;//맵 시작점

        float lastEndY = -14;//마지막 Y점
        allBlockSize = 0;
        for (int i = 0; i < movingBlocks.Length; i++)
        {
            size = movingBlocks[i].buildBlock(GetNextBlockData());//블럭 만들기

            pos.x = start + allBlockSize + size / 2;
            pos.y = -Mathf.Abs(lastEndY - movingBlocks[i].Startpos.y);
            movingBlocks[i].transform.position = pos;

            allBlockSize += size;
            lastEndY = movingBlocks[i].transform.position.y + movingBlocks[i].Endpos.y;
        }
    }


    private Transform GetObject(int type)//오브젝트 풀
    {
        Transform o = null;
        switch ((int)type / 100)
        {
            case 1://바닥
                foreach (Transform obj in floors[type % 100])
                {
                    if (!obj.gameObject.activeInHierarchy) return obj;
                }
                o = Instantiate(floorsOrigin[type % 100]).transform;
                floors[type % 100].Add(o);
                break;

            case 2://플랫폼
                foreach (Transform obj in platforms[type % 100])
                {
                    if (!obj.gameObject.activeInHierarchy) return obj;
                }
                o = Instantiate(platformOrigin[type % 100]).transform;
                platforms[type % 100].Add(o);
                break;

            case 3://장애물
                foreach (Transform obj in obstacle[type % 100])
                {
                    if (!obj.gameObject.activeInHierarchy) return obj;
                }
                o = Instantiate(obstacleOrigin[type % 100]).transform;
                obstacle[type % 100].Add(o);
                break;

            case 4://피~바
                if (type % 100 == 0)//랜덤 물꼬기
                    type = Random.Range(1, 8) + 400;

                foreach (Transform obj in fever)
                {
                    if (!obj.gameObject.activeInHierarchy)
                    {
                        obj.GetComponent<SpriteRenderer>().sprite = feverSkin[type % 100 -1];//타입 설정
                        obj.GetComponent<ObjectInfo>().type = (ObjectType)type;//코드 설정
                        return obj;
                    }
                }
                o = Instantiate(feverOrigin).transform;//새로 만들기
                o.GetComponent<SpriteRenderer>().sprite = feverSkin[type % 100 -1];//타입 설정
                o.GetComponent<ObjectInfo>().type = (ObjectType)type;//코드 설정
                fever.Add(o);
                break;

            case 5:
                foreach (Transform obj in coin[type % 100])
                {
                    if (!obj.gameObject.activeInHierarchy) return obj;
                }
                o = Instantiate(coinOrigin[type % 100]).transform;
                coin[type % 100].Add(o);
                break;
        }
        
        return o;
    }

    //블럭생성을 위한 오브젝트 반환
    public void SetObject(Transform parent, int type, float x, float y)
    {
        Transform obj = GetObject(type);
        obj.gameObject.SetActive(true);
        obj.parent = parent;
        obj.localPosition = new Vector2(x, y);
    }


    //블럭 움직임
    protected bool MoveBlock()
    {
        float camsize = Camera.main.orthographicSize * Screen.width / Screen.height;
        Vector2 pos;

        int overBlock = -1;
        for (int i = 0; i < movingBlocks.Length; i++)//블럭 움직임
        {
            pos.x = movingBlocks[i].transform.position.x - (speed * Time.deltaTime);
            pos.y = movingBlocks[i].transform.position.y;
            movingBlocks[i].transform.position = pos;
            if (pos.x < -camsize - movingBlocks[i].size / 2) overBlock = i;
        }

        if (overBlock != -1)//넘어간 블럭 재배열
        {
            if(isEnd)
            {
                endCount++;
                if (endCount >= 2) return false;
            }

            int lastindex = overBlock - 1 < 0 ? movingBlocks.Length - 1 : overBlock - 1;
            float lastEndY = movingBlocks[lastindex].transform.position.y + movingBlocks[lastindex].Endpos.y;//이전 블럭의 마지막 Y
            float overBlocksize = movingBlocks[overBlock].size;//넘어간 블럭의 사이즈
            allBlockSize -= overBlocksize;//모든 블럭 사이즈에서 넘어간 블럭 사이즈 빼기

            float size = movingBlocks[overBlock].buildBlock(GetNextBlockData());//블럭 만들기

            pos.x = movingBlocks[overBlock].transform.position.x + allBlockSize + overBlocksize / 2 + size / 2;//x축 맞추기
            pos.y = -Mathf.Abs(lastEndY - movingBlocks[overBlock].Startpos.y);//y축 맞추기
            movingBlocks[overBlock].transform.position = pos;

            allBlockSize += size;//모든 블럭 사이즈에 새로만든 블럭 사이즈 더하기
        }
        return true;
    }
}
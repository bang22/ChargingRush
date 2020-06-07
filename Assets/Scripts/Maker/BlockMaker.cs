using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum ObjectType//오브젝트 타입
{
    FLOOR1 = 100,//바닥
    FLOOR2,
    FLOOR3,
    FLOOR4,
    FlOOR5,
    FlOOR6,

    PLATFORM = 200,

    OBSTACLE1 = 300,//장애물
    OBSTACLE2,
    OBSTACLE3,
    OBSTACLE4,

    FEVER = 400,//피버펭귄 랜덤
    FEVER_R,//빨
    FEVER_O,//주
    FEVER_Y,//노
    FEVER_G,//초
    FEVER_B,//파
    FEVER_N,//남
    FEVER_V,//보

    COIN1 = 500,//코인
    COIN2
}



public class BlockMaker : MonoBehaviour
{
    private void Start()
    {
        blockSize = floorsOrigin[0].GetComponent<Renderer>().bounds.size / 2;
        InitColl();
        StartCoroutine(AutoSave());
    }

    private void Update()
    {
        CameraUpdate();//카메라 움직임

        SetEndUpdate();//시작점 , 끝점 배치
        SelectObjectUpdate();//씬에있는 오브젝트 선택
        Drag();//드래그
        MakingUpdate();//오브젝트 배치
        if (Input.GetKeyDown(KeyCode.C))
            StartSetColl();
    }

    //const string m_strPath = "Assets/Resources/";//파일 경로
    const string m_strPath = "";

    public InputField fileName;
    public InputField rotate;

    [Header("에디터")]
    public GameObject objWindow;

    [Header("재료")]
    public GameObject[] floorsOrigin;
    public GameObject[] platformOrigin;
    public GameObject[] obstacleOrigin;
    public GameObject[] coinOrigin;
    public GameObject feverOrigin;
    public Sprite[] feverSkin;

    //카메라

    public float camMoveSpeed = 0.01f;

    private Vector2 camStartPos;
    private Vector2 mouseStartPos;
    private void CameraUpdate()
    {
        if (Input.GetMouseButtonDown(2))//마우스 가운데 버튼
        {
            camStartPos = Camera.main.transform.position;
            mouseStartPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            Vector3 pos = camStartPos + (mouseStartPos - (Vector2)Input.mousePosition) * camMoveSpeed;
            pos.z = -10;
            Camera.main.transform.position = pos;
        }
        Camera.main.orthographicSize += -Input.mouseScrollDelta.y;//크기
    }

    public void CamReset()//카메라 리셋
    {
        Camera.main.transform.position = new Vector3(0, 0, -10);
        Camera.main.orthographicSize = 5;
    }



    public void Save(bool autosave = false)//세입으!!
    {
        string data = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n";//xml로 뽑기위해
        ObjectInfo oi;

        data += "<block>\n";

        //위치 보정
        Vector2 pos, height;
        height = Vector2.zero;
        Vector2 rearrange;
        for (int i = 0; i < transform.childCount; i++)//자식들을 돌아보며 4꼭지점을 찾음
        {
            pos = transform.GetChild(i).localPosition;
            if (pos.y <= height.x) height.x = pos.y;//아래쪽
            if (pos.y >= height.y) height.y = pos.y;//위쪽
        }
        //보정값을 찾음
        rearrange.x = -(StartPos.position.x + Mathf.Abs(StartPos.position.x - EndPos.position.x) * 0.5f);
        rearrange.y = -(height.x + Mathf.Abs(height.x - height.y) * 0.5f);

        Debug.Log(rearrange);

        data += "\n\t<objects>\n";
        for (int i = 0; i < transform.childCount; i++)//자식들을 돌아보며 데이터를 채워나감
        {
            oi = transform.GetChild(i).GetComponent<ObjectInfo>();
            if (oi)
            {
                data += "\t\t<object>\n";
                data += "\t\t\t<type>" + ((int)oi.type).ToString() + "</type>\n";
                data += "\t\t\t<x>" + (oi.transform.localPosition.x + rearrange.x).ToString() + "</x>\n";
                data += "\t\t\t<y>" + (oi.transform.localPosition.y + rearrange.y).ToString() + "</y>\n";
                data += "\t\t</object>\n";
            }
        }
        data += "\t</objects>\n\n";

        data += "\t<colls>\n";
        for (int i = 0; i < allPaths.Count; i++)//콜리더 경로들 
        {
            data += "\t\t<coll>\n";
            for (int j = 0; j < allPaths[i].Length; j++)
            {
                data += "\t\t\t<path>\n";
                data += "\t\t\t\t<x>" + (allPaths[i][j].x + rearrange.x).ToString() + "</x>\n";
                data += "\t\t\t\t<y>" + (allPaths[i][j].y + rearrange.y).ToString() + "</y>\n";
                data += "\t\t\t</path>\n";
            }
            data += "\t\t</coll>\n";
        }
        data += "\t</colls>\n\n";


        //시작점과 끝점
        data += "\n\t<startX>" + (StartPos.position.x + rearrange.x).ToString() + "</startX>";
        data += "\n\t<startY>" + (StartPos.position.y + rearrange.y).ToString() + "</startY>\n";
        data += "\n\t<endX>" + (EndPos.position.x + rearrange.x).ToString() + "</endX>";
        data += "\n\t<endY>" + (EndPos.position.y + rearrange.y).ToString() + "</endY>\n";

        data += "</block>";
        WriteData(data, autosave);
    }

    public void Load()//로드
    {
        Clear();

        XmlDocument xmlDoc = new XmlDocument();
        string data = ReadData();
        if (string.IsNullOrEmpty(data)) return;

        xmlDoc.LoadXml(data);

        //타입과 좌표를 받아서 블럭 만들기
        int type;
        float x, y;
        Transform obj = null;

        //보정값
        Vector2 correction;
        correction.x = float.Parse(xmlDoc.SelectSingleNode("block/objects/object/x").InnerText);
        correction.y = float.Parse(xmlDoc.SelectSingleNode("block/objects/object/y").InnerText);

        correction = correction - new Vector2(Mathf.Floor(correction.x / blockSize.x) * blockSize.x + blockSize.x / 2.0f, Mathf.Floor(correction.y / blockSize.y) * blockSize.y + blockSize.y / 2.0f);
        foreach (XmlNode node in xmlDoc.SelectNodes("block/objects/object"))
        {
            type = int.Parse(node.SelectSingleNode("type").InnerText);
            x = float.Parse(node.SelectSingleNode("x").InnerText);
            y = float.Parse(node.SelectSingleNode("y").InnerText);
            switch (type / 100)//타입에 따라 오브젝트 만들기
            {
                case 1://바닥
                    obj = Instantiate(floorsOrigin[type % 100]).transform;
                    break;

                case 2://플랫폼
                    obj = Instantiate(platformOrigin[type % 100]).transform;
                    break;

                case 3://장애물
                    obj = Instantiate(obstacleOrigin[type % 100]).transform;
                    break;

                case 4://물꼬기
                    obj = Instantiate(feverOrigin).transform;
                    obj.GetComponent<SpriteRenderer>().sprite = feverSkin[type % 100];
                    break;

                case 5:
                    obj = Instantiate(coinOrigin[type % 100]).transform;
                    break;
            }
            obj.parent = transform;
            obj.localPosition = new Vector2(x, y);
            obj.localPosition -= (Vector3)correction;
        }

        //콜리더
        allPaths.Clear();
        Vector2[] path;
        selectSlider.value = 0;
        selectSlider.maxValue = 0;
        foreach (XmlNode node in xmlDoc.SelectNodes("block/colls/coll"))
        {
            path = new Vector2[node.SelectNodes("path").Count];
            for (int i = 0; i < node.SelectNodes("path").Count; i++)
            {
                path[i].x = float.Parse(node.SelectNodes("path")[i].SelectSingleNode("x").InnerText);
                path[i].y = float.Parse(node.SelectNodes("path")[i].SelectSingleNode("y").InnerText);
                path[i] -= correction;
            }
            allPaths.Add(path);
        }


        //시작점,끝점
        StartPos.gameObject.SetActive(true);
        StartPos.position = new Vector2(float.Parse(xmlDoc.SelectSingleNode("block/startX").InnerText), float.Parse(xmlDoc.SelectSingleNode("block/startY").InnerText));
        StartPos.position -= (Vector3)correction;

        EndPos.gameObject.SetActive(true);
        EndPos.position = new Vector2(float.Parse(xmlDoc.SelectSingleNode("block/endX").InnerText), float.Parse(xmlDoc.SelectSingleNode("block/endY").InnerText));
        EndPos.position -= (Vector3)correction;
    }

    private void WriteData(string strData, bool autosave)
    {
        string path = m_strPath + (autosave ? "autoSave/save_" + System.DateTime.Now.ToString("MM-dd_HH-mm") : fileName.text) + ".xml";
        File.Delete(path);//먼저 원래있던 파일의 데이터 지우기

        //작성
        FileStream f = new FileStream(path, FileMode.Append, FileAccess.Write);
        StreamWriter writer = new StreamWriter(f, System.Text.Encoding.Unicode);
        writer.WriteLine(strData);

        writer.Close();
    }

    private string ReadData()
    {
        //읽기
        string data;
        FileStream f = new FileStream(m_strPath + fileName.text + ".xml", FileMode.Open, FileAccess.Read);
        if (!f.CanRead) return null;
        StreamReader reader = new StreamReader(f, System.Text.Encoding.Unicode);
        data = reader.ReadToEnd();

        reader.Close();

        return data;
    }

    /// <배치>
    /// 오브젝트 배치하기
    /// </배치>

    public enum SelectState
    {
        SELECT,
        DRAGED,
        MOVE,
        SETSTART,
        SETEND,

        SETCOLL
    }

    public SelectState state;
    private Vector2 blockSize;
    public List<Transform> selectedObject = new List<Transform>();
    private SpriteRenderer selectedObjectCol;
    private Vector2 selectedObjectSize;

    public Transform StartPos;
    public Transform EndPos;
    public LayerMask mask;

    private bool isDestroy = false;

    private void MakingUpdate()//배치
    {
        if (state == SelectState.MOVE || state == SelectState.DRAGED)
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                ////자석기능
                //Transform other;
                //Vector2 otherSize;


                //for (int i = 0; i < transform.childCount; i++)
                //{
                //    other = transform.GetChild(i);
                //    otherSize = other.GetComponent<Renderer>().bounds.size;

                //    if (Mathf.Abs((pos.x + selectedObjectSize.x * 0.5f) - (other.position.x - otherSize.x * 0.5f)) < 0.5f)//선택된 오브젝트가 왼쪽끝에 가까울때
                //    {
                //        pos.x = other.position.x - selectedObjectSize.x * 0.5f - otherSize.x * 0.5f;
                //        if (Mathf.Abs(((pos.y + selectedObjectSize.y * 0.5f) - (other.position.y + otherSize.y * 0.5f))) < 0.5f)//y축 끝부분
                //            pos.y = other.position.y + otherSize.y * 0.5f - selectedObjectSize.y * 0.5f;
                //    }
                //    if (Mathf.Abs((pos.x - selectedObjectSize.x * 0.5f) - (other.position.x + otherSize.x * 0.5f)) < 0.5f)//선택된 오브젝트가 오른쪽끝에 가까울때
                //    {
                //        pos.x = other.position.x + selectedObjectSize.x * 0.5f + otherSize.x * 0.5f;
                //        if (Mathf.Abs(((pos.y + selectedObjectSize.y * 0.5f) - (other.position.y + otherSize.y * 0.5f))) < 0.5f)//y축 끝부분
                //            pos.y = other.position.y + otherSize.y * 0.5f - selectedObjectSize.y * 0.5f;
                //    }
                //    if (Mathf.Abs((pos.y + selectedObjectSize.y * 0.5f) - (other.position.y - otherSize.y * 0.5f)) < 0.5f)//선택된 오브젝트가 아래쪽끝에 가까울때
                //        pos.y = other.position.y - selectedObjectSize.y * 0.5f - otherSize.y * 0.5f;
                //    if (Mathf.Abs((pos.y - selectedObjectSize.y * 0.5f) - (other.position.y + otherSize.y * 0.5f)) < 0.5f)//선택된 오브젝트가 위쪽끝에 가까울때
                //        pos.y = other.position.y + selectedObjectSize.y * 0.5f + otherSize.y * 0.5f;
                //}

                pos = new Vector2(Mathf.Floor(pos.x / blockSize.x) * blockSize.x + blockSize.x / 2.0f, Mathf.Floor(pos.y / blockSize.y) * blockSize.y + blockSize.y / 2.0f);

                Vector3 plusPos = (Vector2)selectedObject[0].position - pos;
                selectedObject[0].position = pos;

                for (int i = 1; i < selectedObject.Count; i++)
                {
                    selectedObject[i].position -= plusPos;
                }

                //selectedObject[0].position = pos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if(state == SelectState.DRAGED)
                {
                    state = SelectState.MOVE;
                    return;
                }
                if (isDestroy)
                {
                    foreach (Transform obj in selectedObject)
                        Destroy(obj.gameObject);
                }
                else
                {
                    foreach (Transform obj in selectedObject)
                        obj.GetComponent<SpriteRenderer>().color = Vector4.one;
                }

                selectedObject.Clear();
                state = SelectState.SELECT;
                objWindow.SetActive(true);
            }
        }

        if (state == SelectState.SETCOLL)
        {
            SetColl();
        }
    }

    public void SelectObjectUpdate()//이미 씬에 올라간 오브젝트 선택하기
    {
        if (state != SelectState.SELECT) return;
        if (state == SelectState.SELECT && Input.GetMouseButtonDown(0))
        {

            //선택
            Vector2 Mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Transform other;

            for (int i = 0; i < transform.childCount; i++)
            {
                other = transform.GetChild(i);
                Vector2 otherSize = other.GetComponent<Renderer>().bounds.size;

                if (Mpos.x <= other.transform.position.x + otherSize.x * 0.5f && Mpos.x >= other.transform.position.x - otherSize.x * 0.5f &&
                    Mpos.y <= other.transform.position.y + otherSize.y * 0.5f && Mpos.y >= other.transform.position.y - otherSize.y * 0.5f)//점 충돌체크
                {
                    selectedObject.Clear();

                    objWindow.SetActive(false);
                    selectedObject.Add(other.transform);
                    selectedObjectCol = other.transform.GetComponent<SpriteRenderer>();
                    selectedObjectSize = other.GetComponent<Renderer>().bounds.size;
                    selectedObject[0].GetComponent<SpriteRenderer>().color = new Vector4(0.8f, 0.8f, 0.8f, 0.8f);
                    state = SelectState.MOVE;
                    isDestroy = false;
                    break;
                }
            }
        }
    }

    private Vector2 DStart;
    private Vector2 DEnd;
    private bool isDrag=false;
    [SerializeField]
    private LineRenderer DragLine;

    private void Drag()
    {
        if (state != SelectState.SELECT)
        {
            isDrag = false;
            return;
        }

        if (state == SelectState.SELECT && Input.GetMouseButtonDown(0))
        {
            isDrag = true;
            DragLine.enabled = true;
            DStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return;
        }

        if (!isDrag) return;

        DEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (state == SelectState.SELECT && Input.GetMouseButton(0))
        {
            DragLine.SetPosition(0, new Vector3(DStart.x, DStart.y));
            DragLine.SetPosition(1, new Vector3(DEnd.x, DStart.y));
            DragLine.SetPosition(2, new Vector3(DEnd.x, DEnd.y));
            DragLine.SetPosition(3, new Vector3(DStart.x, DEnd.y));
        }

        if (state == SelectState.SELECT && Input.GetMouseButtonUp(0))
        {
            DragLine.enabled = false;
            float swap;

            if (DStart.x > DEnd.x)
            {
                swap = DStart.x;
                DStart.x = DEnd.x;
                DEnd.x = swap;
            }
            if (DStart.y < DEnd.y)
            {
                swap = DStart.y;
                DStart.y = DEnd.y;
                DEnd.y = swap;
            }
            

            Vector2 objP;
            for (int i = 0; i < transform.childCount; i++)
            {
                objP = transform.GetChild(i).position;
                if ((DStart.x < objP.x && objP.x < DEnd.x) && (DStart.y > objP.y && objP.y > DEnd.y))
                {
                    Debug.Log(0);
                    selectedObject.Add(transform.GetChild(i));
                    transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Vector4(0.8f, 0.8f, 0.8f, 0.8f);

                    state = SelectState.DRAGED;
                }
            }
        }
    }

    public void MakeObject(int code)//리스트에서 오브젝트 만들기 시작
    {
        selectedObject.Clear();
        if (state != SelectState.SELECT) return;
        //objWindow.SetActive(false);
        switch (code / 100)//코드에 따라 오브젝트 만들기
        {
            case 1://바닥
                selectedObject.Add(Instantiate(floorsOrigin[code % 100]).transform);
                break;

            case 2://플랫폼
                selectedObject.Add(Instantiate(platformOrigin[code % 100]).transform);
                break;

            case 3://장애물
                selectedObject.Add(Instantiate(obstacleOrigin[code % 100]).transform);
                break;
            case 4://피바
                selectedObject.Add(Instantiate(feverOrigin).transform);
                selectedObject[0].GetComponent<SpriteRenderer>().sprite = feverSkin[code % 100];//스프라이트 설정
                selectedObject[0].GetComponent<ObjectInfo>().type = (ObjectType)code;//코드 설정
                break;
            case 5://코인
                selectedObject.Add(Instantiate(coinOrigin[code % 100]).transform);
                break;
        }

        selectedObject[0].GetComponent<SpriteRenderer>().color = new Vector4(0.8f, 0.8f, 0.8f, 0.8f);
        //오브젝트를 옮기는데 필요한 컴포넌트들 저장
        selectedObject[0].parent = transform;
        selectedObjectCol = selectedObject[0].GetComponent<SpriteRenderer>();
        selectedObjectSize = selectedObject[0].GetComponent<Renderer>().bounds.size;

        state = SelectState.MOVE;
        isDestroy = false;
    }

    public void DestroyObject(bool isDestroy)//오브젝트 삭제
    {
        if (state == SelectState.MOVE)
            this.isDestroy = isDestroy;
    }












    //시작점 끝점 정하기
    bool select = false;
    public void SetEndUpdate()
    {
        if (!(state == SelectState.SETSTART || state == SelectState.SETEND)) return;
        Debug.Log(state);
        if (!Input.GetMouseButtonUp(0)) return;

        if (!select)//버튼클릭 동시에 실행 막기
        {
            select = true;
            return;
        }

        Transform other;
        for (int i = 0; i < transform.childCount; i++)
        {
            //선택
            Vector2 Mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            other = transform.GetChild(i);
            Vector2 otherSize = other.GetComponent<Renderer>().bounds.size;

            if (Mpos.x <= other.transform.position.x + otherSize.x * 0.5f && Mpos.x >= other.transform.position.x - otherSize.x * 0.5f &&
                Mpos.y <= other.transform.position.y + otherSize.y * 0.5f && Mpos.y >= other.transform.position.y - otherSize.y * 0.5f)//점 충돌체크
            {
                Vector2 pos = other.transform.position;
                Vector2 size = other.transform.GetComponent<Renderer>().bounds.size;
                Debug.Log(state);
                if (state == SelectState.SETSTART)
                {
                    pos.x = other.transform.position.x - size.x * 0.5f;
                    pos.y += size.y * 0.5f;
                    StartPos.transform.position = pos;
                    StartPos.gameObject.SetActive(true);
                }
                else
                {
                    pos.x = other.transform.position.x + size.x * 0.5f;
                    pos.y += size.y * 0.5f;
                    EndPos.transform.position = pos;
                    EndPos.gameObject.SetActive(true);
                }
                select = false;
                state = SelectState.SELECT;
                return;
            }
        }
    }

    public void SetEndPos(bool side)
    {
        if (side)
        {
            StartPos.gameObject.SetActive(false);
            state = SelectState.SETSTART;
            Debug.Log(side);
        }
        else
        {
            EndPos.gameObject.SetActive(false);
            state = SelectState.SETEND;
            Debug.Log(side);
        }
    }

    public void Clear()
    {
        StartPos.gameObject.SetActive(false);
        EndPos.gameObject.SetActive(false);

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        InitColl();
    }






    ///콜리더
    [Header("콜리더")]
    public Text selectText;
    public Slider selectSlider;
    public PolygonCollider2D coll;
    public LineRenderer collLine;//보여주기

    private List<Vector2[]> allPaths = new List<Vector2[]>();
    private List<Vector2> paths = new List<Vector2>();

    private void InitColl()//초기화
    {
        allPaths.Clear();
        allPaths.Add(new Vector2[0]);

        paths.Clear();

        coll.pathCount = 0;//콜리더 초기화
        collLine.positionCount = 0;
        //슬라이더
        selectSlider.value = 0;
        selectSlider.maxValue = 0;
    }

    public void CollSlider()
    {
        selectText.text = "콜리더 선택 : " + selectSlider.value.ToString();

        collLine.positionCount = 0;
        collLine.positionCount = allPaths[(int)selectSlider.value].Length;
        Debug.Log(allPaths[(int)selectSlider.value].Length);
        for (int i = 0; i < allPaths[(int)selectSlider.value].Length; i++)
        {
            Debug.Log(allPaths[(int)selectSlider.value][i]);
            collLine.SetPosition(i, allPaths[(int)selectSlider.value][i]);
        }
    }

    public void NewColl()//새 콜리더
    {
        allPaths.Add(new Vector2[0]);

        coll.pathCount += 1;
        selectSlider.maxValue += 1;
        selectSlider.value = selectSlider.maxValue;
    }

    public void DelColl()
    {
        allPaths.Remove(allPaths[(int)selectSlider.value]);

        //슬라이더
        selectSlider.maxValue -= 1;
        selectSlider.value -= 1;
        CollSlider();
    }

    public void StartSetColl()
    {
        paths.Clear();
        //라인렌더러
        collLine.positionCount = 1;

        state = SelectState.SETCOLL;
    }

    private void SetColl()
    {
        if (Input.GetMouseButtonDown(0))//그만
        {
            EndSetColl();
            return;
        }

        Vector2 mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos = Vector2.zero;
        bool done = false;
        //자석기능
        Transform other;
        Vector2 otherSize;


        for (int i = 0; i < transform.childCount; i++)
        {
            other = transform.GetChild(i);
            otherSize = other.GetComponent<Renderer>().bounds.size;

            if (other.tag != "platform") continue;

            if (Mathf.Abs((mpos.y) - (other.position.y + otherSize.y * 0.5f)) < 0.3f)//상
            {
                pos.y = other.position.y + otherSize.y * 0.5f;

                if (Mathf.Abs((mpos.x) - (other.position.x - otherSize.x * 0.5f)) < 0.3f)//좌상
                {
                    pos.x = other.position.x - otherSize.x * 0.5f;
                    done = true;
                }
                else if (Mathf.Abs((mpos.x) - (other.position.x + otherSize.x * 0.5f)) < 0.3f)//우상
                {
                    pos.x = other.position.x + otherSize.x * 0.5f;
                    done = true;
                }
            }
            else if (Mathf.Abs((mpos.y) - (other.position.y - otherSize.y * 0.5f)) < 0.3f)//하
            {
                pos.y = other.position.y - otherSize.y * 0.5f;

                if (Mathf.Abs((mpos.x) - (other.position.x - otherSize.x * 0.5f)) < 0.3f)//좌하
                {
                    pos.x = other.position.x - otherSize.x * 0.5f;
                    done = true;
                }
                else if (Mathf.Abs((mpos.x) - (other.position.x + otherSize.x * 0.5f)) < 0.3f)//우하
                {
                    pos.x = other.position.x + otherSize.x * 0.5f;
                    done = true;
                }
            }
        }
        collLine.SetPosition(collLine.positionCount - 1, mpos);

        if (!done) return;

        foreach (Vector2 v in paths)//체크 중복
        {
            if (v == pos) return;
        }
        collLine.SetPosition(collLine.positionCount - 1, pos);//마그넷
        //다음 라인
        collLine.positionCount += 1;
        collLine.SetPosition(collLine.positionCount - 1, mpos);

        paths.Add(pos);
    }

    public void EndSetColl()
    {
        isDrag = false;

        state = SelectState.SELECT;

        collLine.positionCount -= 1;//라인
        coll.SetPath((int)selectSlider.value, paths.ToArray());//콜리더
        allPaths[(int)selectSlider.value] = paths.ToArray();//목록
    }


    IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(180);//3분마다 오토셉
            Save(true);
        }
    }
}
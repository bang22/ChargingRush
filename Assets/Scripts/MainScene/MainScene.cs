using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    //클리커
    [SerializeField]
    private Text money;
    [SerializeField]
    private Transform sunglasses;
    private IEnumerator sunglassPopSeq;

    //돈
    [SerializeField]
    GameObject moneyOrigin;
    [SerializeField]
    Transform moneyParent;
    List<Image> moneys = new List<Image>();

    //포즈
    [SerializeField]
    private List<GameObject> poses = new List<GameObject>();

    private void Start()
    {
        Screen.SetResolution(Screen.width, (Screen.width / 16) * 9, true);

        money.text = PlayerPrefs.GetInt("money", 0).ToString();

        poses[Random.Range(0, poses.Count)].SetActive(true);
    }

    public void MakeMoney()//클릭
    {
        Debug.Log("돈이 들어온다!");
        PlayerPrefs.SetInt("money", PlayerPrefs.GetInt("money", 0) + PlayerPrefs.GetInt("clickmoney", 1));
        money.text = PlayerPrefs.GetInt("money", 0).ToString();

        //팝
        if (sunglassPopSeq != null)
            StopCoroutine(sunglassPopSeq);
        sunglassPopSeq = SunGlassesPop();
        StartCoroutine(sunglassPopSeq);

        //돈 효과

        Image mo=null;

        for(int i=0;i<moneys.Count;i++)
        {
            if(!moneys[i].gameObject.activeInHierarchy)
            {
                mo = moneys[i];
                break;
            }
        }
        if(mo == null)
        {
            mo = Instantiate(moneyOrigin,moneyParent).GetComponent<Image>();
            moneys.Add(mo);
        }
        
        mo.transform.position = Input.mousePosition ;
        Debug.Log(Input.mousePosition);
        mo.gameObject.SetActive(true);
        StartCoroutine(MoneyMove(mo));
    }

    IEnumerator SunGlassesPop()
    {
        float progress = 0;
        while (true)
        {
            yield return 0;

            progress += Time.deltaTime*10;

            if(progress < 1)
                sunglasses.localScale = Vector3.Lerp(Vector2.one,new Vector2(1.1f,1.1f), progress);
            else
                sunglasses.localScale = Vector3.Lerp(new Vector2(1.1f, 1.1f), Vector2.one, progress-1);
            if (progress >= 2)
                break;
        }
    }

    IEnumerator MoneyMove(Image money)
    {
        float progress=0;

        Vector2 SPos = money.transform.position;
        Vector2 EPos = money.transform.position;
        EPos.y += 40;

        while (true)
        {
            yield return 0;
            money.transform.position = Vector2.Lerp(SPos, EPos, progress);
            money.color = new Vector4(1, 1, 1, 1-progress);
            progress += Time.deltaTime*2;
            if (progress >= 1) break;
        }
        money.gameObject.SetActive(false);
    }

    //책
    [SerializeField]
    private GameObject Note;
    [SerializeField]
    private GameObject map;

    [SerializeField]
    private Animator bookAnim;
    [SerializeField]
    private Animator mapAnim;

    public void OpenDiary()//다이어리 열기
    {
        Note.SetActive(true);
        map.SetActive(false);
        bookAnim.Play("note@open");
    }

    public void OpenMap()//다이어리 열기
    {
        Note.SetActive(false);
        map.SetActive(true);
        mapAnim.Play("note@open");
    }


    public void CloseNote()
    {
        bookAnim.Play("note@close");
        StartCoroutine(p(Note));
    }

    public void CloseMap()
    {
        mapAnim.Play("note@close");
        StartCoroutine(p(map));
    }

    IEnumerator p(GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);
        obj.SetActive(false);
    }

    //씬 변경
    public void GameStart(string name)
    {
        SceneManager.LoadScene(name);
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum GameState
{
    CHARGING,   //차징중
    GO,         //가즈아
    GAMEOVER    //게임오바
}


public class GameManager : BlockManager
{
    /// 싱글톤 ///

    private static GameManager _Instance = null;

    public static GameManager I
    {
        get
        {
            if (_Instance == null)
            {
                Debug.Log("instance is null");
            }
            return _Instance;
        }
    }

    void Awake()
    {
        _Instance = this;
        //QualitySettings.vSyncCount = 0; // vsync 사용안함 
        //Application.targetFrameRate = 3; // 30 프레임 고정. 
    }

    private void Start()
    {
        if (!testmode)
            GameInit();
    }

    [Header("UI")]
    //차징
    [SerializeField]
    private Text energeText;//에너지 잔량
    [SerializeField]
    private Image timer;//타이머
    [SerializeField]
    private Sprite[] timerSprite;//타이머 이미지

    [SerializeField]
    private GameObject chargingUI;
    [SerializeField]
    private Image chargingHeart;
    private int count;

    //돈
    [SerializeField]
    private Text moneyText;
    private int money;

    //피버
    [SerializeField]
    private Image[] fishUI = new Image[7];//물고기 UI
    [SerializeField]
    private Sprite[] fishs = new Sprite[8];//물고기
    [SerializeField]
    private GameObject touchSign;
    [SerializeField]
    private GameObject feverBG;
    [SerializeField]
    private GameObject feverSign;

    //일시정지
    [SerializeField]
    private GameObject pauseButton;
    [SerializeField]
    private Animator pause;
    [SerializeField]
    private Transform home;
    [SerializeField]
    private Transform resume;
    [SerializeField]
    private Transform restart;

    private float limitTime = 1;//차징제한시간

    private GameState state;//게임 상태

    //에너지 (스테미너 || 체력)
    private float energy;

    public CameraMotion cam;
    public Player player;

    private void Update()
    {
        switch (state)
        {
            case GameState.CHARGING:
                Charging();
                break;

            case GameState.GO:
                if(!MoveBlock())//블럭 움직임
                {
                    GameOver();//스테이지 클리어;
                    player.Clear();
                }
                //ReduceEnergy();
                BGScroll();
                break;
        }
    }




    //*//차징//*//

    private void GameInit()
    {
        gameover.SetTrigger("close");
        BlockInit();
        for (int i = 0; i < fishUI.Length; i++)
        {
            fishUI[i].sprite = fishs[0];
        }

        count = 0;
        energy = 0;
        energeText.text = "0";
        moneyText.text = "0";

        money = 0;

        player.PlayerInit();
        //cam.transform.position = new Vector3(0, 0, -10);

        StopAllCoroutines();
        StartCoroutine(ChargingTimer());
    }

    private IEnumerator ChargingTimer(bool isfever = false)
    {
        if (isfever)
        {
            feverBG.SetActive(true);
            feverSign.SetActive(true);
        }
        else
        {
            touchSign.SetActive(true);
        }

        player.SetMove(false);//플레이어 정지
        state = GameState.CHARGING;

        timer.gameObject.SetActive(true);
        chargingUI.SetActive(true);
        for (int i = 5; i > 0; i--)
        {
            timer.sprite = timerSprite[i - 1];
            yield return new WaitForSeconds(1);
        }
        chargingUI.SetActive(false);
        timer.gameObject.SetActive(false);
        state = GameState.GO;
        player.SetMove(true);//플레이어 점프가능

        if (isfever)
        {
            feverBG.SetActive(false);
            feverSign.SetActive(false);
            InitFever();
        }
        else
        {
            touchSign.SetActive(false);
        }
    }

    private void Charging()//차징
    {
        //ChargingUI
        if (Input.GetMouseButtonDown(0))
        {
            chargingHeart.fillAmount = count / 3.0f;
            count++;
            if (count >= 3)
            {
                energy += 1;
                energeText.text = ((int)energy).ToString();
                count = 0;
            }
        }
    }

    //*//돈//*//
    public void GetMoney(int value)
    {
        PlayerPrefs.SetInt("money", PlayerPrefs.GetInt("money", 0) + value);
        money += value;
        moneyText.text = money.ToString();
    }

    //*//피버//*//

    public void GetFever(int num)//피버 채우기
    {
        fishUI[num].sprite = fishs[num + 1];
        StartCoroutine(bounce(fishUI[num].transform, 1.05f));

        //모든 꼬기 차있으면 피버 꼬!기!
        foreach (Image f in fishUI)
            if (f.sprite == fishs[0]) return;

        StopAllCoroutines();
        StartCoroutine(ChargingTimer(true));
    }

    private void InitFever()//피버 초기화
    {
        foreach (Image f in fishUI)
        {
            f.sprite = fishs[0];
        }
    }




    //*//게임 오버//*//
    [SerializeField]
    private Animator gameover;
    [SerializeField]
    private Text coinState;
    [SerializeField]
    private Image distanceGage;


    public void GameOver()
    {
        Debug.Log("asdasd");
        coinState.text = money.ToString();


        gameover.SetTrigger("open");
        state = GameState.GAMEOVER;

    }

    //*//게임플레이//*//

    //private void ReduceEnergy()//에너지 점점달기
    //{
    //    energy -= Time.deltaTime * (1);
    //    energeText.text = ((int)energy).ToString();
    //}


    //플레이어//

    public bool GetDamage()//데미지 받기
    {
        energy -=3;
        energeText.text = ((int)energy <=0 ? 0:(int)energy).ToString();
        cam.CamShake(0.2f); 

        return energy <= 0;//사망
    }


    //*//배경//*//

    [SerializeField]
    private MeshRenderer[] BGs = new MeshRenderer[4];
    float offset = 0;
    void BGScroll()
    {
        offset += speed/1000 * Time.deltaTime;

        for (int i = 0; i < BGs.Length; i++)
        {
            BGs[i].materials[0].mainTextureOffset = new Vector2(i * offset, 0);
        }
    }


    //*//UI//*//

    IEnumerator bounce(Transform t, float size, bool unscaled = false)//바운스바운스 두근대
    {
        float progress = 0;
        while (true)
        {
            yield return 0;
            if (unscaled)
                progress += Time.unscaledDeltaTime * 10;//타임스케일 무!시!
            else
                progress += Time.deltaTime * 10;

            if (progress < 1)
                t.localScale = Vector3.Lerp(Vector3.one, Vector3.one * size, progress);
            else if (progress <= 2)
                t.localScale = Vector3.Lerp(Vector3.one * size, Vector3.one, progress - 1);
            else
                break;
        }
    }

    IEnumerator delayLoadScene(string name, float time)
    {
        Time.timeScale = 1;
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(name);
    }

    public void GamePause()
    {
        Debug.Log("adasd");
        pause.SetTrigger("open");
        Time.timeScale = 0;
        StartCoroutine(bounce(home, 1.3f, true));
        pauseButton.SetActive(false);
    }

    public void Resume()
    {
        pause.SetTrigger("close");
        Time.timeScale = 1;
        StartCoroutine(bounce(resume, 1.3f));
        pauseButton.SetActive(true);

    }

    public void Restart()
    {
        pause.SetTrigger("close");
        Time.timeScale = 1;
        StartCoroutine(bounce(restart, 1.3f));
        pauseButton.SetActive(true);
        GameInit();
    }

    public void Home()
    {
        pause.SetTrigger("close");
        Time.timeScale = 1;
        StartCoroutine(bounce(home, 1.3f));
        StartCoroutine(delayLoadScene("main", 0.1f));
    }

    //////////////테스트
    public InputField pathField;
    public void Test()
    {
        path = pathField.text + ".xml";
        GameInit();
    }
}
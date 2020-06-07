using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{

    public Transform book;
    bool isOpen = false;

    public void Book()
    {
        StartCoroutine(BookAnim());
    }

    IEnumerator BookAnim()
    {
        Vector2 StartPos = new Vector2(0,-1000);
        float time = 0;

        while (true)
        {
            yield return 0;
            time += Time.deltaTime;

            if (!isOpen)
                book.transform.localPosition = Vector2.Lerp(StartPos, Vector2.zero, time);
            else
                book.transform.localPosition = Vector2.Lerp(Vector2.zero, StartPos, time);

            if (time >= 1) break;
        }
        isOpen = !isOpen;
    }

    public void GameStart(string name)
    {
        SceneManager.LoadScene(name);
    }
}

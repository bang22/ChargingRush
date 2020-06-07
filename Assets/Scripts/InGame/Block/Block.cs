using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Xml;

public class Block : MonoBehaviour {
    
    public float size;
    public PolygonCollider2D coll;
    public Vector2 Startpos;
    public Vector2 Endpos;

    public float buildBlock(TextAsset asset)
    {
        Startpos = Vector2.zero;
        Endpos = Vector2.zero;
        XmlDocument xmlDoc = new XmlDocument();

        if(GameManager.I.testmode)//테스트
        {
            FileStream f = new FileStream(GameManager.I.path, FileMode.Open, FileAccess.Read);
            if (!f.CanRead) return 0;
            StreamReader reader = new StreamReader(f, System.Text.Encoding.Unicode);
            xmlDoc.LoadXml(reader.ReadToEnd());

            reader.Close();
        }
        else
        xmlDoc.LoadXml(asset.text);

        //원래있던 자식들 비활성화해서 오브젝트풀에서 다시 쓸 수 있게 만들기
        Collider2D c;
        for (int i=0;i<transform.childCount;i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
            //콜리더 다시 활성화
            c = transform.GetChild(i).GetComponent<Collider2D>();
            if (c != null)
                c.enabled = true;
        }

        //타입과 좌표를 받아서 블럭 만들기
        int type;
        float x, y;
        foreach(XmlNode node in xmlDoc.SelectNodes("block/objects/object"))
        {
            type= int.Parse(node.SelectSingleNode("type").InnerText);
            x = float.Parse(node.SelectSingleNode("x").InnerText);
            y = float.Parse(node.SelectSingleNode("y").InnerText);
            GameManager.I.SetObject(transform, type, x, y);
        }

        //콜리더
        XmlNodeList list = xmlDoc.SelectNodes("block/colls/coll");
        XmlNode nod;

        Vector2[] path = new Vector2[list.Count];
        coll.pathCount = list.Count;

        for (int i=0;i< list.Count;i++)
        {
            nod = list[i];
            path = new Vector2[nod.SelectNodes("path").Count];
            for (int j = 0; j < nod.SelectNodes("path").Count; j++)
            {
                path[j].x = float.Parse(nod.SelectNodes("path")[j].SelectSingleNode("x").InnerText);
                path[j].y = float.Parse(nod.SelectNodes("path")[j].SelectSingleNode("y").InnerText);
            }
            coll.SetPath(i, path);
        }


        //사이즈 구하기
        Startpos.x = float.Parse(xmlDoc.SelectSingleNode("block/startX").InnerText);
        Startpos.y = float.Parse(xmlDoc.SelectSingleNode("block/startY").InnerText);
        
        Endpos.x = float.Parse(xmlDoc.SelectSingleNode("block/endX").InnerText);
        Endpos.y = float.Parse(xmlDoc.SelectSingleNode("block/endY").InnerText);

        size = Mathf.Abs(Startpos.x - Endpos.x);

        //Debug.Log(gameObject.name +" : "+ Startpos.ToString()+" & "+ Endpos.ToString());
        return size;
    }
}

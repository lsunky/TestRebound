using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRebound : MonoBehaviour
{
    private LineRenderer m_LineRenderer;
    private List<Vector3> vecList;

    private Transform ball;
    private Transform directBall;

    private Transform resultBall;

    
    public float Speed = 2;
    private bool inMove;

    private Vector3 curDir;
    private Vector3 curTarget;

    private (Vector3 crossPoint, Vector3 reflectDir, int index, Vector3 norDir) pack;
    // Start is called before the first frame update
    void Start()
    {
        ball = transform.Find("ball");
        directBall = transform.Find("directBall");
        resultBall = transform.Find("resultBall");
        m_LineRenderer = gameObject.AddComponent<LineRenderer>();
        m_LineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        m_LineRenderer.startWidth = 0.1f;
        m_LineRenderer.endWidth = 0.1f;

        Transform rectRoot = transform.Find("rect");
        vecList = new List<Vector3>();
        foreach (Transform transPoint in rectRoot)
        {
            vecList.Add(transPoint.position);
        }
        vecList.Add(vecList[0]);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoardPanel();
        if (inMove)
        {
            Vector3 endPos = ball.position + Speed * Time.deltaTime * curDir;
            Vector3 newDir = (curTarget - endPos).normalized;
            if (ArrivedPos(newDir ,curDir))
            {
                ball.position = curTarget;
                pack = GetNextPoint(pack.crossPoint, pack.reflectDir);
                this.curTarget = pack.crossPoint;
                curDir = (curTarget - ball.position).normalized;
                this.resultBall.position = curTarget;
            }
            else
            {
                ball.position = endPos;
            }
        }
    }

    private bool ArrivedPos(in Vector3 tempDir,in Vector3 curDir)
    {
        float distance = Vector3.Distance(tempDir, curDir);
        // Debug.Log($"tempDir: {tempDir} curDir: {curDir}");
        return distance >= 0.5f;
    }

    private void UpdateBoardPanel()
    {
        int length = vecList.Count;
        m_LineRenderer.positionCount = vecList.Count;
        for (int i = 0; i < length; i++)
        {
            m_LineRenderer.SetPosition(i, vecList[i]);
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0,0,100,60),"Test"))
        {
            this.resultBall.gameObject.SetActive(true);
            pack = GetNextPoint(directBall.position, ball.position);
            this.curTarget = pack.crossPoint;
            curDir = (curTarget - ball.position).normalized;
            this.resultBall.position = curTarget;
            inMove = true;
        }

        // if (GUI.Button(new Rect(0,100,100,60),"check"))
        // {
        //     int i = 1;
        //     var startPos = directBall.position;
        //     var endPos = ball.position;
        //     var point = GameObject.Instantiate(ball);
        //     point.position = vecList[i];
        //     point = GameObject.Instantiate(ball);
        //     point.position = vecList[i+1];
        //     
        //     bool isinside = Isinside(endPos, startPos, vecList[i], vecList[i + 1]);
        //     Debug.Log(isinside);
        // }
    }
    
    private (Vector3 crossPoint,Vector3 reflectDir,int index,Vector3 norDir) GetNextPoint(Vector3 startPos,Vector3 endPos)
    {
        int myLineIndex = GetHitLineIndex(startPos,endPos);
        Vector2 result = Vector2.zero;
        if (SegmentsInterPoint(startPos,endPos,vecList[myLineIndex],vecList[myLineIndex+1],ref result))
        {
            Vector3 result3 =   new Vector3(result.x, result.y, 0);
            resultBall.position = result3;
            Vector2 norDir = GetNorDir(vecList[myLineIndex]-vecList[myLineIndex+1]);
            Vector3 inDirection = endPos - startPos;
            Vector2 reflectDir = Vector2.Reflect(result3 - startPos, norDir)+result;
            return (result, reflectDir, myLineIndex,norDir);
        }
        else
        {
            Debug.LogError("计算错误");
        }

        return (Vector3.zero, Vector3.zero,0,Vector3.zero);
    }

    private int GetHitLineIndex(Vector3 startPos,Vector3 endPos)
    {
        // Vector3 startPos = this.directBall.position;
        // Vector3 ballPos = this.ball.position;

        for (int i = 0,length = vecList.Count-1; i < length; i++)
        {
            if (CheckIsCross(startPos,endPos,vecList[i],vecList[i+1]) || Isinside(endPos,startPos,vecList[i],vecList[i+1]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 2d平面法向量其实就是逆时针旋转90度
    /// x' = x cos(t) - y sin(t)
    /// y' = x sin(t) + y cos(t)
    /// t=90度
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static Vector2 GetNorDir(Vector2 dir)
    {
        return new Vector2(-dir.y, dir.x).normalized;
    }
    
    public static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - b.x * a.y;
    }
    
    /// <summary>
    /// 获取交点
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <param name="IntrPos"></param>
    /// <returns></returns>
    public static bool SegmentsInterPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 IntrPos)
    {
        //计算交点坐标  
        float t = Cross(a -c, d -c) / Cross (d-c,b-a);
        // Debug.Log($" t { t.ToString()}");
        float dx = t * (b.x - a.x);
        float dy = t * (b.y - a.y);

        IntrPos = new Vector2() { x = a.x + dx, y = a.y + dy };
        return true;
    }

    public bool CheckIsCross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        //v1×v2=x1y2-y1x2 
        //以线段ab为准，是否c，d在同一侧
        Vector2 ab = b - a;
        Vector2 ac = c - a;
        float abXac = Cross(ab,ac);

        Vector2 ad = d - a;
        float abXad = Cross(ab, ad);

        if (abXac * abXad >= 0)
        {
            return false;
        }

        //以线段cd为准，是否ab在同一侧
        Vector2 cd = d - c;
        Vector2 ca = a - c;
        Vector2 cb = b - c;

        float cdXca = Cross(cd, ca);
        float cdXcb = Cross(cd, cb);
        if (cdXca * cdXcb >= 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 判断点是否在三角形内
    /// </summary>
    /// <param name="point"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    bool Isinside(Vector3 point,Vector3 a,Vector3 b,Vector3 c)
    {
        Vector3 pa = a - point;
        Vector3 pb = b - point;
        Vector3 pc = c - point;
        Vector3 pab = Vector3.Cross(pa,pb);
        Vector3 pbc = Vector3.Cross(pb, pc);
        Vector3 pca = Vector3.Cross(pc, pa);
            
        float d1 = Vector3.Dot(pab, pbc);
        float d2 = Vector3.Dot(pab, pca);
        float d3 = Vector3.Dot(pbc, pca);
 
        if (d1 > 0 && d2 > 0 && d3 > 0) return true;
        return false;
    }
}

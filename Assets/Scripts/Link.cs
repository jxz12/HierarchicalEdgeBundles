using UnityEngine;
using System.Collections.Generic;

public class Link : MonoBehaviour
{
    [SerializeField] LineRenderer lr;
    List<Node> controlPoints;
    public void Init(Node src, Node tgt)
    {
        var path = new LinkedList<Node>();
        path.AddFirst(src);
        path.AddLast(tgt);
        var headSrc = path.First;
        var headTgt = path.Last;
        while (headSrc.Value != headTgt.Value)
        {
            if (headSrc.Value.Depth == headTgt.Value.Depth)
            {
                headSrc = path.AddAfter(headSrc, headSrc.Value.Parent);
                headTgt = path.AddBefore(headTgt, headTgt.Value.Parent);
            }
            else if (headSrc.Value.Depth > headTgt.Value.Depth) {
                headSrc = path.AddAfter(headSrc, headSrc.Value.Parent);
            } else {
                headTgt = path.AddBefore(headTgt, headTgt.Value.Parent);
            }
        }
        path.Remove(headSrc); // headSrc equals headTgt here so remove duplicate
        if (path.Count > 3) {
            path.Remove(headTgt); // remove lowest common ancestor as per Holten (2006)
        }
        controlPoints = new List<Node>(path);
        // if (src.name == "UnityEngine.LowLevel.PlayerLoop") {
        //     for (int i=0; i<controlPoints.Count; i++) {
        //         print($"{i} {controlPoints[i]}");
        //     }
        // }
    }
    // public void Highlight(Color c)
    // {
    //     //float a;
    //     //if (controlPoints.Count > 2)
    //     //    a = Mathf.Max(.2f, (1f / (controlPoints.Count - 2)));
    //     //else
    //     //    a = 1;
    //     //c.a = a;

    //     Col = c;
    //     lr.sortingOrder = 1;
    // }
    // public void Unhighlight()
    // {
    //     Col = DefaultCol;
    //     lr.sortingOrder = 0;
    //     lr.endColor = new Color(1,1,1,1);
    // }

    [SerializeField] int segmentsPerSpline = 5;
    public void Draw(float beta)
    {
        if (controlPoints.Count == 2)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, controlPoints[0].Pos);
            lr.SetPosition(1, controlPoints[1].Pos);
        }
        else if (beta == 0)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, controlPoints[0].Pos);
            lr.SetPosition(1, controlPoints[controlPoints.Count-1].Pos);
        }
        else
        {
            var p = new List<Vector3>();
            foreach (var v in controlPoints) {
                p.Add(v.Pos);
            }
            int N = p.Count;
            lr.positionCount = N;
            Vector3 pn1_0 = p[N - 1] - p[0];
            for (int i=1; i<N-1; i++) {
                p[i] = (beta * p[i]) + (1 - beta) * (p[0] + (float)i / (N - 1) * pn1_0);
            }
            var spline = new List<Vector3>(CubicParametricCurve.PiecewiseBspline(p, segmentsPerSpline));
            lr.positionCount = spline.Count;
            lr.SetPositions(spline.ToArray());
        }
    }
}
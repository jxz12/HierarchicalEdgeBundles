using UnityEngine;
using System.Collections.Generic;
using ParametricCurves;

public class Link : MonoBehaviour
{
    [SerializeField] LineRenderer lr;
    public void Init(Vector3 src, Vector3 tgt)
    {
        lr.SetPosition(0, src);
        lr.SetPosition(0, tgt);
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

    // public int segmentsPerSpline = 5;
    // public void Draw(float beta) {
    //     if (controlPoints.Count == 2)
    //     {
    //         lr.positionCount = 2;
    //         lr.SetPosition(0, controlPoints[0].Pos);
    //         lr.SetPosition(1, controlPoints[1].Pos);
    //     }
    //     else if (beta == 0)
    //     {
    //         lr.positionCount = 2;
    //         lr.SetPosition(0, controlPoints[0].Pos);
    //         lr.SetPosition(1, controlPoints[controlPoints.Count-1].Pos);
    //     }
    //     else
    //     {
    //         var p = new List<Vector3>();
    //         foreach (var v in controlPoints)
    //             p.Add(v.Pos);

    //         int N = p.Count;
    //         lr.positionCount = N;
    //         Vector3 pn1_0 = p[N - 1] - p[0];
    //         for (int i=1; i<N-1; i++)
    //             p[i] = (beta * p[i]) + (1 - beta) * (p[0] + (float)i / (N - 1) * pn1_0);

    //         List<Vector3> spline = CubicParametricCurve.PiecewiseBspline(p, segmentsPerSpline);
    //         lr.positionCount = spline.Count;
    //         for (int i=0; i<spline.Count; i++)
    //             lr.SetPosition(i, spline[i]);
    //             //lr.SetPosition(i, (Vector2)spline[i]);
    //     }
    // }

}
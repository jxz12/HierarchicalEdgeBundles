using UnityEngine;
using System.Collections.Generic;

public class Link : MonoBehaviour
{
    [SerializeField] LineRenderer lr;
    [SerializeField] Color startCol, endCol;
    void Awake()
    {
        var grad = new Gradient();
        var alphaKeys = new GradientAlphaKey[]{ new GradientAlphaKey(1,1), new GradientAlphaKey(1,1) };

        var viridis = new GradientColorKey[]{
            new GradientColorKey(new Color(0.267004f, 0.004874f, 0.329415f), 1),
            new GradientColorKey(new Color(0.275191f, 0.194905f, 0.496005f), 6/7f),
            new GradientColorKey(new Color(0.212395f, 0.359683f, 0.551710f), 5/7f),
            new GradientColorKey(new Color(0.153364f, 0.497000f, 0.557724f), 4/7f),
            new GradientColorKey(new Color(0.122312f, 0.633153f, 0.530398f), 3/7f),
            new GradientColorKey(new Color(0.288921f, 0.758394f, 0.428426f), 2/7f),
            new GradientColorKey(new Color(0.626579f, 0.854645f, 0.223353f), 1/7f),
            new GradientColorKey(new Color(0.993248f, 0.906157f, 0.143936f), 0),
        };
        grad.SetKeys(viridis, alphaKeys);

        // var startLab = LABColor.FromColor(startCol);
        // var endLab = LABColor.FromColor(endCol);
        // var colorKeys = new GradientColorKey[8];
        // for (int i=0; i<8; i++)
        // {
        //     float t = i/8f;
        //     var midLab = LABColor.Lerp(startLab, endLab, t);
        //     colorKeys[i] = new GradientColorKey(LABColor.ToColor(midLab), t);
        // }
        // grad.SetKeys(colorKeys, alphaKeys);

        lr.colorGradient = grad;
    }
    List<Node> controlPoints;
    public void FindControlPoints(Node src, Node tgt, bool removeLCA=true)
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
        if (removeLCA && path.Count > 3) {
            path.Remove(headTgt); // remove lowest common ancestor as per Holten (2006)
        }
        controlPoints = new List<Node>(path);
        // if (src.name == "UnityEngine.LowLevel.PlayerLoop") {
        //     for (int i=0; i<controlPoints.Count; i++) {
        //         print($"{i} {controlPoints[i]}");
        //     }
        // }
    }
    public void SetWidth(float width)
    {
        lr.startWidth = lr.endWidth = width;
    }
    public void Show(bool showing)
    {
        lr.enabled = showing;
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
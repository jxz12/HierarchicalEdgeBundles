using UnityEngine;
using System;
using System.Collections.Generic;

public class Dendrogram : MonoBehaviour
{
    [SerializeField] TextAsset graphTxt;
    [SerializeField] Link linkPrefab;
    [SerializeField] Node nodePrefab;

    Dictionary<int, HashSet<int>> adjacency;
    Dictionary<int, string> names;
    Dictionary<int, Vector2> radialPos;
    void Awake()
    {
        // get node names
        var lines = graphTxt.text.Split('\n');
        int lineIdx = 0;
        while (lines[lineIdx] != "")
        {
            var nodeTxt = lines[lineIdx].Split(':');
            int idx = int.Parse(nodeTxt[0]);
            names[idx] = nodeTxt[1];
            adjacency[idx] = new HashSet<int>();
            lineIdx += 1;
        }
        lineIdx += 1;
        // get adjacency
        while (lineIdx < lines.Length)
        {
            var linkTxt = lines[lineIdx].Split(' ');
            int src = int.Parse(linkTxt[0]);
            int tgt = int.Parse(linkTxt[1]);
            adjacency[src].Add(tgt);
            lineIdx += 1;
        }
        int nLeaves = CountLeaves(-1);
        int CountLeaves(int root)
        {
            int n = 0;
            foreach (int child in adjacency[root])
            {
                if (child < 0) {
                    n += CountLeaves(child);
                }
            }
            return n==0? 1:n;
        }
        int leafCounter = 0;
        int depth = LayoutTree(-1, 0);
        int LayoutTree(int root, int depth)
        {
            int nChildren = 0;
            int maxDepth = depth;
            float minAngle=2*Mathf.PI, maxAngle=0;
            foreach (int child in adjacency[root])
            {
                if (child < 0) {
                    maxDepth = Math.Max(maxDepth, LayoutTree(child, depth+1));
                    minAngle = Mathf.Min(minAngle, radialPos[child].y);
                    maxAngle = Mathf.Min(maxAngle, radialPos[child].y);
                    nChildren += 1;
                }
            }
            if (nChildren == 0) // if leaf
            {
                float angle = 2*Mathf.PI * (float)leafCounter/nLeaves;
                radialPos[root] = new Vector2(1, angle);
                leafCounter += 1;
            }
            else
            {
                float angle = (minAngle+maxAngle)/2;
                radialPos[root] = new Vector2((float)depth/maxDepth, angle);
            }
            return maxDepth;
        }
    }
}
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.IO;
using System.Collections.Generic;

public class Dendrogram : MonoBehaviour
{
    [SerializeField] TextAsset graphTxt;
    [SerializeField] Link linkPrefab;
    [SerializeField] Node nodePrefab;

    SparseVector<Node> nodes = new SparseVector<Node>();
    SparseMatrix<Link> links = new SparseMatrix<Link>();
    void Awake()
    {
        var names = new Dictionary<int, string>();
        var adjacency = new Dictionary<int, HashSet<int>>();

        // read text file
        var sr = new StringReader(graphTxt.text);
        string line;
        while ((line=sr.ReadLine()) != null)
        {
            if (line.Contains(":"))
            {
                var nodeTxt = line.Split(':');
                int idx = int.Parse(nodeTxt[0]);
                names[idx] = nodeTxt[1];
                adjacency[idx] = new HashSet<int>();
            }
            else if (line.Contains(" "))
            {
                var linkTxt = line.Split(' ');
                int src = int.Parse(linkTxt[0]);
                int tgt = int.Parse(linkTxt[1]);
                adjacency[src].Add(tgt);
                // this means all ":" lines must come before all " " lines
            }
        }
        var seen = new HashSet<int>();
        int nLeaves = CountLeaves(-1);
        print($"nLeaves = {nLeaves}");
        int CountLeaves(int root)
        {
            if (root >= 0) {
                return 1;
            }
            Assert.IsFalse(seen.Contains(root));
            seen.Add(root);
            int n = 0;
            foreach (int child in adjacency[root])
            {
                n += CountLeaves(child);
            }
            return n;
        }

        // get dendrogram positions recursively
        int leafCounter = 0;
        BuildTree(-1, 0);
        Node BuildTree(int root, int depth)
        {
            if (root >= 0)
            {
                Node leaf = InitNode(root, depth, depth, 2*Mathf.PI * (float)leafCounter/nLeaves);
                leafCounter += 1;
                return leaf;
            }
            int maxDepth = depth;
            float minAngle=2*Mathf.PI, maxAngle=0;
            var children = new List<Node>();
            foreach (int idx in adjacency[root])
            {
                Node child = BuildTree(idx, depth+1);
                maxDepth = Math.Max(maxDepth, child.MaxDepth);
                minAngle = Mathf.Min(minAngle, child.Angle);
                maxAngle = Mathf.Max(maxAngle, child.Angle);
                children.Add(child);
            }

            Node parent = InitNode(root, depth, maxDepth, (minAngle+maxAngle)/2);
            parent.Children = children;
            foreach (Node child in children) {
                child.Parent = parent;
            }
            return parent;

            // place nodes according to their radial coordinates
            Node InitNode(int idx, int treeDepth, int treeMaxDepth, float angle)
            {
                var node = Instantiate(nodePrefab);
                node.name = names[idx];
                node.Depth = treeDepth;
                node.MaxDepth = treeMaxDepth;
                node.Angle = angle;
                nodes[idx] = node;

                float radius = (float)treeDepth / treeMaxDepth;
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);
                node.Pos = 10 * radius * new Vector3(x,y,0);
                return node;
            }
        }

        // join nodes with bundled links
        foreach (int src in adjacency.Keys)
        {
            foreach (int tgt in adjacency[src])
            {
                if (src >= 0 && tgt >= 0)
                {
                    var link = Instantiate(linkPrefab);
                    link.name = $"{names[src]} -> {names[tgt]}";
                    links[src,tgt] = link;

                    link.Init(nodes[src], nodes[tgt]);
                    link.Draw(.75f);
                }
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

public class Dendrogram : MonoBehaviour
{
    [SerializeField] TextAsset graphTxt;
    [SerializeField] Link linkPrefab;
    [SerializeField] Node nodePrefab;
    [SerializeField] Slider bundlingStrength, linkWidth;
    [SerializeField] Toggle removeLCA;

    SparseVector<Node> nodes = new SparseVector<Node>();
    SparseMatrix<Link> links = new SparseMatrix<Link>();
    void Awake()
    {
        var names = new Dictionary<int, string>();
        var adjacency = new Dictionary<int, List<int>>();
        var nDependencies = new Dictionary<int, int>();

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
                adjacency[idx] = new List<int>();

                nDependencies[idx] = 0;
            }
            else if (line.Contains(" "))
            {
                var linkTxt = line.Split(' ');
                int src = int.Parse(linkTxt[0]);
                int tgt = int.Parse(linkTxt[1]);
                adjacency[src].Add(tgt); // this means all ":" lines must come before all " " lines

                nDependencies[src] += 1;
                nDependencies[tgt] += 1;
            }
        }
        int rootIdx = -1; // all
        // int rootIdx = -2; // Editor
        // int rootIdx = -1275; // External
        // int rootIdx = -1525; // Modules
        // int rootIdx = -2701; // Runtime
        // int rootIdx = -3099; // Tools

        int nLeaves = CountLeaves(rootIdx);
        print($"nLeaves = {nLeaves}");
        int CountLeaves(int root)
        {
            if (root >= 0) {
                return 1;
            }
            int n = 0;
            foreach (int child in adjacency[root])
            {
                n += CountLeaves(child);
            }
            return n;
        }

        // get dendrogram positions recursively
        int leafCounter = 0;
        var rootNode = BuildTree(rootIdx, 0);
        print($"depth = {rootNode.MaxDepth}");
        Node BuildTree(int root, int depth)
        {
            // skip and merge any folders/source files with only one subfolder/class
            while (root<0 && adjacency[root].Count == 1)
            {
                string prefix = names[root];
                root = adjacency[root][0];
                names[root] = $"{prefix}/{names[root]}";
                // print($"merged {names[root]}");
            }
            if (root >= 0)
            {
                Node leaf = InitNode(root, depth, depth, 2*Mathf.PI * (float)leafCounter/nLeaves);
                leafCounter += 1;
                return leaf;
            }
            int maxDepth = depth;
            float minAngle=2*Mathf.PI, maxAngle=0;
            var children = new List<Node>();

            // foreach (int idx in adjacency[root])
            // foreach (int idx in adjacency[root].OrderBy(x=>x))
            foreach (int idx in adjacency[root].OrderBy(x=>adjacency[x].Count))
            // foreach (int idx in adjacency[root].OrderBy(x=>UnityEngine.Random.Range(0,1f)))
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
                var node = Instantiate(nodePrefab, transform);
                node.name = names[idx];
                node.Depth = treeDepth;
                node.MaxDepth = treeMaxDepth;
                node.Angle = angle;
                nodes[idx] = node;

                float radius = Mathf.Pow((float)treeDepth / treeMaxDepth, .5f);
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);
                node.Pos = radius * new Vector3(x,y,0);
                return node;
            }
        }

        // join nodes with bundled links
        int minDependencies = 50;
        foreach (int src in adjacency.Keys)
        {
            foreach (int tgt in adjacency[src])
            {
                if (src>=0 && tgt>=0 && nodes[src]!=null && nodes[tgt]!=null &&
                    (nDependencies[src]>=minDependencies || nDependencies[tgt]>=minDependencies))
                {
                    var link = Instantiate(linkPrefab, nodes[src].transform);
                    link.name = $"{names[src]} -> {names[tgt]}";
                    links[src,tgt] = link;

                    link.FindControlPoints(nodes[src], nodes[tgt], removeLCA.isOn);
                    link.SetWidth(linkWidth.value);
                    link.Draw(bundlingStrength.normalizedValue);
                }
            }
        }
        removeLCA.onValueChanged.AddListener(ToggleLCA);
        bundlingStrength.onValueChanged.AddListener(Rebundle);
        linkWidth.onValueChanged.AddListener(ResizeLinks);
    }
    void ResizeLinks(float foo)
    {
        foreach (var link in links)
        {
            link.SetWidth(linkWidth.value);
        }
    }
    void ToggleLCA(bool foo)
    {
        foreach (var ij in links.IndexPairs)
        {
            int src = ij.Item1, tgt = ij.Item2;
            links[ij].FindControlPoints(nodes[src], nodes[tgt], removeLCA.isOn);
            links[ij].Draw(bundlingStrength.normalizedValue);
        }
    }
    void Rebundle(float foo)
    {
        foreach (var ij in links.IndexPairs)
        {
            links[ij].Draw(bundlingStrength.normalizedValue);
        }
    }
}
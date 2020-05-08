using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    [SerializeField] MeshRenderer mr;
    public int Depth { get; set; }
    public int MaxDepth { get; set; }
    public float Angle { get; set; }
    public Vector3 Pos {
        get { return transform.position; }
        set { transform.position = value; }
    }
    public Node Parent { get; set; } = null;
    public List<Node> Children { get; set; } = null;
}
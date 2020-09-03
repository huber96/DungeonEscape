using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [System.NonSerialized]
    public Vector3 worldPos;
    [System.NonSerialized]
    public Quaternion worldRot;
    [System.NonSerialized]
    public bool visited = false;

    [Space(10)]
    [Header("CONNECTED NODES")]
    public Node forwardNode;
    public Node leftNode;
    public Node rightNode;
    public Node backNode;

    Transform tr;

    [Space(10)]
    [Header("STORY")]
    public string[] synonyms = new string[] { };
    public LineData[] arrivalLines;
    public LineData[] visitedLines;

    protected void Awake()
    {
        worldPos = transform.position;
        worldRot = transform.rotation;
    }

    private void Start()
    {
        if (forwardNode == null && leftNode == null & rightNode == null && backNode == null)
        {
            Debug.LogWarning(this.gameObject.name + " node not connected to any other nodes!");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //if (Application.isPlaying) { return; }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.25f);


        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up);


        Gizmos.color = Color.cyan;
        if (forwardNode != null)
        {
            Gizmos.DrawLine(transform.position, forwardNode.transform.position);
        }
        if (rightNode != null)
        {
            Gizmos.DrawLine(transform.position, rightNode.transform.position);
        }
        if (leftNode != null)
        {
            Gizmos.DrawLine(transform.position, leftNode.transform.position);
        }
        if (backNode != null)
        {
            Gizmos.DrawLine(transform.position, backNode.transform.position);
        }
    }
#endif
}

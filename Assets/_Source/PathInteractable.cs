using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathInteractable : MonoBehaviour
{
    public float movementSpeed = 1f;
    public Transform[] pathPoints;
    int pointCount = 0;
    int targetIndex = 0;
    Vector3 targetPos;

    public bool play = false;


    private void Awake()
    {
        pointCount = pathPoints.Length;
    }

    private void Start()
    {
        targetPos = pathPoints[0].position;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    play = !play;
        //}

        if (play)
        {
            if (transform.position != targetPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * movementSpeed);
            }
            else if (++targetIndex < pointCount)
            {
                targetPos = pathPoints[targetIndex].position;
            }
            else
            {
                play = false;
            }

        }
    }
}

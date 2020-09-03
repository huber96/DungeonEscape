using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
public class OnClickMoveCamera : MonoBehaviour
{
    Transform tr;

    private void Awake()
    {
        tr = transform;
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            Destroy(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Selection.activeTransform == tr)
        {
            Camera.main.transform.position = tr.position;
            Camera.main.transform.rotation = tr.rotation;
        }
    }
}

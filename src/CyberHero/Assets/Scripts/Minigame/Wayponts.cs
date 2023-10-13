using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wayponts : MonoBehaviour
{
    public Transform[] points;

    private void Awake() {
        points = new Transform[transform.childCount];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(index: i);
        }
    }
}

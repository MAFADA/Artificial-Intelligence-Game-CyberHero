using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableScript : MonoBehaviour
{
    float[] rotations = { 0, 90, 180, 270 };

    public float[] correctRotation;
    public bool isPlaced = false;

    int PossibleRotations = 1;

    CableMinigameController controller;

    private void Awake()
    {
        controller = GameObject.Find("MinigameCableController").GetComponent<CableMinigameController>();
    }

    void Start()
    {
        PossibleRotations = correctRotation.Length;

        int rand = Random.Range(0, rotations.Length);
        transform.eulerAngles = new Vector3(0, 0, rotations[rand]);

        if (PossibleRotations > 1)
        {
            if (transform.eulerAngles.z == correctRotation[0] || transform.eulerAngles.z == correctRotation[1])
            {
                isPlaced = true;
                controller.correctMove();
            }
        }
        else
        {
            if (transform.eulerAngles.z == correctRotation[0])
            {
                isPlaced = true;
                controller.correctMove();
            }
        }

    }

    public void RotateCable()
    {
        transform.Rotate(new Vector3(0, 0, 90));

        if (PossibleRotations > 1)
        {
            if (transform.eulerAngles.z == correctRotation[0] || transform.eulerAngles.z == correctRotation[1] && isPlaced == false)
            {
                isPlaced = true;
                controller.correctMove();

            }
            else if (isPlaced == true)
            {
                isPlaced = false;
                controller.wrongMove();
            }
        }
        else
        {
            if (transform.eulerAngles.z == correctRotation[0] && isPlaced == false)
            {
                isPlaced = true;
                controller.correctMove();

            }
            else if(isPlaced == true)
            {
                isPlaced = false;
                controller.wrongMove();

            }
        }

    }

}

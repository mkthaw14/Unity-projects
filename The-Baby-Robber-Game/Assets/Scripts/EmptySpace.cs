using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptySpace : MonoBehaviour
{
    public string fuck;
    public bool isOccupied = false;

    private void Awake()
    {
        fuck = "MotherFucker";
    }

    public void TakeOverPosition(bool positionIsTaken)
    {
        if (positionIsTaken)
        {
            fuck = "FuckYou";
            isOccupied = true;
        }
    }
}

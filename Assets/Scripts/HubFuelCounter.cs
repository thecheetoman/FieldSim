using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubFuelCounter : MonoBehaviour
{
    public GameManager gameManager;
    // select alliance
    public enum Alliance { Red, Blue }

    public Alliance hubAlliance;

    public bool IsBlue()
    {
        return hubAlliance == Alliance.Blue;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GamePiece"))
        {
            gameManager.scorePoint(IsBlue());
        }
    }
}

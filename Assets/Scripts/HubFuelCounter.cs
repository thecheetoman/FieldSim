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
            if (other.TryGetComponent<GamePiece>(out GamePiece piece))
            {
                // Pass whether this is the Blue Hub AND whether the piece was shot legally
                gameManager.ScorePoint(IsBlue(), piece.WasShotFromLegalZone);

                // Destroy or recycle the game piece after scoring
                Destroy(other.gameObject);
            }
        }
    }
}
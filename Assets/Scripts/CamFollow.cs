using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            Debug.Log("I think i found the : " + player.name); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

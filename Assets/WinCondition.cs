﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCondition : MonoBehaviour {
    [SerializeField]
    private GameObject[] players;
    private PlayerStateController[] stateControllers;
    private int[] scores;
    private string roundState;
    private bool[] whosAlive;
    private int deathCounter;

    // Use this for initialization
    void Start () {
        stateControllers = new PlayerStateController[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            whosAlive[i] = true;
            stateControllers[i] = players[i].GetComponent<PlayerStateController>();
            stateControllers[i].OnPlayerDeath += DeathHandler;
        }

        // Initialize scores
        scores = new int[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            scores[i] = 0;
        }

    }
	
	// Update is called once per frame
	void Update () {

        // If there is 1 alive, player wins
        if (deathCounter == players.Length - 1)
        {
            int winner = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (whosAlive[i])
                    winner = i;
            }
            scores[winner]++;
            int playerNumber = winner + 1;
            roundState = "Player " + playerNumber.ToString() + "Wins This Round!"; //TO DO: change this to character name?
        }
        // Ties if no players are alive
        else if (deathCounter == players.Length)
        {
            roundState = "Tie!";
        }
    }
    void DeathHandler (int playerNum)
    {
        whosAlive[playerNum] = false;
        deathCounter++;
    }
}

using Dan.Main;
using LeaderboardCreatorDemo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleGame : MonoBehaviour
{
    // You can include properties that represent various aspects of your game data.
    // For example, the player's score, level, and any other relevant information.
    public LeaderboardManager leaderboardManager;
    public int Score { get; set; }

    // Add more properties as needed for your specific game.

    // You might also want to include a constructor to initialize the object.
    public ExampleGame(string _Username, int _Score)
    {
        Score = _Score;
    }

    public void Upload( int _score)
    {
        Score = _score;
        leaderboardManager.UploadEntry();
    }
}

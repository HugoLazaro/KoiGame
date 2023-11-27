using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene1 : MonoBehaviour
{
    void Update()
    {
        // Reset scene on "R" key press
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }

        // Reset scene on "Fire2" button press (adjust the button name based on your input settings)
        if (Input.GetButtonDown("Fire2"))
        {
            ResetScene();
        }
    }

    void ResetScene()
    {
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}

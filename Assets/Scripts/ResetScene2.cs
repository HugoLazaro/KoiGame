using LeaderboardCreatorDemo;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResetScene2 : MonoBehaviour
{
    public ExampleGame game;
    public ContadorScript contador;
    public TMP_InputField inputField;
    void Update()
    {
        // Reset scene on "R" key press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (inputField.text.Length > 0) { StartCoroutine(ResetScene()); }
            else
            {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(currentScene.name);
            }
        }

        // Reset scene on "Fire2" button press (adjust the button name based on your input settings)
        else if (Input.GetButtonDown("Jump"))
        {
            if (inputField.text.Length > 0) { StartCoroutine(ResetScene()); }
            else
            {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(currentScene.name);
            }
        }
    }

    private IEnumerator ResetScene()
    {
        game.Upload((int)((contador.contador) * 100));

        yield return new WaitForSeconds(1.5f);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        yield return null;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalControl : MonoBehaviour
{
    public ContadorScript contador;
    public GameObject player;
    Rigidbody2D playerRb;
    PlayerMovement playerControl;
    bool collisionado = false;
    public GameObject UiFinal;
    public GameObject time;
    public TextMeshProUGUI timeFinal;
    public GameObject reset;
    // Start is called before the first frame update
    void Start()
    {
        playerRb = player.GetComponent<Rigidbody2D>();
        playerControl = player.GetComponent<PlayerMovement>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && !collisionado)
        {
            collisionado = true;
            contador.DetenerContador();
            float result;
            float.TryParse(contador.text.text, out result);
            timeFinal.text = "Time: " + result;
            playerControl.enabled = false;
            playerRb.velocity = Vector2.zero; // Stop any ongoing movemen
            player.transform.position = new Vector3(6f, 107.5f, 0f);
            player.isStatic = true;

            //Hacer aparecer resultado, introducir nombre y leaderboard
            UiFinal.SetActive(true);
            time.SetActive(false);
            reset.SetActive(false);

        }
    }
}

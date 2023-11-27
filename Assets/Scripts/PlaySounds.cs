using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySounds : MonoBehaviour
{

    public AudioSource jump;
    public AudioSource dash;

    public void PlayJump()
    {
        jump.Play();
    }

    public void PlayDash()
    {
        dash.Play();
    }
}

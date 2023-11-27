using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimControl : MonoBehaviour
{
    public PlayerMovement player;
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.IsDashing) //DASH
        {
            anim.SetBool("Dashing", true);
            anim.SetBool("Jumping", false);
            anim.SetBool("Running", false);
            anim.SetBool("Falling", false);
            anim.SetBool("Sliding", false);
        }
        else if (player.IsSliding) //SLIDE
        {

            anim.SetBool("Sliding", true);
            anim.SetBool("Falling", false);
            anim.SetBool("Jumping", false);
            anim.SetBool("Running", false);
            anim.SetBool("Dashing", false);
        }
        else if(player.IsJumping) //JUMP
        {
            anim.SetBool("Jumping", true);
            anim.SetBool("Running", false);
            anim.SetBool("Falling", false);
            anim.SetBool("Dashing", false);
            anim.SetBool("Sliding", false);
        }
        else if (player.RB.velocity.y < -0.1f) //FALL
        {
            anim.SetBool("Falling", true);
            anim.SetBool("Jumping", false);
            anim.SetBool("Running", false);
            anim.SetBool("Dashing", false);
            anim.SetBool("Sliding", false);
        }
        else if(player.RB.velocity.x > 0.1f || player.RB.velocity.x < -0.1f) //RUN
        {
            anim.SetBool("Running", true);
            anim.SetBool("Jumping", false);
            anim.SetBool("Falling", false);
            anim.SetBool("Dashing", false);
            anim.SetBool("Sliding", false);
        }
        else 
        {
            anim.SetBool("Falling", false);
            anim.SetBool("Jumping", false); 
            anim.SetBool("Running", false);
            anim.SetBool("Dashing", false);
            anim.SetBool("Sliding", false);
        }

    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")] //Create a new playerData object by right clicking in the Project Menu then Create/Player/Player Data and drag onto the player
public class PlayerData : ScriptableObject
{
	[Header("Gravity")]
	[HideInInspector] public float gravityStrength; //Downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex.
    [HideInInspector] public float gravityScale; //Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D).
										  //Also the value the player's rigidbody2D.gravityScale is set to.
	[Space(5)]
	public float fallGravityMult; //Multiplier to the player's gravityScale when falling.
	public float maxFallSpeed; //Maximum fall speed (terminal velocity) of the player when falling.
	[Space(5)]
	public float fastFallGravityMult; //Larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed.
									  //Seen in games such as Celeste, lets the player fall extra fast if they wish.
	public float maxFastFallSpeed; //Maximum fall speed(terminal velocity) of the player when performing a faster fall.
	
	[Space(20)]

	[Header("Run")]
	public float runMaxSpeed; //Target speed we want the player to reach.
	public float runAcceleration; //The speed at which our player accelerates to max speed, can be set to runMaxSpeed for instant acceleration down to 0 for none at all
    [HideInInspector] public float runAccelAmount; //The actual force (multiplied with speedDiff) applied to the player.
	public float runDecceleration; //The speed at which our player decelerates from their current speed, can be set to runMaxSpeed for instant deceleration down to 0 for none at all
    [HideInInspector] public float runDeccelAmount; //Actual force (multiplied with speedDiff) applied to the player .
	[Space(5)]
	[Range(0f, 1)] public float accelInAir; //Multipliers applied to acceleration rate when airborne.
	[Range(0f, 1)] public float deccelInAir;
	[Space(5)]
	public bool doConserveMomentum = true;

	[Space(20)]

	[Header("Jump")]
	public int numJumps;
	public float jumpHeight; //Altura del salto
	public float jumpTimeToApex; //Tiempo hasta tope
    [HideInInspector] public float jumpForce; //The actual force applied (upwards) to the player when they jump.

	[Header("Both Jumps")]
	public float cutJumpSpeed; //Velocidad al finaliazr salto
	public float jumpCutGravityMult; //Bajar al jugador si corta el salto
	[Range(0f, 1)] public float jumpHangGravityMult; //Reduce gravedad en cima
	public float jumpHangTimeThreshold; //Velocidad a la que el player experimenta el jumo hang
	[Space(0.5f)]
	public float jumpHangAccelerationMult; //aceleracion jump hang
    public float jumpHangMaxSpeedMult; //Deceleracion jump hang			

	[Header("Wall Jump")]
	public Vector2 wallJumpForce; //Fuerza del salto
	[Space(5)]
	[Range(0f, 1f)] public float wallJumpRunLerp; //Reduce el movimiento del jugador durante el salto.
	[Range(0f, 1.5f)] public float wallJumpTime; //Tiempo que pasa hasta reducir el movimiento del jugador.
	public bool doTurnOnWallJump; //Rotar on wall jump

	[Space(20)]

	[Header("Slide")]
	public float slideSpeed; //Slide max velocidad
	public float slideAccel; //Slice aceleracion
    [Range(0.01f, 0.5f)] public float slideTimeBegin; //Slice timeout

    [Header("Assists")]
	[Range(0.01f, 0.5f)] public float coyoteTime;
	[Range(0.01f, 0.5f)] public float wallCoyoteTime;
    [Range(0.01f, 0.5f)] public float LastDirectionCoyote;
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;
    
    [Space(20)]

	[Header("Dash")]
	public int dashAmount;
	public float dashSpeed;
	public float dashSleepTime; //Duration for which the game freezes when we press dash but before we read directional input and apply a force
	public bool dashStopTime;
    [Space(5)]
	public float dashAttackTime;
	[Space(5)]
	public float dashEndTime; //Time after you finish the inital drag phase, smoothing the transition back to idle (or any standard state)
	public Vector2 dashEndSpeed; //Slows down player, makes dash feel more responsive (used in Celeste)
	[Range(0f, 1f)] public float dashEndRunLerp; //Slows the affect of player movement while dashing
	[Space(5)]
	public float dashRefillTime;
	[Space(5)]
	[Range(0.01f, 0.5f)] public float dashInputBufferTime;
	

	//Unity Callback, called when the inspector updates
    private void OnValidate()
    {
		//Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
		gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
		
		//Calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics2D)
		gravityScale = gravityStrength / Physics2D.gravity.y;

		//Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
		runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
		runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

		//Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
		jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

		#region Variable Ranges
		runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
		runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
		#endregion
	}
}
/*
	Created by @DawnosaurDev at youtube.com/c/DawnosaurStudios
	Thanks so much for checking this out and I hope you find it helpful! 
	If you have any further queries, questions or feedback feel free to reach out on my twitter or leave a comment on youtube :D

	Feel free to use this in your own games, and I'd love to see anything you make!
 */

using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	//Scriptable object which holds all the player's movement parameters. If you don't want to use it
	//just paste in all the parameters, though you will need to manuly change all references in this script
	public PlayerData Data;
	public PlaySounds soundControl;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
	//Script to handle all player animations, all references can be safely removed if you're importing into your own project.
	
	#endregion

	#region STATE PARAMETERS
	//Variables control the various actions the player can perform at any time.
	//These are fields which can are public allowing for other sctipts to read them
	//but can only be privately written to.
	public bool IsFacingRight { get; private set; }
	public bool IsJumping { get; private set; }
	public int numJumps { get; private set; }
    public bool IsWallJumping { get; private set; }
	public bool IsDashing { get; private set; }
	public bool IsSliding { get; private set; }

	//Timers (also all fields, could be private and a method returning a bool could be used)
	public float LastOnGroundTime { get; private set; }
	public float LastOnWallTime { get; private set; }
	public float LastOnWallRightTime { get; private set; }
	public float LastOnWallLeftTime { get; private set; }

	//Jump
	private bool _isJumpCut;
	private bool _isJumpFalling;


    //Wall Jump
    private float _wallJumpStartTime;
	private int _lastWallJumpDir;

	//Slide
	private float _timePreSlide;

	//Dash
	private int _dashesLeft;
	private bool _dashRefilling;
	private Vector2 _lastDashDir;
	private bool _isDashAttacking;

	#endregion

	#region INPUT PARAMETERS
	private Vector2 _moveInput;

	public float LastPressedJumpTime { get; private set; }
	public float LastPressedDirection { get; private set; }
    public float LastPressedDashTime { get; private set; }
	#endregion

	#region CHECK PARAMETERS
	//Set all of these up in the inspector
	[Header("Checks")] 
	[SerializeField] private Transform _groundCheckPoint;
	//Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
	[Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);

    //CHeck platform upwards
    [SerializeField] private Transform _upFrontCheckPoint;
    [SerializeField] private Transform _upBackCheckPoint;
    [SerializeField] private Transform _upMidCheckPoint;
    [SerializeField] private Vector2 _upSideCheckSize = new Vector2(0.2f, 0.5f);
    [SerializeField] private Vector2 _upMidCheckSize = new Vector2(0.5f, 0.5f);

    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
    private bool dashjoystick;
    #endregion

    private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
	}

	private void Start()
	{
		SetGravityScale(Data.gravityScale);
		IsFacingRight = true;
	}

	private void Update()
	{
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
		LastOnWallTime -= Time.deltaTime;
		LastOnWallRightTime -= Time.deltaTime;
		LastOnWallLeftTime -= Time.deltaTime;

		_timePreSlide -=Time.deltaTime;

		LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
		if(LastPressedDirection > 0.1)
		{
			LastPressedDirection -= Time.deltaTime;
		} else if(LastPressedDirection < 0.1)
		{
			LastPressedDirection += Time.deltaTime;
		}
		#endregion

		#region INPUT HANDLER
		_moveInput.x = Input.GetAxisRaw("Horizontal");
		_moveInput.y = Input.GetAxisRaw("Vertical");

		if (_moveInput.x != 0)
		{
			CheckDirectionToFace(_moveInput.x > 0);
			if(_moveInput.x > 0) { LastPressedDirection = Data.LastDirectionCoyote; }
			else { LastPressedDirection = -Data.LastDirectionCoyote; }
        }

		if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J) || Input.GetButtonDown("Jump"))
        {
			OnJumpInput();
        }

		if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J) || Input.GetButtonUp("Jump"))
		{
			OnJumpUpInput();
		}

		if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K) || Input.GetMouseButtonDown(0))
		{
            dashjoystick = false;
            OnDashInput();
		}
		else if (Input.GetButtonDown("Fire1"))
		{
			dashjoystick = true;
			OnDashInput();
		}
		#endregion

		#region COLLISION CHECKS
		if (!IsDashing && !IsJumping)
		{
			//Ground Check
			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer)) //checks if set box overlaps with ground
			{
				LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
            }		

			//Right Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
					|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
				LastOnWallRightTime = Data.wallCoyoteTime;

			//Right Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
				LastOnWallLeftTime = Data.wallCoyoteTime;

            //Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
		}
		#endregion

		#region JUMP CHECKS
		if (IsJumping && RB.velocity.y < 0)
		{
			IsJumping = false;

			_isJumpFalling = true;
		}

		if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
		{
			IsWallJumping = false;
		}

		if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
			_isJumpCut = false;

			_isJumpFalling = false;
		}

		if (!IsDashing)
		{
			//Jump
			if (CanJump() && LastPressedJumpTime > 0)
			{
				IsJumping = true;
				numJumps--;
				IsWallJumping = false;
				_isJumpCut = false;
				_isJumpFalling = false;
				Jump();

			}
			//WALL JUMP
			else if (CanWallJump() && LastPressedJumpTime > 0)
			{
				/*
				//Hacer un wallJump
				if(LastPressedDirection > 0.1) //Va a la derecha
				{
					if(LastOnWallLeftTime < LastOnWallRightTime) //Esta en la izquierda
					{
                        Debug.Log("Salto Pared(pulso der)");
                    } else { Debug.Log("Salto Contrario(pulso der)"); }
					
				}
                else if (LastPressedDirection < -0.1) //Va a la izquierda
                {
                    if (LastOnWallLeftTime < LastOnWallRightTime) //Esta en la izquierda
                    {
                        Debug.Log("Salto Contrario(pulso izq)");
                    }
                    else { Debug.Log("Salto Pared(pulso izq)"); }
                } else { Debug.Log("Salto contrario por falta de info"); 
				*/
                IsWallJumping = true;
				IsJumping = false;
				_isJumpCut = false;
				_isJumpFalling = false;

				_wallJumpStartTime = Time.time;
				_lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;
                WallJump(_lastWallJumpDir);
			}
		}
        #endregion

        #region DASH CHECKS
        if (CanDash() && LastPressedDashTime > 0)
		{
			//Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
			IsDashing = true;
            StartCoroutine(nameof(PreDash), Data.dashSleepTime); 
		}
		#endregion

		#region SLIDE CHECKS
		if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
		{ IsSliding = true; }
		else { IsSliding = false; _timePreSlide = Data.slideTimeBegin; }
		#endregion

		#region GRAVITY
		if (!_isDashAttacking)
		{
			//Higher gravity if we've released the jump input or are falling
			if (IsSliding)
			{
				SetGravityScale(0);
			}
			else if (RB.velocity.y < 0 && _moveInput.y < 0)
			{
				//Much higher gravity if holding down
				SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
			}
			else if (_isJumpCut)
			{
				//Higher gravity if jump button released
				SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
			else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
			{
				SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
			}
			else if (RB.velocity.y < 0)
			{
				//Higher gravity if falling
				SetGravityScale(Data.gravityScale * Data.fallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
			}
			else
			{
				//Default gravity if standing on a platform or moving upwards
				SetGravityScale(Data.gravityScale);
			}
		}
		else
		{
			//No gravity when dashing (returns to normal once initial dashAttack phase over)
			SetGravityScale(0);
		}
		#endregion
    }

    private void FixedUpdate()
	{
		//Handle Run
		if (!IsDashing)
		{
			if (IsWallJumping)
				Run(Data.wallJumpRunLerp);
			else
				Run(1);
		}
		else if (_isDashAttacking)
		{
			Run(Data.dashEndRunLerp);
		}

		//Handle Slide
		if (IsSliding)
			Slide();
    }

    #region INPUT CALLBACKS
	//Methods which whandle input detected in Update()
    public void OnJumpInput()
	{
		LastPressedJumpTime = Data.jumpInputBufferTime;
	}

	public void OnJumpUpInput()
	{
		if (CanJumpCut() || CanWallJumpCut())
		{ _isJumpCut = true; RB.velocity = new Vector2(RB.velocity.x, Mathf.Min(Data.cutJumpSpeed, RB.velocity.y)); }
	}

	public void OnDashInput()
	{
		LastPressedDashTime = Data.dashInputBufferTime;
	}
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}

	private IEnumerator PreDash(float duration)
	{
		if (Data.dashStopTime) { Time.timeScale = 0; }
		yield return new WaitForSecondsRealtime(duration); //Must be Realtime since timeScale with be 0 
        if (Data.dashStopTime) { Time.timeScale = 1; }


        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 directionToMouse = (mousePosition - transform.position).normalized;

		//Check if joystick
		if (dashjoystick)
		{
            Vector2 joystickDirection = GetJoystickDirection();
            if (joystickDirection != Vector2.zero)
                _lastDashDir = joystickDirection.normalized;
            else
			{
				if (IsFacingRight)
				{
					joystickDirection = new Vector2(1f, 0);
				} else
				{
                    joystickDirection = new Vector2(-1f, 0);
                }
            }


            //Direccion del objeto al raton
            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
			_isJumpCut = false;

            StartCoroutine(nameof(StartDash), joystickDirection);
        } else
		{

            //Direccion del objeto al raton
            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;

            StartCoroutine(nameof(StartDash), directionToMouse);
        }
    }

    // Function to get normalized joystick direction
    private Vector2 GetJoystickDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical).normalized;
    }
    #endregion

    //MOVEMENT METHODS
    #region RUN METHODS
    private void Run(float lerpAmount)
	{
		//Calculate the direction we want to move in and our desired velocity
		float targetSpeed = _moveInput.x * Data.runMaxSpeed;
		//We can reduce are control using Lerp() this smooths changes to are direction and speed
		targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

		#region Calculate AccelRate
		float accelRate;

		//Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
		#endregion

		#region Add Bonus Jump Apex Acceleration
		//Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
		{
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;
		}
		#endregion

		#region Conserve Momentum
		//We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
		if(Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			//Prevent any deceleration from happening, or in other words conserve are current momentum
			//You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
			accelRate = 0; 
		}
		#endregion

		//Calculate difference between current velocity and desired velocity
		float speedDif = targetSpeed - RB.velocity.x;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;

		//Convert this to a vector and apply to rigidbody
		RB.AddForce(movement * Vector2.right, ForceMode2D.Force);

		/*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
	}

	private void Turn()
	{
		//stores scale and flips the player along the x axis, 
		Vector3 scale = transform.localScale; 
		scale.x *= -1;
		transform.localScale = scale;
		
		IsFacingRight = !IsFacingRight;
	}
    #endregion

    #region JUMP METHODS
    private void Jump()
	{
		//SOUND + ANIMATION
		soundControl.PlayJump();
		
		//Ensures we can't call Jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;

		#region Perform Jump
		//We increase the force applied if we are falling
		//This means we'll always feel like we jump the same amount 
		//(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
		float force = Data.jumpForce;
		//if (RB.velocity.y < 0)
		//	force -= RB.velocity.y;
		force = Mathf.Min(force, 16);
		RB.velocity = new Vector2(RB.velocity.x, 0);
		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		#endregion
	}

	private void WallJump(int dir)
	{

        //Ensures we can't call Wall Jump multiple times from one press
        LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		LastOnWallRightTime = 0;
		LastOnWallLeftTime = 0;

		#region Perform Wall Jump
		Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
		force.x *= dir; //apply force in opposite direction of wall

		if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
			force.x -= RB.velocity.x;

		if (RB.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
			force.y -= RB.velocity.y;

		//Unlike in the run we want to use the Impulse mode.
		//The default mode will apply are force instantly ignoring masss
		RB.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}
	#endregion

	#region DASH METHODS
	//Dash Coroutine
	private IEnumerator StartDash(Vector2 dir)
	{
		//Overall this method of dashing aims to mimic Celeste, if you're looking for
		// a more physics-based approach try a method similar to that used in the jump

		LastOnGroundTime = 0;
		LastPressedDashTime = 0;

		float startTime = Time.time;

		_dashesLeft--;
		_isDashAttacking = true;

		SetGravityScale(0);

		//Set rotation
		CheckDirectionToFace(dir.x > 0);


        RB.constraints = RigidbodyConstraints2D.None;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		if(dir.x < 0) { angle = angle - 180; }
		// Rotate the object to face the mouse
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		//SET ANIMATION + SOUND
		soundControl.PlayDash();

        //We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
        while (Time.time - startTime <= Data.dashAttackTime && IsDashing)
		{
			RB.velocity = dir.normalized * Data.dashSpeed;
			//Pauses the loop until the next frame, creating something of a Update loop. 
			//This is a cleaner implementation opposed to multiple timers and this coroutine approach is actually what is used in Celeste :D
			yield return null;

			//Reset the jumps 
			if (Physics2D.OverlapBox(_upMidCheckPoint.position, _upMidCheckSize, 0, _groundLayer) && LastOnGroundTime < 0){
				numJumps = 1;
			}
		}

        //Set rotation back
        RB.constraints = RigidbodyConstraints2D.FreezeRotation;
		transform.rotation = Quaternion.identity;

        startTime = Time.time;

		_isDashAttacking = false;

		//Begins the "end" of our dash where we return some control to the player but still limit run acceleration (see Update() and Run())
		SetGravityScale(Data.gravityScale);
		RB.velocity = Data.dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= Data.dashEndTime)
		{
			yield return null;
		}

        //Dash over
        IsDashing = false;

        yield break;
	}

	//Short period before the player is able to dash again
	private IEnumerator RefillDash(int amount)
	{
		//SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
		_dashRefilling = true;
		yield return new WaitForSeconds(Data.dashRefillTime);
		_dashRefilling = false;
		_dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
	}

  //  private void OnCollisionEnter2D(Collision2D collision)
  //  {
  //      if(IsDashing)
		//{
		//	IsDashing = false;
		//	Debug.Log("Stop dash");
		//}
  //  }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void Slide()
	{
		//We remove the remaining upwards Impulse to prevent upwards sliding
		if(RB.velocity.y > 0)
		{
		    RB.AddForce(-RB.velocity.y * Vector2.up,ForceMode2D.Impulse);
		}
	
		//Works the same as the Run but only in the y-axis
		//THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
		float speedDif = Data.slideSpeed - RB.velocity.y;
		//Tiempo antes de deslizar
		if(_timePreSlide > 0)
		{
			RB.velocity = new Vector2(RB.velocity.x, 0);
		} else
		{
			//So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
			//The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
			float movement = speedDif * Data.slideAccel;
			movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif)  * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));
			RB.AddForce(movement * Vector2.up);
		}

	}
    #endregion


    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight)
		{
			Turn();
		}
	}

    private bool CanJump()
    {

		//Si esta tocando el suelo
        if (LastOnGroundTime > 0 || Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping) //checks if set box overlaps with ground
        {
			//Debug.Log("1");
            numJumps = Data.numJumps;
            return true;
        }
		
		//Esta en la pared, no en el suelo
        if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
		{
			//Debug.Log("2");
            numJumps = 1;
            return false;
        }

        //Esta en la pared, no en el suelo
        if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
            || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping )
		{
			//Debug.Log("2");
            numJumps = 1;
            return false;
        }

		//Esta cayendo
		if(RB.velocity.y < 0 && !_isJumpFalling) { 
			//Debug.Log("3");
            numJumps = 1;
            return true; 
		}

		//Viene de un salto, ni pared, ni suelo
        if (numJumps > 0)
		{
			//Debug.Log("4");
			return true;
        } else { return false; }
    }

	private bool CanWallJump()
    {
		if(LastOnWallTime > 0 && !(LastOnGroundTime > 0) )
		{
			if (!IsWallJumping) { return true; }
		} return false;
	}

	private bool CanJumpCut()
    {
		return IsJumping && RB.velocity.y > 0;
    }

	private bool CanWallJumpCut()
	{
		return IsWallJumping && RB.velocity.y > 0;
	}

	private bool CanDash()
	{
        if (IsDashing) { return false; }

        if (!IsDashing && _dashesLeft < Data.dashAmount && (LastOnGroundTime > 0 || LastOnWallTime > 0) && !_dashRefilling)
		{
			StartCoroutine(nameof(RefillDash), 1);
		}

		return _dashesLeft > 0;
	}

	public bool CanSlide()
    {
		if ((Physics2D.OverlapBox(transform.position,new Vector2(1.2f, 1.2f), 0, _groundLayer) && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0))
			return true;
		else
			return false;
	}
	#endregion



	#region EDITOR METHODS
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
		Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);

        //Check is platform upwards
        Gizmos.DrawWireCube(_upFrontCheckPoint.position, _upSideCheckSize);
        Gizmos.DrawWireCube(_upBackCheckPoint.position, _upSideCheckSize);
        Gizmos.DrawWireCube(_upMidCheckPoint.position, _upMidCheckSize);

    }
    #endregion
}
using System.Collections;
using System.Collections.Generic;
using log4net;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    private static readonly ILog Log = LogManager.GetLogger(type: typeof(PlayerController));

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    #region movement variables

    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float distanceWallDetect = 0.05f;
    [SerializeField] private float offsetYAxisWallDetect = 0.5f;
    private Vector2 direction;

    #endregion

    #region jump variables
    [Header("Jump")]
    [SerializeField] private float gravity = -14f;
    [SerializeField] private float startJumpPower = 5.6f;
    [SerializeField] private float holdJumpPowerPerFrame = 17f;
    [SerializeField] private float distanceGroundDetect = 0.2f; 
    [SerializeField] private float offsetXAxisGroundDetect = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    private bool isJumping;
    #endregion

    #region behaviour events

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        float horizontal = direction.x * speed;
        float vertical = gravity * Time.deltaTime * 0.5f;

        if (isJumping)
        {
            vertical += holdJumpPowerPerFrame * Time.deltaTime;
        }

        if (OnLeftWall())
        {
            horizontal = Mathf.Max(horizontal, 0f);
        }

        if (OnRightWall())
        {
            horizontal = Mathf.Min(horizontal, 0f);
        }

        Vector2 moveVector = new Vector2(horizontal, rb.velocity.y + vertical);
        rb.velocity = moveVector;
    }

    #endregion

    #region inputs

    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();
        if (inputMovement.x > 0) sr.flipX = false;
        if (inputMovement.x < 0) sr.flipX = true;
        direction = new Vector2(inputMovement.x, 0);
    }

    public void OnJump(InputAction.CallbackContext value)
    {
        if (value.started && CanJump())
        {
            Log.Debug("Start jumping: " + startJumpPower);
            rb.velocity = new Vector2(rb.velocity.x, startJumpPower);
            isJumping = true;
        }

        if (value.canceled || value.performed)
        {
            if (value.performed) Log.Debug("Jumped with full power");
            //rb.velocity = new Vector2(rb.velocity.x, 0f);
            isJumping = false;
        }
    }

    #endregion

    #region Jumps behavior

    private float GetJumpVelocity()
    {
        float jumpHeight = 6f;
        float timeToJumpHieght = 3f;
        float jumpGravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpHieght, 2);
        float jumpVelocity = Mathf.Abs(jumpGravity) * timeToJumpHieght;
        return jumpVelocity;
    }

    private bool CanJump()
    {
        bool onGround = Physics2D.Raycast(LeftStartPointToDetectGround(), Vector2.down, distanceGroundDetect, groundLayer) || Physics2D.Raycast(RightStartPointToDetectGround(), Vector2.down, distanceGroundDetect, groundLayer);
        return onGround;
    }

    // Center of starter point to detect ground in X Axis
    private Vector3 StartPointToDetectGroundOrigin()
    {
        return transform.position + Vector3.down * transform.localScale.y / 2;
    }

    private Vector3 LeftStartPointToDetectGround()
    {
        return StartPointToDetectGroundOrigin() + Vector3.left * offsetXAxisGroundDetect;
    }

    private Vector3 RightStartPointToDetectGround()
    {
        return StartPointToDetectGroundOrigin() + Vector3.right * offsetXAxisGroundDetect;
    }

    #endregion

    #region Walls behavior

    private bool OnWall()
    {
        return OnLeftWall() || OnRightWall();
    }

    private bool OnLeftWall()
    {
        return Physics2D.Raycast(BottomLeftStartPointToDetectWall(), Vector2.left, distanceWallDetect, groundLayer) || Physics2D.Raycast(TopLeftStartPointToDetectWall(), Vector2.left, distanceWallDetect, groundLayer);
    }

    private bool OnRightWall()
    {
        return Physics2D.Raycast(BottomRightStartPointToDetectWall(), Vector2.right, distanceWallDetect, groundLayer) || Physics2D.Raycast(TopRightStartPointToDetectWall(), Vector2.right, distanceWallDetect, groundLayer);
    }

    private Vector3 StartLeftPointToDetectWallOrigin()
    {
        return transform.position + Vector3.left * transform.localScale.x / 2;
    }

    private Vector3 StartRightPointToDetectWallOrigin()
    {
        return transform.position + Vector3.right * transform.localScale.x / 2;
    }

    private Vector3 TopLeftStartPointToDetectWall()
    {
        return StartLeftPointToDetectWallOrigin() + Vector3.up * offsetYAxisWallDetect;
    }

    private Vector3 BottomLeftStartPointToDetectWall()
    {
        return StartLeftPointToDetectWallOrigin() + Vector3.down * offsetYAxisWallDetect;
    }

    private Vector3 TopRightStartPointToDetectWall()
    {
        return StartRightPointToDetectWallOrigin() + Vector3.up * offsetYAxisWallDetect;
    }

    private Vector3 BottomRightStartPointToDetectWall()
    {
        return StartRightPointToDetectWallOrigin() + Vector3.down * offsetYAxisWallDetect;
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // Ground Detect
        Gizmos.color = Color.green;
        Gizmos.DrawLine(LeftStartPointToDetectGround(), LeftStartPointToDetectGround() + Vector3.down * distanceGroundDetect);
        Gizmos.DrawLine(RightStartPointToDetectGround(), RightStartPointToDetectGround() + Vector3.down * distanceGroundDetect);

        // Left Wall Detect
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(TopLeftStartPointToDetectWall(), TopLeftStartPointToDetectWall() + Vector3.left * distanceWallDetect);
        Gizmos.DrawLine(BottomLeftStartPointToDetectWall(), BottomLeftStartPointToDetectWall() + Vector3.left * distanceWallDetect);

        // Right Wall Detect
        Gizmos.DrawLine(TopRightStartPointToDetectWall(), TopRightStartPointToDetectWall() + Vector3.right * distanceWallDetect);
        Gizmos.DrawLine(BottomRightStartPointToDetectWall(), BottomRightStartPointToDetectWall() + Vector3.right * distanceWallDetect);
    }

}

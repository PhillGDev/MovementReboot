using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Singleton;
    public Transform Head, Camera, GroundCheck;
    public LayerMask GroundMask;
    public float HeadRotationSpeed, CheckSize;
    public float BodyMovementSpeed;
    public float JumpHeight;
    [Range(.1f, 1f)]
    public float AirMoveFraction;
    Vector2 Rotation;
    Vector2 inputRotation;
    Vector2 Movement;
    public Rigidbody Body;

    public void SetCursorState(CursorLockMode mode)
    {
        Debug.Log("Cursor mode now set to " + mode.ToString());
        Cursor.lockState = mode;
    }
    private void Start()
    {
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        GetInput();
        MoveCamera();
    }
    private void Awake()
    {
        Singleton = this;
        SetBodyState(true);
    }
    private void FixedUpdate()
    {
        ApplyForces();
    }
    #region Helper Functions
    bool BodyState;
    public void SetBodyState(bool state)
    {
        if (state != BodyState)
        {
            BodyState = state;
        }
    }
    void ApplyForces()
    {
        if (GetGrounded()) GroundMove();
        else AirMove();
    }
    void GroundMove()
    {
        Vector3 InputDir = (transform.forward * Movement.y + transform.right * Movement.x).normalized;
        //This still somehow results in a slow stop.
        Vector3 Force = (InputDir * (Input.GetKey(KeyCode.LeftShift) ? BodyMovementSpeed * 2f : BodyMovementSpeed)) * Body.mass + (Body.velocity * Body.mass * -1f);
        Debug.DrawLine(transform.position, transform.position + Force, Color.green);
        Body.AddForce(Force, ForceMode.Force); //How to only account for velocity that is not our intended direction?
    }
    void AirMove()
    {
        Vector3 InputDir = (transform.forward * Movement.y + transform.right * Movement.x).normalized;
        //This still somehow results in a slow stop.
        Vector3 Force = ((InputDir * (Input.GetKey(KeyCode.LeftShift) ? BodyMovementSpeed * 2f : BodyMovementSpeed)) * Body.mass) * AirMoveFraction; //No dampening, we're in the god damm sky.
        Body.AddForce(Force, ForceMode.Force); //How to only account for velocity that is not our intended direction?
        Debug.DrawLine(transform.position, transform.position + Force, Color.green);
    }
    void Jump()
    {
        Vector3 Force = JumpHeight * Body.mass * transform.up;
        Body.AddForce(Force, ForceMode.Impulse);
        Debug.DrawLine(transform.position, transform.position + Force, Color.blue, 15f);
    }
    void GetInput()
    {
        if (!BodyState)
        {
            inputRotation = Vector2.zero;
            Movement = Vector2.zero;
        }
        else
        {
            inputRotation = new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
            Movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (Input.GetKeyDown(KeyCode.Space) && GetGrounded()) Jump();
        }
    }
    public void SetCameraHold(Transform pos)
    {
        Camera.SetParent(pos, false);
    }
    void MoveCamera()
    {
        if (BodyState)
        {
            Rotation += HeadRotationSpeed * Time.deltaTime * inputRotation;
            inputRotation = Vector2.zero;
            Rotation = new Vector2(Mathf.Clamp(Rotation.x, -90f, 90f), Rotation.y);
            Head.localRotation = Quaternion.Euler(new Vector3(Rotation.x, 0f, 0f)); //Be local, so it works when we are rotated.
            transform.localRotation = Quaternion.Euler(new Vector3(0f, Rotation.y, 0f));
            Camera.SetPositionAndRotation(Head.position, Head.rotation);
        }
    }
    bool GetGrounded()
    {
        return Physics.CheckSphere(GroundCheck.position, CheckSize, GroundMask);
    }
    #endregion
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        if (GroundCheck != null) Gizmos.DrawWireSphere(GroundCheck.position, CheckSize);
        Gizmos.color = Color.green;
    }
}

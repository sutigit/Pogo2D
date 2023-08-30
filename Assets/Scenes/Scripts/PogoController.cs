using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PogoController : MonoBehaviour
{
    [Header("Pogo Settings")]
    public float maxJump = 3.0f;
    public float maxTiltAngle = 30.0f;
    public float smoothenRotation = 0.5f;
    public float maxSpeed = 5.0f;
    public float springStiffness = 10f;


    private Rigidbody2D rb2D;
    private float _raycastLength = 0.7f;
    private Vector3 _springOrigin = Vector3.zero;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void Update()
    {
        // this reloads the scene
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Base");
        }

        // tilt control
        if (Input.GetKey("a") || Input.GetKey("left"))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, maxTiltAngle), smoothenRotation);
        }
        else if (Input.GetKey("d") || Input.GetKey("right"))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, -maxTiltAngle), smoothenRotation);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, 0), smoothenRotation);
        }
    }

    void FixedUpdate()
    {
        // get the raycast hit
        Vector3 position = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(position, -transform.up, _raycastLength);
        if (!hit) return;


        // get pogos relative angle
        float angle = transform.rotation.eulerAngles.z;
        angle = clamp(shiftAngle(angle, 90), 0, 180) * Mathf.Deg2Rad;

        // calculate force depending on distance from hit object;
        float force = 0;
        force = (_raycastLength - hit.distance) * springStiffness;

        // add force to unit circle components
        float x = Mathf.Cos(angle) * force;
        float y = Mathf.Sin(angle) * force;


        // Limit speed and jump height
        // get current x and y velocity
        float xVel = rb2D.velocity.x;
        float yVel = rb2D.velocity.y;
        
        if (Mathf.Abs(xVel) > maxSpeed)
        {
            x = 0;
        }

        // check if we are going too high
        if (Mathf.Abs(yVel) > maxJump)
        {
            y = 0;
        }


        // check if we are breaking
        // and add breaking force addording to angle
        float angleDeg = angle * Mathf.Rad2Deg;
        float breakForce = Mathf.Abs(90 - angleDeg) / 20f;

        // Debug.Log("breakForce" + breakForce);

        if (angleDeg > 90 && xVel > 0)
        {
            x = -breakForce;
        }
        else if (angleDeg < 90 && xVel < 0)
        {
            x = breakForce;
        }

        // apply force
        rb2D.AddForce(new Vector2(x, y) * 100);

        #if UNITY_EDITOR
            Debug.DrawRay(transform.position, Vector2.down * _raycastLength, Color.green);
        #endif
    }

    // my utility functions
    private float shiftAngle(float input, float shift)
    {
        float newVal = input + shift;

        if (newVal > 360f)
        {
            return newVal - 360f;
        }
        else
        {
            return newVal;
        }
    }

    private float clamp(float input, float start, float end)
    {
        if (input > start && input < end)
        {
            return input;
        }
        else
        {
            return 0f;
        }
    }
}

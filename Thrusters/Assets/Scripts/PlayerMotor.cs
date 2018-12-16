using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;
    private Vector3 thrusterForce = Vector3.zero;

    private float slowCoefficient = 0f;


    [SerializeField]
    private float cameraRotationLimit = 85f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    public void SetSlow(float slow, float duration)
    {
        StartCoroutine(PerformSlow(slow, duration));
    }

    IEnumerator PerformSlow(float slow, float duration)
    {
        slowCoefficient = slow;
        yield return new WaitForSeconds(duration + 0.1f);
        slowCoefficient = 0f;
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime * (1 - slowCoefficient));
        }

        if (thrusterForce != Vector3.zero)
        {
            rb.AddForce(thrusterForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }


    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    public void RotateCamera(float _cameraRotation)
    {
        cameraRotationX = Mathf.Lerp(cameraRotationX, _cameraRotation, 0.5f);
        //cameraRotationX = _cameraRotation;
    }

    public void ApplyThruster(Vector3 _thrusterForce)
    {
        thrusterForce = Vector3.Lerp(thrusterForce, _thrusterForce, 0.5f);
        //thrusterForce = _thrusterForce;
    }

    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        //rb.MoveRotation(Quaternion.Slerp(rb.rotation, rb.rotation * Quaternion.Euler(rotation), 0.5f));
        if (cam != null)
        {

            currentCameraRotationX -= cameraRotationX;
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);


            cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);
            //cam.transform.Rotate(-cameraRotationX);
        }

    }
}

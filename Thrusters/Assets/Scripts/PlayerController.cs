using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float lookSensetivity = 3f;


    [Header("Thruster setting:")]

    [SerializeField]
    private float thrusterForce = 1000f;

    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;
    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;
    private float thrusterFuelAmount = 1f;

    public float GetThrusterFuelAmount()
    {
        return thrusterFuelAmount;
    }


    [SerializeField] private LayerMask environmentMask;

    [Header("Spring setting:")]

    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;

    private Animator animator;
    private PlayerMotor motor;
    private ConfigurableJoint joint;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();


        SetJointSetting(jointSpring);
    }


    void Update()
    {
        if (PauseMenu.isOn)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            motor.Move(Vector3.zero);
            motor.Rotate(Vector3.zero);
            motor.RotateCamera(0f);
            motor.ApplyThruster(Vector3.zero);
            return;
        }

        float yRot = Input.GetAxis("Mouse X");

        Vector3 rotation = new Vector3(0f, yRot, 0f) * (SettingManager.GameSettinngs.mouseSensitivity * 4000 + 1f) * Time.deltaTime;



        motor.Rotate(rotation);


        float xRot = Input.GetAxis("Mouse Y");

        float cameraRotationX = xRot * /*lookSensetivity **/ (SettingManager.GameSettinngs.mouseSensitivity * 4000 + 1f) * Time.deltaTime;


        motor.RotateCamera(cameraRotationX);

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


    }

    private void FixedUpdate()
    {
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, Vector3.down, out _hit, 100f, environmentMask))
        {
            joint.targetPosition = new Vector3(0, -_hit.point.y, 0);
        }
        else
        {
            joint.targetPosition = new Vector3(0, 5, 0);
        }

        float xMov = Input.GetAxis("Horizontal");
        float zMov = Input.GetAxis("Vertical");

        Vector3 movHorizontal = transform.right * xMov;
        Vector3 movVertical = transform.forward * zMov;

        Vector3 velocity = (movHorizontal + movVertical) * speed;

        animator.SetFloat("ForwardVelocity", zMov);


        motor.Move(velocity);




        Vector3 _thrusterForce = Vector3.zero;
        if (Input.GetButton("Jump") && thrusterFuelAmount > 0.0)
        {
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

            if (thrusterFuelAmount > 0.01f)
            {
                _thrusterForce = Vector3.up * thrusterForce;
                SetJointSetting(0f);
            }

        }
        else
        {
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;
            SetJointSetting(jointSpring);
        }

        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0, 1);

        motor.ApplyThruster(_thrusterForce);
    }

    private void SetJointSetting(float _jointSpring)
    {
        joint.yDrive = new JointDrive
        {
            //mode = jointMode,
            positionSpring = _jointSpring,
            maximumForce = jointMaxForce

        };
    }
}

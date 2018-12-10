using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public static CameraControl Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public float KeyboardMoveSpeed = 1000;

    public float MouseLookSensitivity = 10;

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= KeyboardMoveSpeed * transform.right * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += KeyboardMoveSpeed * transform.right * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += KeyboardMoveSpeed * transform.forward * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= KeyboardMoveSpeed * transform.forward * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.position += KeyboardMoveSpeed * transform.up * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position -= KeyboardMoveSpeed * transform.up * Time.deltaTime;
        }
    }
}

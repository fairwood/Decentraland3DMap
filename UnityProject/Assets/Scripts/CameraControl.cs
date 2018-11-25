using UnityEngine;

public class CameraControl : MonoBehaviour
{

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
        if (Input.GetKey(KeyCode.Mouse1))
        {
            transform.Rotate(Vector3.right, -Input.GetAxis("Mouse Y") * MouseLookSensitivity, Space.Self);
            transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * MouseLookSensitivity, Space.World);
        }
        if (Input.GetKey(KeyCode.Mouse2))
        {
            transform.Translate(-new Vector3(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y")) * 60f, Space.Self);
        }

        transform.position += transform.forward * Input.mouseScrollDelta.y * 100f;
    }
}

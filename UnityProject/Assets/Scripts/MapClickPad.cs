using UnityEngine;
using UnityEngine.EventSystems;

public class MapClickPad : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsMouseOnMap;

    void Update()
    {
        BoxCollider boxCldr = null;
        if (IsMouseOnMap)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            var hit = Physics.Raycast(ray, out hitInfo, 1e8f, 1 << 8, QueryTriggerInteraction.Collide);
            if (hit)
            {
                boxCldr = hitInfo.collider as BoxCollider;
            }
        }

        DclMap.Instance.HoveredBoxCollider = boxCldr;

        if (IsMouseOnMap)
        {
            var cmrTra = CameraControl.Instance.transform;
            if (Input.GetKey(KeyCode.Mouse1))
            {
                cmrTra.Rotate(Vector3.right, -Input.GetAxis("Mouse Y") * CameraControl.Instance.MouseLookSensitivity, Space.Self);
                cmrTra.Rotate(Vector3.up, Input.GetAxis("Mouse X") * CameraControl.Instance.MouseLookSensitivity, Space.World);
            }
            if (Input.GetKey(KeyCode.Mouse2))
            {
                cmrTra.Translate(-new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 60f, Space.Self);
            }

            cmrTra.position += cmrTra.forward * Input.mouseScrollDelta.y * 100f;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsMouseOnMap = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsMouseOnMap = false;

    }
}
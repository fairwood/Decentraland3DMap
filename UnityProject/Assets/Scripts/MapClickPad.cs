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
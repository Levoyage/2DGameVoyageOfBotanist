using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIBlockerDetector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = Input.mousePosition;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);

            Debug.Log("🧩 UI Click Raycast Results:");
            foreach (var r in results)
            {
                Debug.Log("→ " + r.gameObject.name + " on layer " + r.gameObject.layer);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class TestButtonClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[Test] Retry 被成功点击");
    }
}

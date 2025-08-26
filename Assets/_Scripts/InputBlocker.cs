using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class InputBlocker : MonoBehaviour
{
    [Tooltip("Minimum time (in seconds) between clicks")]
    public float clickCooldown = 1f;

    private static float lastClickTime = -Mathf.Infinity;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse / touch tap
        {
            // Check if the click is over a UI element
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                if (Time.unscaledTime - lastClickTime < clickCooldown)
                {
                    // Too soon → block the click
                    Debug.Log("Click blocked - too fast!");

                    // Cancel UI click this frame
                    EventSystem.current.SetSelectedGameObject(null);
                    return;
                }

                // Valid click → allow and register
                lastClickTime = Time.unscaledTime;
            }
        }
    }
}
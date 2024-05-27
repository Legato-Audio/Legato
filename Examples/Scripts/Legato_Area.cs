using Legato;
using UnityEngine;

public class Legato_Area : MonoBehaviour
{
    [SerializeField] private Legato_Event enterEvent = null, exitEvent = null;

    private void OnTriggerEnter(Collider other)
    {
        if(enterEvent != null) enterEvent.Trigger();
    }

    private void OnTriggerExit(Collider other)
    {
        if (exitEvent != null) exitEvent.Trigger();
    }
}

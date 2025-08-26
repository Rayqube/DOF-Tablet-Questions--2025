using UnityEngine;

public class State : MonoBehaviour
{
    public virtual void Enter(string message = "", State previousState = null)
    {

    }

    public virtual void Exit(string message = "", State newState = null)
    {
        
    }

    public virtual void Tick()
    {

    }
}
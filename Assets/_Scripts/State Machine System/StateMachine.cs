using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;

public abstract class StateMachine : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected State defaultState;
    [SerializeField] protected bool enterDefaultStateOnStart;
    [Header("Monitoring")]
    [ShowInInspector,ReadOnly] public State currentState;
    [ShowInInspector, ReadOnly] public static bool inputAllowed = true;
    protected List<State> states = new List<State>();
    public static Action<State> OnAnyStateMachineStateChanged;
    public Action<State> OnStateChanged;



    public virtual void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<State>() != null)
            {
                states.Add(transform.GetChild(i).GetComponent<State>());
            }
        }
    }

    private void Start()
    {
        if (defaultState != null && enterDefaultStateOnStart)
        { ChangeState("", defaultState); }
    }

    public virtual void Update()
    {
        if (currentState != null)
        {
            currentState.Tick();
        }
    }

    public void ExitCurrentState(string message = "", State newState = null)
    {
        if (currentState != null)
        {
            // Check if the current state has a StateMachine component
            StateMachine stateMachine = currentState.GetComponent<StateMachine>();
            if (stateMachine != null && stateMachine != this) // Ensure it's not the same state machine
            {
                // Call ExitCurrentState on the nested state machine
                stateMachine.ExitCurrentState(message, newState);
                stateMachine.currentState = stateMachine.defaultState;
            }

            // Call Exit on the current state
            currentState.Exit(message, newState);
        }
    }

    public void ChangeState(string message = "", State newState = null)
    {
        if(!inputAllowed) { return; }
        if (currentState == newState) { return; }
        ExitCurrentState(message, newState); 
        State previousState = currentState;
        currentState = newState;
        currentState.Enter(message, currentState);
        OnStateChanged?.Invoke(currentState);
        OnAnyStateMachineStateChanged?.Invoke(currentState);
    }

}

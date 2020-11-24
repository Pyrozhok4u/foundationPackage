using System;
using System.Collections.Generic;
using Foundation.Logger;
using Foundation.Utils.OperationUtils;

namespace Foundation.FSM
{
    public class FSM<TTransition, TState> : IFSM<TTransition>
        where TTransition : Enum where TState : Enum
    {
        private readonly Dictionary<TState, FSMState<TTransition,TState>> _states = new Dictionary<TState, FSMState<TTransition, TState>>();
        
        public FSMState<TTransition,TState> CurrentState { get; private set; }
        
        public void AddState(FSMState<TTransition,TState> state)
        {
            _states.Add(state.StateId, state);
        }

        public void SetInitialState(TState stateId)
        {
            CurrentState = _states[stateId];
            
            CurrentState.OnStateEnter();
        }
        
        public void DeleteState(TState stateId)
        {
            _states.Remove(stateId);
        }
        
        public void PerformTransition(TTransition transitionId)
        {
            Result<TState> outputStateResult = CurrentState.GetOutputState(transitionId);
            if (!outputStateResult.Success) { return; }

            CurrentState.OnStateExit();
 
            CurrentState = _states[outputStateResult.Data];
            
            this.Log($"PerformTransition {CurrentState.StateId}");
            
            CurrentState.OnStateEnter();
        }
    }
}

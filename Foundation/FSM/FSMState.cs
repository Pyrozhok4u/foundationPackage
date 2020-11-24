using System;
using System.Collections.Generic;
using Foundation.Logger;
using Foundation.ServicesResolver;
using Foundation.Utils.OperationUtils;

namespace Foundation.FSM
{
    public abstract class FSMState<TTransition, TState> where TTransition : Enum where TState : Enum
    {
        protected readonly Dictionary<TTransition, TState> TransitionStateMap = new Dictionary<TTransition, TState>();
        protected readonly ServiceResolver ServiceResolver;

        public TState StateId { get; }

        public FSMState(ServiceResolver serviceResolver, TState stateId)
        {
            ServiceResolver = serviceResolver;
            StateId = stateId;
        }

        public void AddTransition(TTransition transitionId, TState stateId)
        {
            TransitionStateMap.Add(transitionId, stateId);
        }

        public void DeleteTransition(TTransition transitionId)
        {
            TransitionStateMap.Remove(transitionId);
        }

        public Result<TState> GetOutputState(TTransition transitionId)
        {
            Result<TState> result = new Result<TState>();
            if (TransitionStateMap.TryGetValue(transitionId, out TState nextStateId))
            {
                result.Data = nextStateId;
            }
            else
            {
                result.SetFailure($"Transition {transitionId} is not defined in state {StateId}");
                this.LogError(result);
            }

            return result;
        }
        
        public virtual void OnStateEnter() { }
        
        public virtual void OnStateExit() { } 
    }
}

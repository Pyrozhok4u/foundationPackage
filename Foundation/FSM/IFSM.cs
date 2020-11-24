using System;

namespace Foundation.FSM
{
    public interface IFSM<in TTransition> where TTransition : Enum
    {
        void PerformTransition(TTransition transitionId);
    }
}

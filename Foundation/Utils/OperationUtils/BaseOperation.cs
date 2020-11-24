using System;
using Foundation.Logger;

namespace Foundation.Utils.OperationUtils
{
    internal abstract class BaseOperation
    {
        internal readonly int operationID;
        internal readonly bool RunInParallel;
        protected string TargetMethodSeparator = ": ";
        internal int OperationIndex { get; private set; }
        
        protected Action<BaseOperation, Result> OnOperationComplete;
        protected Result PreviousResult;
        
        protected BaseOperation(bool runInParallel)
        {
            operationID = OperationID.NextID;
            RunInParallel = runInParallel;
        }

        internal void ExecuteOperation(int operationIndex, Result previousResult, Action<BaseOperation, Result> onOperationComplete)
        {
            OperationIndex = operationIndex;
            PreviousResult = previousResult;
            OnOperationComplete = onOperationComplete;
            ExecuteOperation();
        } 
        
        protected abstract void ExecuteOperation();
        internal abstract string GetOperationName();
    }
    
    internal static class OperationID
    {
        private static int _operationID;
        public static int NextID => ++_operationID;
    }
}

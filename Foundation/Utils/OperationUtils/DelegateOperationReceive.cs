namespace Foundation.Utils.OperationUtils
{
    internal class DelegateOperationReceive<T> : BaseOperation
    {
        private readonly OperationsQueue.OperationReceive<T> _operation;

        internal DelegateOperationReceive(OperationsQueue.OperationReceive<T> operation, bool runInParallel) : base(runInParallel)
        {
            _operation = operation;
        }

        protected override void ExecuteOperation()
        {
            Result<T> result = PreviousResult as Result<T>;
            if (result == null)
            {
                result.SetFailure($"{_operation.Method.Name} failed casting to Result<{typeof(T).Name}>!");
                OnOperationComplete.Invoke(this, result);
            }
            else if (result.Data == null)
            {
                result.SetFailure($"{_operation.Method.Name} is expecting to receive {typeof(T).Name} but it is null!");
                OnOperationComplete.Invoke(this, result);
            }
            else
            {
                _operation.Invoke(result.Data, OnExecutionComplete);
            }
        }

        private void OnExecutionComplete(Result result)
        {
            OnOperationComplete.Invoke(this, result);
        }

        internal override string GetOperationName()
        {
            return _operation.Target + TargetMethodSeparator + _operation.Method.Name;
        }
    }
}
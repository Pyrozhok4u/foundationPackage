namespace Foundation.Utils.OperationUtils
{
    internal class DelegateOperationSendReceive<T, U> : BaseOperation
    {
        private readonly OperationsQueue.OperationSendReceive<T, U> _operation;

        internal DelegateOperationSendReceive(OperationsQueue.OperationSendReceive<T, U> operation, bool runInParallel) : base(runInParallel)
        {
            _operation = operation;
        }

        protected override void ExecuteOperation()
        {
            Result<T> result = PreviousResult as Result<T>;
            if (result.Data != null)
            {
                _operation.Invoke(result.Data, OnExecutionComplete);
            }
            else
            {
                result.SetFailure($"{_operation.Method.Name} is expecting to receive {typeof(T).Name} but it is null!");
                OnOperationComplete.Invoke(this, result);
            }
        }

        private void OnExecutionComplete(Result<U> result)
        {
            OnOperationComplete.Invoke(this, result);
        }

        internal override string GetOperationName()
        {
            return _operation.Target + TargetMethodSeparator + _operation.Method.Name;
        }
    }
}
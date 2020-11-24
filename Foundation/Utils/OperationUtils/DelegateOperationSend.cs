namespace Foundation.Utils.OperationUtils
{
    internal class DelegateOperationSend<T> : BaseOperation
    {
        private readonly OperationsQueue.OperationSend<T> _operation;

        internal DelegateOperationSend(OperationsQueue.OperationSend<T> operation, bool runInParallel) : base(runInParallel)
        {
            _operation = operation;
        }

        protected override void ExecuteOperation()
        {
            _operation.Invoke(OnExecutionComplete);
        }

        private void OnExecutionComplete(Result<T> result)
        {
            if (result.Data == null)
            {
                result.SetFailure($"{_operation.Method.Name} is expecting to send {typeof(T).Name} but it is null!");
            }
            OnOperationComplete.Invoke(this, result);
        }

        internal override string GetOperationName()
        {
            return _operation.Target + TargetMethodSeparator + _operation.Method.Name;
        }
    }
}
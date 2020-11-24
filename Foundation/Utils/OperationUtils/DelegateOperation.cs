namespace Foundation.Utils.OperationUtils
{
    internal class DelegateOperation : BaseOperation
    {
        private readonly OperationsQueue.Operation _operation;

        internal DelegateOperation(OperationsQueue.Operation operation, bool runInParallel) : base(runInParallel)
        {
            _operation = operation;
        }

        protected override void ExecuteOperation()
        {
            _operation.Invoke(OnExecutionComplete);
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
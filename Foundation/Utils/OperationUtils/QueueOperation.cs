namespace Foundation.Utils.OperationUtils
{
    internal class QueueOperation : BaseOperation
    {
        private readonly OperationsQueue _queue;

        internal QueueOperation(OperationsQueue queue, bool runInParallel) : base(runInParallel)
        {
            _queue = queue;
        }

        protected override void ExecuteOperation()
        {
            _queue.Finally(OnExecutionComplete);
            _queue.Run();
        }

        private void OnExecutionComplete(Result result)
        {
            OnOperationComplete.Invoke(this, result);
        }

        internal override string GetOperationName()
        {
            return "Branch operations queue count: " + _queue.OperationsCount;
        }
    }
}
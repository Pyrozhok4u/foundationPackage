using System;
using System.Collections.Generic;
using Foundation.Logger;
using Foundation.Utils.StopwatchUtils;

namespace Foundation.Utils.OperationUtils
{
    public class OperationsQueue
    {

        private const string DefaultFlowName = "Annonymous Flow";
        private const string OperationStopwatchKey = "Operation_";
        
        // Regular operation (no return or receive value)
        public delegate void Operation(Action<Result> operationComplete);
        public delegate void OperationSend<T>(Action<Result<T>> operationComplete);
        public delegate void OperationReceive<T>(T data, Action<Result> operationComplete);
        public delegate void OperationSendReceive<T, U>(T data, Action<Result<U>> operationComplete);
        
        private Action<Result> _onQueueComplete;

        private string _flowName = DefaultFlowName;
        private List<BaseOperation> _operations = new List<BaseOperation>();
        private bool _isRunning;
        private int _parallelOperationsCounter;
        private int _totalExecutedOperations;
        private Result _lastOperationResult;
        private Result _result = new Result();

        public OperationsQueue(Result initialArgs = null)
        {
            _lastOperationResult = initialArgs;
        }

        public int OperationsCount => _operations.Count;

        #region Run operations queue logic
    
        /// <summary>
        /// Starts running the operations in queue
        /// </summary>
        public void Run(string flowName = DefaultFlowName)
        {
            // Start executing operations only if no operation is currently active
            if (_parallelOperationsCounter > 0) { return; }

            _flowName = flowName;
            ExecuteNextOperation();
        }

        /// <summary>
        /// Start executing the next operation in queue & any
        /// subsequent operations that should run in parallel
        /// </summary>
        private void ExecuteNextOperation()
        {
            _parallelOperationsCounter++;
            _totalExecutedOperations++;

            // Dequeue next operation & execute it
            BaseOperation operation = _operations[0];
            _operations.RemoveAt(0);

            try
            {
                this.Log(operation.GetOperationName());
                StopwatchService.Start(OperationStopwatchKey + operation.operationID);
                operation.ExecuteOperation(_totalExecutedOperations, _lastOperationResult, OnOperationComplete);
            }
            catch (Exception exception)
            {
                this.LogException(exception);
                Result result = new Result();
                result.SetFailure("Exception while executing operation:\n" + exception.Message);
                OnOperationComplete(operation, result);
                return;
            }

            // Run next operation if it should run in parallel
            if (_operations.Count > 0 && _operations[0].RunInParallel) { ExecuteNextOperation(); }
        }

        /// <summary>
        /// Triggered externally by the operation upon completion
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="result"></param>
        private void OnOperationComplete(BaseOperation operation, Result result)
        {
            long operationTime = StopwatchService.Stop(OperationStopwatchKey + operation.operationID);
            this.LogAssertion($"{operation.GetOperationName()}: Time ms: {operationTime}\n{result}", result.Success);
        
            // Check if should wait for parallel operations before proceeding to next step
            _result += result;
            _parallelOperationsCounter--;
            if (_parallelOperationsCounter > 0) { return; }

            _lastOperationResult = result;
            
            // Continue to next operation if exists or complete operation queue
            if (_result.Success && _operations.Count > 0) { ExecuteNextOperation(); }
            else { OnOperationQueueCompleted(); }
        }

        /// <summary>
        /// Called internally when last operation finished & triggers the on queue complete event
        /// </summary>
        private void OnOperationQueueCompleted()
        {
            this.LogAssertion(_flowName + ": " + "Operation queue completed: " + _result, _result.Success);
            // if(_result.Success)
            _onQueueComplete?.Invoke(_result);
            _onQueueComplete = null;
            _operations.Clear();
        }
    
        #endregion
    
        #region Add operations API
    
        public static OperationsQueue Do(Operation operation)
        {
            OperationsQueue queue = new OperationsQueue();
            return queue.Add(operation, false);
        }
        
        public static OperationsQueue Do<T>(OperationSend<T> operation)
        {
            OperationsQueue queue = new OperationsQueue();
            return queue.Add(operation, false);
        }
        
        public static OperationsQueue Do<T, U>(T args, OperationSendReceive<T,U> operation)
        {
            Result<T> result = new Result<T>();
            result.Data = args;
            OperationsQueue queue = new OperationsQueue(result);
            return queue.Add(operation, false);
        }
        
        public static OperationsQueue Do<T>(OperationReceive<T> operation)
        {
            OperationsQueue queue = new OperationsQueue();
            return queue.Add<T>(operation, false);
        }

        public OperationsQueue And(Operation operation)
        {
            return Add(operation, true);
        }

        public OperationsQueue Then(Operation operation)
        {
            return Add(operation, false);
        }

        public OperationsQueue Then<T>(OperationSend<T> operation)
        {
            return Add<T>(operation, false);
        }
        
        public OperationsQueue Then<T>(OperationReceive<T> operation)
        {
            return Add<T>(operation, false);
        }
        
        public OperationsQueue Then<T, U>(OperationSendReceive<T, U> operation)
        {
            return Add<T, U>(operation, false);
        }

        public OperationsQueue Branch(Operation operation, bool parallel = false)
        {
            BranchOperationQueue branchOperationQueue = new BranchOperationQueue(this);
            Add(branchOperationQueue, parallel);
            return branchOperationQueue.Add(operation, parallel);
        }

        public OperationsQueue AndBranch(Operation operation)
        {
            return Branch(operation, true);
        }

        private OperationsQueue Add(Operation operation, bool runInParallel)
        {
            DelegateOperation delegateOperation = new DelegateOperation(operation, runInParallel);
            _operations.Add(delegateOperation);
            return this;
        }
        
        private OperationsQueue Add<T>(OperationSend<T> operation, bool runInParallel)
        {
            DelegateOperationSend<T> delegateOperationSend = new DelegateOperationSend<T>(operation, runInParallel);
            _operations.Add(delegateOperationSend);
            return this;
        }

        private OperationsQueue Add<T>(OperationReceive<T> operation, bool runInParallel)
        {
            DelegateOperationReceive<T> delegateOperation = new DelegateOperationReceive<T>(operation, runInParallel);
            _operations.Add(delegateOperation);
            return this;
        }
        
        private OperationsQueue Add<T, U>(OperationSendReceive<T, U> operation, bool runInParallel)
        {
            DelegateOperationSendReceive<T, U> delegateOperation = new DelegateOperationSendReceive<T, U>(operation, runInParallel);
            _operations.Add(delegateOperation);
            return this;
        }
        
        private OperationsQueue Add(OperationsQueue queue, bool runInParallel)
        {
            QueueOperation queueOperation = new QueueOperation(queue, runInParallel);
            _operations.Add(queueOperation);
            return this;
        }

        public virtual OperationsQueue Finally(Action<Result> onQueueComplete)
        {
            _onQueueComplete = onQueueComplete;
            return this;
        }

        #endregion
    }

    public class BranchOperationQueue : OperationsQueue
    {
        private readonly OperationsQueue _parentQueue;

        internal BranchOperationQueue(OperationsQueue parentQueue)
        {
            _parentQueue = parentQueue;
        }

        public override OperationsQueue Finally(Action<Result> onQueueComplete)
        {
            base.Finally(onQueueComplete);
            // Return parent queue to allow farther chaining of operations
            return _parentQueue;
        }
    }
}

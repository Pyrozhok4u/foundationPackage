using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Foundation.Logger;

namespace Foundation.Utils.OperationUtils
{
    public class Result<T> : Result
    {
        public T Data;
        
        public static T operator +(T a, Result<T> b)
        {
            (a as Result)?.AddResult(b);
            return a;
        }

        public static Result<T> operator +(Result<T> a, Result b)
        {
            a.AddResult(b);
            return a;
        }
    }

    public class Result
    {
        private bool _success = true;
        public bool Success => _success;
        public List<string> Messages = new List<string>();
        private string _subTitle;
        
        #region Constructors
        
        /// <summary>
        /// Create a successful result
        /// </summary>
        public Result() { }
        
        /// <summary>
        /// Create a result with success state (false / true) & a message
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        public Result(bool success, string message) : this()
        {
            _success = success;
            AddMessage(message);
        }

        // Syntactic sugar for quickly setting up successful result 
        public static Result Successful => new Result();

        // Syntactic sugar for quickly setting up failed result 
        public static Result Failure(string message)
        {
            return new Result(false, message);
        }
        
        #endregion
        
        #region Set failure, add messages, and combine results...
        
        /// <summary>
        /// Creates a failed result with the given exception message
        /// </summary>
        /// <param name="exception"></param>
        public Result(Exception exception)
        {
            SetFailure(exception.Message);
        }

        /// <summary>
        /// Sets a sub-title after the result title & before the messages
        /// </summary>
        /// <param name="subTitle"></param>
        public void SetSubTitle(string subTitle)
        {
            _subTitle = subTitle;
        }
        
        /// <summary>
        /// Sets the result as failed with informative message
        /// </summary>
        /// <param name="message"></param>
        public void SetFailure(string message)
        {
            _success = false;
            AddMessage(message);
        }

        /// <summary>
        /// Sets the result as failed with informative message
        /// </summary>
        /// <param name="messages"></param>
        public void SetFailure(List<string> messages)
        {
            _success = false;
            AddMessage(messages);
        }

        public void AssertMessage(bool condition, string message)
        {
            if(condition) { AddMessage(message); }
            else { SetFailure(message); }
        }
        
        public void AddMessage(List<string> messages)
        {
            foreach (string message in messages)
            {
                AddMessage(message);
            }
        }

        public void AddMessage(string message)
        {
            // Ignore null or empty messages
            if (message == null) message = "<null message>";
            // Finally, add the message
            Messages.Add(message);
        }

        public static Result operator +(Result a, Result b)
        {
            return a.AddResult(b);
        }

        public Result AddResult(Result result)
        {
            // If either result failed, fail the entire result
            if (!result.Success) { _success = false; }
            // Copy messages if exists...
            AddMessage(result.Messages);
            return this;
        }

        #endregion
        
        #region Get / Print result info helpers
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Result successful: " + Success);
            if (!string.IsNullOrEmpty(_subTitle))
            {
                sb.AppendLine(_subTitle);
            }
            for (int i = 0; i < Messages.Count; i++)
            {
                sb.AppendLine((i + 1) + ": " + Messages[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints to log the result state & messages line by line
        /// </summary>
        [Conditional(LoggerConfig.EnableLogsSymbols)]
        public void Log()
        {
            this.LogAssertion(this.ToString(), _success);
        }
        
        #endregion
    }
}

using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Foundation.Logger
{
	/// <summary>
	/// Class containing methods to ease debugging while developing a game.
	/// Methods are called only if "ENABLE_LOGS" define is added to Scripting Define Symbols in Player Settings.
	/// Should add reference to UnityEngine.dll before building solution
	/// </summary>
	public static class Debugger
	{
		private const string ENABLE_LOGS = "ENABLE_LOGS";
		/// <summary>
		/// Used for stripping Errors and Exceptions
		/// </summary>
		public static bool IsErrorsEnabled;

		#region Logs
		/// <summary>
		/// Logs message to the Unity Console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		[Conditional(ENABLE_LOGS)]
		public static void Log(this object caller, object message)
		{
			UnityEngine.Debug.Log(GetTheStackInfo(caller, message));
		}

		/// <summary>
		/// Logs message to the Unity Console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		[Conditional(ENABLE_LOGS)]
		public static void Log(this object caller, object message, UnityEngine.Object context)
		{
			UnityEngine.Debug.Log(GetTheStackInfo(caller, message), context);
		}

		/// <summary>
		/// Logs a formatted message to the Unity Console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		[Conditional(ENABLE_LOGS)]
		public static void LogFormat(this object caller, string format, params object[] args)
		{
			UnityEngine.Debug.LogFormat(GetTheStackInfo(caller, format), args);
		}

		/// <summary>
		/// Logs a formatted message to the Unity Console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="context">Object to which the message applies.</param>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		[Conditional(ENABLE_LOGS)]
		public static void LogFormat(this object caller, UnityEngine.Object context, string format, params object[] args)
		{
			UnityEngine.Debug.LogFormat(context, GetTheStackInfo(caller, format), args);
		}
		#endregion

		#region Assertions
		//The assertion method should receive a bool condition & log message.

		//If the condition evaluates to "true", print normal log, else - print an error.
		//Assertions stripping should be similar to error reporting.i.e.they are not fully stripped but only print if the error enable flag is on.
		/// <summary>
		/// Logs error if condition is false
		/// </summary>
		/// <param name="caller">Caller object to get type etc.</param>
		/// <param name="message">A user provided message that will be logged</param>
		/// <param name="condition">The condition to test</param>
		public static void Assert(this object caller, object message, bool condition = true)
		{
			if (!condition)
			{
				LogError(caller, "[Condition: " + condition + "]: " + message);
			}
		}

		/// <summary>
		/// Logs error if condition is false
		/// </summary>
		/// <param name="caller">Caller object to get type etc.</param>
		/// <param name="context">Object to which the message applies.</param>
		/// <param name="message">A user provided message that will be logged</param>
		/// <param name="condition">The condition to test</param>
		public static void Assert(this object caller, object message, Object context, bool condition = true)
		{
			if (!condition)
			{
				LogError(caller, "[Condition: " + condition + "]: " + message, context);
			}
		}

		/// <summary>
		/// Logs error if condition is false
		/// </summary>
		/// <param name="caller">Caller object to get type etc.</param>
		/// <param name="format">A user provided format that will be logged</param>
		/// <param name="condition">The condition to test</param>
		/// <param name="args">Format arguments.</param>
		public static void AssertFormat(this object caller, string format, bool condition = true, params object[] args)
		{
			if (!condition)
			{
				LogErrorFormat(caller, "[Condition: " + condition + "]: " + format, args);
			}
		}

		/// <summary>
		/// Logs error if condition is false
		/// </summary>
		/// <param name="caller">Caller object to get type etc.</param>
		/// <param name="context">Object to which the message applies.</param>
		/// <param name="format">A user provided format that will be logged</param>
		/// <param name="condition">The condition to test</param>
		/// <param name="args">Format arguments.</param>
		public static void AssertFormat(this object caller, Object context, string format, bool condition = true, params object[] args)
		{
			if (!condition)
			{
				LogErrorFormat(caller, context, "[Condition: " + condition + "]: " + format, args);
			}
		}

		/// <summary>
		/// Logs message if condition is true, otherwise log a error
		/// </summary>
		/// <param name="caller">Caller object to get type etc.</param>
		/// <param name="message">A user provided message that will be logged</param>
		/// <param name="condition">The condition to test</param>
		public static void LogAssertion(this object caller, object message, bool condition = true)
		{
			if (condition)
			{
				Log(caller, message);
			}
			else
			{
				LogError(caller, "[Condition: " + condition + "]: " + message);
			}
		}

		/// <summary>
		/// Logs message if condition is true, otherwise log a error
		/// </summary>
		/// <param name="caller">Caller object to get type etc.</param>
		/// <param name="context">Object to which the message applies.</param>
		/// <param name="message">A user provided message that will be logged</param>
		/// <param name="condition">The condition to test</param>
		public static void LogAssertion(this object caller, object message, Object context, bool condition = true)
		{
			if (condition)
			{
				Log(caller, message, context);
			}
			else
			{
				LogError(caller, "[Condition: " + condition + "]: " + message, context);
			}
		}

		/// <summary>
		/// Logs message if condition is true, otherwise log a error
		/// </summary>
		/// <param name="caller">Caller object to get type etc.</param>
		/// <param name="format">A user provided format that will be logged</param>
		/// <param name="condition">The condition to test</param>
		/// <param name="args">Format arguments.</param>
		public static void LogAssertionFormat(this object caller, string format, bool condition = true, params object[] args)
		{
			if (condition)
			{
				LogFormat(caller, format, args);
			}
			else
			{
				LogErrorFormat(caller, "[Condition: " + condition + "]: " + format, args);
			}
		}

		/// <summary>
		/// Logs message if condition is true, otherwise log a error
		/// </summary>
		/// <param name="caller">Caller object to get type etc.</param>
		/// <param name="context">Object to which the message applies.</param>
		/// <param name="format">A user provided format that will be logged</param>
		/// <param name="condition">The condition to test</param>
		/// <param name="args">Format arguments.</param>
		public static void LogAssertionFormat(this object caller, Object context, string format, bool condition = true, params object[] args)
		{
			if (condition)
			{
				LogFormat(caller, context, format, args);
			}
			else
			{
				LogErrorFormat(caller, context, "[Condition: " + condition + "]: " + format, args);
			}
		}

		#endregion

		#region Warnings
		/// <summary>
		/// A variant of Debug.Log that logs a warning message to the console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		[Conditional(ENABLE_LOGS)]
		public static void LogWarning(this object caller, object message)
		{
			UnityEngine.Debug.LogWarning(GetTheStackInfo(caller, message));
		}

		/// <summary>
		/// A variant of Debug.Log that logs a warning message to the console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		[Conditional(ENABLE_LOGS)]
		public static void LogWarning(this object caller, object message, UnityEngine.Object context)
		{
			UnityEngine.Debug.LogWarning(GetTheStackInfo(caller, message), context);
		}

		/// <summary>
		/// Logs a formatted warning message to the Unity Console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		[Conditional(ENABLE_LOGS)]
		public static void LogWarningFormat(this object caller, string format, params object[] args)
		{
			UnityEngine.Debug.LogWarningFormat(GetTheStackInfo(caller, format), args);
		}

		/// <summary>
		/// Logs a formatted warning message to the Unity Console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="context">Object to which the message applies.</param>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		[Conditional(ENABLE_LOGS)]
		public static void LogWarningFormat(this object caller, UnityEngine.Object context, string format, params object[] args)
		{
			UnityEngine.Debug.LogWarningFormat(context, GetTheStackInfo(caller, format), args);
		}
		#endregion

		#region Errors & Exceptions
		/// <summary>
		/// A variant of Debug.Log that logs an error message to the console.
		/// </summary>
		/// <param name="caller">Caller object to get type etc.</param>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		public static void LogError(this object caller, object message)
		{
#if ENABLE_LOGS
			UnityEngine.Debug.LogError(GetTheStackInfo(caller, message));
#endif
			if (!IsErrorsEnabled) { return; }
			// TODO: Report analytic event...			
		}

		/// <summary>
		/// A variant of Debug.Log that logs an error message to the console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogError(this object caller, object message, UnityEngine.Object context)
		{
#if ENABLE_LOGS
			UnityEngine.Debug.LogError(GetTheStackInfo(caller, message), context);
#endif
			if (!IsErrorsEnabled) { return; }
			// TODO: Report analytic event...
		}

		/// <summary>
		/// Logs a formatted error message to the Unity console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		public static void LogErrorFormat(this object caller, string format, params object[] args)
		{
#if ENABLE_LOGS
			UnityEngine.Debug.LogErrorFormat(GetTheStackInfo(caller, format), args);
#endif
			if (!IsErrorsEnabled) { return; }
			// TODO: Report analytic event...
		}

		/// <summary>
		/// Logs a formatted error message to the Unity console.
		/// </summary>
		/// <param name="context">Object to which the message applies.</param>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		/// <param name="caller"> Caller object to get type etc.</param>
		public static void LogErrorFormat(this object caller, UnityEngine.Object context, string format, params object[] args)
		{

#if ENABLE_LOGS

			UnityEngine.Debug.LogErrorFormat(context, GetTheStackInfo(caller, format), args);
#endif
			if (!IsErrorsEnabled) { return; }
			// TODO: Report analytic event...
		}

		/// <summary>
		/// A variant of Debug.Log that logs an error message to the console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="exception">Runtime Exception.</param>
		public static void LogException(this object caller, Exception exception)
		{
#if ENABLE_LOGS
			UnityEngine.Debug.Log(GetTheStackInfo(caller, exception.Message));
			UnityEngine.Debug.LogException(exception);
#endif
			if (!IsErrorsEnabled) { return; }
			// TODO: Report analytic event...
		}

		/// <summary>
		/// A variant of Debug.Log that logs an error message to the console.
		/// </summary>
		/// <param name="caller"> Caller object to get type etc.</param>
		/// <param name="exception">Runtime Exception.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogException(this object caller, Exception exception, UnityEngine.Object context)
		{
#if ENABLE_LOGS

			UnityEngine.Debug.Log(GetTheStackInfo(caller, exception.Message));
			UnityEngine.Debug.LogException(exception, context);
#endif
			if (!IsErrorsEnabled) { return; }
			// TODO: Report analytic event...
		}
		#endregion

		private static string GetTheStackInfo(object caller, object message)
		{
			if (caller == null)
			{
				return message.ToString();
			}
			if (Application.platform != RuntimePlatform.WebGLPlayer && Application.platform != RuntimePlatform.Android)
			{
				StackFrame frame = new StackFrame(2, true);
				int lineNumber = frame.GetFileLineNumber();

				if (lineNumber == 0)
				{
					frame = new StackFrame(3, true);
					lineNumber = frame.GetFileLineNumber();
				}
				string methodName = frame.GetMethod().Name;
				return "[line:" + lineNumber + "] " + caller.GetType().Name + ": " + methodName + ": " + message.ToString();
			}
			return caller.GetType().Name + ": " + message.ToString();
		}

	}

}

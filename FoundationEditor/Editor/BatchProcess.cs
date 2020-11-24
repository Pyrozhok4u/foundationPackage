#if UNITY_EDITOR
#define IS_EDITOR
#endif

using System;
using System.Diagnostics;
using System.Threading;
using Foundation.Logger;
using Foundation.MonoUtils;
using Foundation.Utils.OperationUtils;
using Debug = UnityEngine.Debug;
#if IS_EDITOR
using UnityEditor;
#endif

namespace FoundationEditor
{


    public class BatchProcess
    {
        private Process _process;
        private Action<Result<string>> _callback;
        private bool _resultsRecived;

        private string _output;
        private string _error;

        public BatchProcess(string fileName, string args, Action<Result<string>> onProcessResult)
        {

            if (onProcessResult == null) return;

            _callback = onProcessResult;
#if UNITY_EDITOR
            EditorApplication.update += OnUpdate;
#endif
            Thread t = new Thread(delegate() { StartProcess(fileName, args); });
            t.Start();
        }

        public static void StartProcess(string fileName, string args, Action<Result<string>> onProcessResult) { new BatchProcess(fileName, args, onProcessResult); }

        private void StartProcess(string fileName, string args)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;

            startInfo.FileName = fileName;
            startInfo.Arguments = args;

            _process = new Process();

            _process.StartInfo = startInfo;
            _process.EnableRaisingEvents = true;

            _process.Start();

            // According this documentation reading both StandardError & StandardOutput
            // at the same time may lead to dead-lock on rare scenarions.
            // As the solution is pretty complicated and doesn’t seem needed atm we’ll suffice with this.
            // For future reference: https://github.com/kenny-evitt/ExecuteCommandLineProgram/blob/master/ExecuteCommandLineProgram/CommandLineProgramProcess.cs
            // Script main’s addition is that it “ReadsToEnd” on different threads.
            _error = _process.StandardError.ReadToEnd();
            _output = _process.StandardOutput.ReadToEnd();

            _process.WaitForExit();
            _process.Close();

            this.Log("Finished process");

            _resultsRecived = true;
        }

        private void OnUpdate()
        {
            if (!_resultsRecived) return;

            if (_callback != null)
            {
                _resultsRecived = false;

                // If error, send the error result only
                // TODO: Should be encasulated in result object that always contains both error & result in case both contain info.
                if (!string.IsNullOrEmpty(_error))
                {
                    Result<string> result = new Result<string>();
                    result.Data = _error;
                    result.SetFailure(_error);
                    _callback.Invoke(result);
                }
                else
                {
                    Result<string> result = new Result<string>();
                    result.Data = _output;
                    _callback.Invoke(result);
                }

                KillProess();
            }
        }

        private void KillProess()
        {
#if UNITY_EDITOR
            EditorApplication.update -= OnUpdate;
#endif
            _callback = null;
            _process = null;
        }
    }
}

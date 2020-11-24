using System;
using System.Collections.Generic;
using Foundation.Logger;
using Foundation.Utils.OperationUtils;
using UnityEditor;
using UnityEditor.Compilation;

namespace FoundationEditor.EditorUtils.Editor
{
    /// <summary>
    /// Simple worker class to get notified and continue execution only after unity finished compiling...
    /// </summary>
    public class CompileHandler
    {
        private Action<Result> _onCompilerFinished;
        private volatile bool _active;
        private volatile bool _detectedCompilationErrors;
        private volatile List<CompilerMessage> _compilerErrors = new List<CompilerMessage>();

        public static CompileHandler WaitForCompile(Action<Result> onComplete)
        {
            return new CompileHandler(onComplete);
        }
        
        private CompileHandler(Action<Result> onComplete)
        {
            _onCompilerFinished = onComplete;
            _active = true;
            EditorApplication.update += OnEditorUpdate;
            CompilationPipeline.assemblyCompilationFinished += ProcessCompileFinish;
        }

        ~CompileHandler()
        {
            this.Assert("Compile Handler destroyed - is active state: " + _active, !_active);
            EditorApplication.update -= OnEditorUpdate;
            CompilationPipeline.assemblyCompilationFinished -= ProcessCompileFinish;
        }
        
        private void ProcessCompileFinish(string assemblyName, CompilerMessage[] compilerMessages)
        {
            foreach (CompilerMessage compilerMessage in compilerMessages)
            {
                if (compilerMessage.type == CompilerMessageType.Error)
                {
                    _compilerErrors.Add(compilerMessage);
                }
            }
            this.Log($"CompilationPipeline.assemblyCompilationFinished: {assemblyName} - errors: {_compilerErrors.Count}");
        }

        private void OnEditorUpdate()
        {
            // Wait until compilation is done...
            if (EditorApplication.isCompiling) { return; }

            if (_active)
            {
                Result result = new Result();
                if (_compilerErrors.Count > 0)
                {
                    result.SetSubTitle("Compiler finished with errors:");
                    foreach (CompilerMessage compilerError in _compilerErrors)
                    {
                        result.SetFailure($"{compilerError.line}:{compilerError.file}:{compilerError.message}");
                    }
                }
                // Invoke callback and null out action reference
                _active = false;
                this.Log("Unity Finished compiling - trigger compiler handler callback");
                _onCompilerFinished?.Invoke(result);
                _onCompilerFinished = null;
                CompilationPipeline.assemblyCompilationFinished -= ProcessCompileFinish;
                EditorApplication.update -= OnEditorUpdate;
            }
        }
    }
}

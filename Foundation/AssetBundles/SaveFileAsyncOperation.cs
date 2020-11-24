#if UNITY_ANDROID || UNITY_IOS
#define ENABLE_ASYNC_SAVE
#endif

#if ENABLE_ASYNC_SAVE
using System.IO;
using System.Threading.Tasks;
#endif

using System;
using Foundation.Utils.OperationUtils;
using Foundation.Logger;
using Foundation.MonoUtils;
using Foundation.ServicesResolver;

namespace Foundation.AssetBundles
{
    public class SaveFileAsyncOperation
    {
        #if ENABLE_ASYNC_SAVE
        
        private readonly MonoService _monoService;
        private readonly string _filePath;
        private readonly Action<Result> _onComplete;
        
        private Task _asyncTask;
        private FileStream _sourceStream;

        public SaveFileAsyncOperation(byte[] bytes, string filePath, ServiceResolver serviceResolver, Action<Result> onComplete)
        {
            _filePath = filePath;
            _onComplete = onComplete;
            _monoService = serviceResolver.Resolve<MonoService>();
            
            Init(bytes, filePath);
        }

        private void Init(byte[] bytes, string filePath)
        {
            
            _sourceStream = File.Open(filePath, FileMode.OpenOrCreate);
            _sourceStream.Seek(0, SeekOrigin.End);
            _asyncTask = _sourceStream.WriteAsync(bytes, 0, bytes.Length);
            
            _monoService.RegisterSlowUpdate(TrackCacheBundleAsync);
        }

        private void TrackCacheBundleAsync(float deltaTime)
        {
            bool finished = false;
            if (_asyncTask.IsFaulted)
            {
                this.LogError($"Error caching async file {_filePath}: {_asyncTask.Exception?.Message}");
                finished = true;
            }
            else if (_asyncTask.IsCompleted)
            {
                this.Log($"Async save finished successfully for file {_filePath}");
                finished = true;
            }

            if (finished)
            {
                // for now, we don't need to do anything on complete & we're tracking just for reporting purposes
                _monoService.UnregisterSlowUpdate(TrackCacheBundleAsync);
                _onComplete?.Invoke(Result.Successful);
            }
        }
        
        #endif

        public static void SaveAsync(byte[] bytes, string filePath, ServiceResolver serviceResolver, Action<Result> onComplete = null)
        {
            #if ENABLE_ASYNC_SAVE
            new SaveFileAsyncOperation(bytes, filePath, serviceResolver, onComplete);
            #else
            onComplete?.Invoke(Result.Failure("Save async is not supported on current platform"));
            #endif
        }
    }
}

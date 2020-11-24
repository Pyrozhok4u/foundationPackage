using System;
using Foundation.Utils.OperationUtils;

namespace FoundationEditor.AWSUtils.Editor
{
    public interface ICloudStorage
    {
        void DownloadFile(string path, Action<Result<string>> onComplete = null);
        void DownloadBytes(string path, Action<Result<byte[]>> onComplete = null);
        void UploadFile(string localFilePath, string path, Action<Result> onComplete = null);
        void DeleteFile(string path, Action<Result> onComplete = null);
    }
}

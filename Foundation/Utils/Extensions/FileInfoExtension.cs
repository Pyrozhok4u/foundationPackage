using System.IO;
using Foundation.Logger;

namespace Foundation.Utils.Extensions
{
    public static class FileInfoExtensions
    {
        public static void Rename(this FileInfo fileInfo, string newName)
        {
            if (fileInfo.Directory == null)
            {
                Debugger.LogError(null, $"Can't rename file {fileInfo.Name} because it's parent directory is null!");
                return;
            }
            fileInfo.MoveTo(Path.Combine(fileInfo.Directory.FullName, newName));
        }
    }
}

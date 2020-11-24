using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Foundation.Utils.OperationUtils;

namespace Foundation.AssetBundles
{
    public static class BundleManifestUtils
    {
        
        public const string HashSeparator = "~";
        
        public static string GetBundleManifestHashedFileName(string hash, bool includeExtension = true)
        {
            string fileName = $"BundleManifest~{hash}";
            if (includeExtension) fileName += ".bytes";
            return fileName;
        }
        
        public static Result SerializeBundleManifest(BundleManifest bundleManifest, string filePath)
        {
            Result result = new Result();
            FileStream fileStream  = null;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Create);
                BinaryFormatter binaryFormatter = GetBinaryFormatter();
                binaryFormatter.Serialize(fileStream, bundleManifest);
            }
            catch (Exception e)
            {
                //this.LogException(e);
                result.SetFailure("Failed serializing bundle manifest: " + e.Message);
            }
            finally
            {
                // Make sure we dispose memory stream
                fileStream?.Dispose();
            }

            return result;
        }
        
        public static Result<BundleManifest> DeSerializeBundleManifest(byte[] bytes)
        {
            Result<BundleManifest> result = new Result<BundleManifest>();

            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream(bytes);
                BinaryFormatter binaryFormatter = GetBinaryFormatter();
                
                BundleManifest bundleManifest = binaryFormatter.Deserialize(memoryStream) as BundleManifest;
                result.Data = bundleManifest;
                if (bundleManifest == null)
                {
                    result.SetFailure("Deserialize bundle manifest failed (null)");
                }
            }
            catch (Exception e)
            {
                //this.LogException(e);
                result.SetFailure("Exception deserializing bundle manifest: " + e.Message);
            }
            finally
            {
                // Make sure we dispose memory stream
                memoryStream?.Dispose();
            }

            return result;
        }

        private static BinaryFormatter GetBinaryFormatter()
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter()
            {
                AssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeFormat = FormatterTypeStyle.TypesWhenNeeded
            };
            return binaryFormatter;
        }
        
        public static string GetHashedName(string name, string hash)
        {
            return name + HashSeparator + hash;
        }
    }
}

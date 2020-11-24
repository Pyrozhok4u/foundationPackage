using System;
using System.IO;
using System.Text;

namespace FoundationEditor.Utils.MD5.Editor
{
    public static class MD5Utils
    {
        private const string HexadecimalFormat = "x2";
    
        public static string CreateMD5Hash(string fullFilePath)
        {
            string hash = string.Empty;
            using (FileStream fileStream = new FileStream(fullFilePath, FileMode.Open))
            {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] data = md5.ComputeHash(fileStream);
                    StringBuilder sBuilder = new StringBuilder();
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString(HexadecimalFormat));
                    }
                    hash = sBuilder.ToString();
                }
            }
            return hash;
        }

        public static bool VerifyMD5Hash(string fullFilePath, string hash)
        {
            if (string.IsNullOrEmpty(hash)) { return false; }
            return 0 == StringComparer.OrdinalIgnoreCase.Compare(CreateMD5Hash(fullFilePath), hash);
        }
    
    }
}

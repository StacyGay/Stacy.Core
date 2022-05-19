using System;
using System.Text;

namespace Stacy.Core.Encryption
{
    public class CodeTimestampEncryptor
    {
        public string EncryptCode(string code)
        {
            var toEncrypt = string.Format("{0}|{1}", code, DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            var bytes = Encoding.UTF8.GetBytes(toEncrypt);
            return Convert.ToBase64String(bytes);
        }

        public string DecryptCode(string encryptedCode)
        {
            var encodedBytes = Convert.FromBase64String(encryptedCode);
            var decodedString = Encoding.UTF8.GetString(encodedBytes);
            return decodedString.Split('|')[0];
        }
    }
}

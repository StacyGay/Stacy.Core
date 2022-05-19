using System;
using System.Security.Cryptography;
using System.Text;

namespace Stacy.Core.Encryption
{
    public class TripleDesEncryptor
    {
        public string Encrypt(string toEncrypt, string key, string iv)
        {
            var toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
            
            var hash = new SHA512CryptoServiceProvider();
            var keyArray = hash.ComputeHash(Encoding.UTF8.GetBytes(key));
            var trimmedBytes = new byte[24];
            Buffer.BlockCopy(keyArray, 0, trimmedBytes, 0, 24);
            keyArray = trimmedBytes;
            
            var ivArray = hash.ComputeHash(Encoding.UTF8.GetBytes(iv));
            var trimmedIvBytes = new byte[8];
            Buffer.BlockCopy(ivArray, 0, trimmedIvBytes, 0, 8);
            ivArray = trimmedIvBytes;

            var tdes = new TripleDESCryptoServiceProvider
            {
                Mode = CipherMode.CBC, //mode of operation. there are other 4 modes.
                Padding = PaddingMode.PKCS7,
                Key = keyArray, //set the secret key for the tripleDES algorithm
                IV = ivArray
            };
            
            var cTransform = tdes.CreateEncryptor();

            //transform the specified region of bytes array to resultArray
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public string Decrypt(string cipherString, string key, string iv)
        {
            //get the byte code of the string
            var toEncryptArray = Convert.FromBase64String(cipherString);

            var hash = new SHA512CryptoServiceProvider();
            var keyArray = hash.ComputeHash(Encoding.UTF8.GetBytes(key));
            var trimmedBytes = new byte[24];
            Buffer.BlockCopy(keyArray, 0, trimmedBytes, 0, 24);
            keyArray = trimmedBytes;

            var ivArray = hash.ComputeHash(Encoding.UTF8.GetBytes(iv));
            var trimmedIvBytes = new byte[8];
            Buffer.BlockCopy(ivArray, 0, trimmedIvBytes, 0, 8);
            ivArray = trimmedIvBytes;

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray, //set the secret key for the tripleDES algorithm
                Mode = CipherMode.CBC, //mode of operation. there are other 4 modes. 
                Padding = PaddingMode.PKCS7, //padding mode(if any extra byte added)
                IV = ivArray
            };

            var cTransform = tdes.CreateDecryptor();

            //transform the specified region of bytes array to resultArray
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return Encoding.UTF8.GetString(resultArray);
        }

        // Used with coldfusion
        public string EncryptWithBase64(string toEncrypt, string key, string iv)
        {
            var toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

            var keyArray = Encoding.UTF8.GetBytes(key);

            var ivArray = Convert.FromBase64String(iv);

            var tdes = new TripleDESCryptoServiceProvider
            {
                Mode = CipherMode.CBC, //mode of operation. there are other 4 modes.
                Padding = PaddingMode.PKCS7,
                Key = keyArray, //set the secret key for the tripleDES algorithm
                IV = ivArray
            };

            var cTransform = tdes.CreateEncryptor();

            //transform the specified region of bytes array to resultArray
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        // Used with coldfusion
        public string DecryptWithBase64(string cipherString, string key, string iv)
        {
            //get the byte code of the string
            var toEncryptArray = Convert.FromBase64String(cipherString);

            var keyArray = Encoding.UTF8.GetBytes(key);

            var ivArray = Convert.FromBase64String(iv);

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray, //set the secret key for the tripleDES algorithm
                Mode = CipherMode.CBC, //mode of operation. there are other 4 modes. 
                Padding = PaddingMode.PKCS7, //padding mode(if any extra byte added)
                IV = ivArray
            };

            var cTransform = tdes.CreateDecryptor();

            //transform the specified region of bytes array to resultArray
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return Encoding.UTF8.GetString(resultArray);
        }
    }
}

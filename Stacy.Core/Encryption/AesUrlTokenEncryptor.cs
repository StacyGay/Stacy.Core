using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Stacy.Core.Encryption
{
	public class AesUrlTokenEncryptor
	{
		public byte[] SaltBytes { get; set; } = { 1,8,3,4,5,2,0,2 };
		public int Iterations { get; set; } = 500;
		public int KeySize { get; set; } = 256;
		public int BlockSize { get; set; } = 128;
		public CipherMode Mode { get; set; } = CipherMode.CBC;

		public string Encrypt(string value, string password)
		{
			var valueBytes = Encoding.UTF8.GetBytes(value);
			var passwordBytes = Encoding.UTF8.GetBytes(password);
			passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

			using (var stream = new MemoryStream())
			using (var aes = new RijndaelManaged())
			{
				aes.KeySize = KeySize;
				aes.BlockSize = BlockSize;

				var key = new Rfc2898DeriveBytes(passwordBytes, SaltBytes, Iterations);
				aes.Key = key.GetBytes(aes.KeySize / 8);
				aes.IV = key.GetBytes(aes.BlockSize / 8);
				aes.Mode = CipherMode.CBC;

				using (var cryptoStream = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write))
				{
					cryptoStream.Write(valueBytes, 0, valueBytes.Length);
					cryptoStream.Close();
				}

				//return HttpServerUtility.UrlTokenEncode(stream.ToArray());
				return Convert.ToBase64String(stream.ToArray());
			}
		}

		public string Decrypt(string value, string password)
		{
			var valueBytes = Convert.FromBase64String(value);
			if (valueBytes == null)
				throw new ArgumentException("Incorrectly encoded token value");

			var passwordBytes = Encoding.UTF8.GetBytes(password);
			passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

			using (var stream = new MemoryStream())
			using (var aes = new RijndaelManaged())
			{
				aes.KeySize = KeySize;
				aes.BlockSize = BlockSize;

				var key = new Rfc2898DeriveBytes(passwordBytes, SaltBytes, Iterations);
				aes.Key = key.GetBytes(aes.KeySize / 8);
				aes.IV = key.GetBytes(aes.BlockSize / 8);
				aes.Mode = CipherMode.CBC;

				using (var cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Write))
				{
					cryptoStream.Write(valueBytes, 0, valueBytes.Length);
					cryptoStream.Close();
				}

				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		private byte[] GetRandomBytes()
		{
			const int saltSize = 4;
			var ba = new byte[saltSize];
			RandomNumberGenerator.Create().GetBytes(ba);
			return ba;
		}
	}
}

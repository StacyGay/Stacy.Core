using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;

namespace Stacy.Core.Data
{
	public class StringCompressor
	{
		public static string Zip(string text)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(text);
			var ms = new MemoryStream();
			using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
			{
				zip.Write(buffer, 0, buffer.Length);
			}

			ms.Position = 0;
			var outStream = new MemoryStream();

			var compressed = new byte[ms.Length];
			ms.Read(compressed, 0, compressed.Length);

			var gzBuffer = new byte[compressed.Length + 4];
			Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
			//return HttpServerUtility.UrlTokenEncode(gzBuffer);
			return Convert.ToBase64String(gzBuffer);
		}

		public static string Unzip(string compressedText)
		{
			//byte[] gzBuffer = HttpServerUtility.UrlTokenDecode(compressedText);
			byte[] gzBuffer = Convert.FromBase64String(compressedText);
			using (var ms = new MemoryStream())
			{
				int msgLength = BitConverter.ToInt32(gzBuffer, 0);
				ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

				var buffer = new byte[msgLength];

				ms.Position = 0;
				using (var zip = new GZipStream(ms, CompressionMode.Decompress))
				{
					zip.Read(buffer, 0, buffer.Length);
				}

				return Encoding.UTF8.GetString(buffer);
			}
		}
	}
}
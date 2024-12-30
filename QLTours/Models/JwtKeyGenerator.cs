using System.Security.Cryptography;

namespace QLTours.Models
{
	public class JwtKeyGenerator
	{
		public static string GenerateSecureKey()
		{
			using (var randomBytesGenerator = new RNGCryptoServiceProvider())
			{
				var keyBytes = new byte[32]; // 32 bytes = 256 bits
				randomBytesGenerator.GetBytes(keyBytes);
				return Convert.ToBase64String(keyBytes); // Chuyển đổi sang Base64 cho an toàn
			}
		}
	}
}

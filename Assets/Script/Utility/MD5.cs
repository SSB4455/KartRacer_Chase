/*
SSBB4455 2020-12-23
*/
using System.Text;

namespace UnityStandardAssets.Utility
{
	public class MD5
	{



		public static string GenerateMD5(string str)
		{
			System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
			byte[] byteOld = Encoding.UTF8.GetBytes(str);
			byte[] byteNew = md5.ComputeHash(byteOld);
			StringBuilder sb = new StringBuilder();
			foreach (byte b in byteNew)
			{
				sb.Append(b.ToString("x2"));
			}
			return sb.ToString();
		}

	}
}
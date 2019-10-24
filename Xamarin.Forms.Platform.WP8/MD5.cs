using System.Security.Cryptography;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal abstract class MD5 : HashAlgorithm
	{
		public MD5()
		{
			HashSizeValue = 128;
		}
	}
}
using System.IO;

namespace Microsoft.Maui
{
	public interface IHashAlgorithm
	{
		byte[] ComputeHash(byte[] input);

		byte[] ComputeHash(Stream inputStream);

		string ComputeHashString(string input);
	}
}
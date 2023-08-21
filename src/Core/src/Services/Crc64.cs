// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace Microsoft.Maui
{
	public static class Crc64
	{
		public static byte[] ComputeHash(byte[] input)
		{
			using var algorithm = new Crc64HashAlgorithm();
			return algorithm.ComputeHash(input);
		}

		public static byte[] ComputeHash(Stream inputStream)
		{
			using var algorithm = new Crc64HashAlgorithm();
			return algorithm.ComputeHash(inputStream);
		}

		public static string ComputeHashString(string input)
		{
			using var algorithm = new Crc64HashAlgorithm();
			return algorithm.ComputeHashString(input);
		}
	}
}
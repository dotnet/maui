// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
namespace Microsoft.Maui.Platform
{
	internal static class StringExtensions
	{
		public static string? TrimToMaxLength(this string? currentText, int maxLength) =>
			maxLength >= 0 && currentText?.Length > maxLength
				? currentText.Substring(0, maxLength)
				: currentText;
	}
}
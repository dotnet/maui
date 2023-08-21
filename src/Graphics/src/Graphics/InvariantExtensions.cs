// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
	internal static class InvariantExtensions
	{
		public static string ToInvariantString(this char target)
		{
			return target.ToString();
		}

		public static string ToInvariantString(this int target)
		{
			return target.ToString(CultureInfo.InvariantCulture);
		}

		public static bool EqualsIgnoresCase(this string target, string value)
		{
			return target.Equals(value, StringComparison.OrdinalIgnoreCase);
		}
	}
}

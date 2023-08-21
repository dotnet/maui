// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Platform
{
	[PortHandler]
	public static class UnitExtensions
	{
		public const float EmCoefficient = 0.0624f;

		public static float ToEm(this double pt)
		{
			return (float)pt * EmCoefficient; //Coefficient for converting Pt to Em
		}
	}
}
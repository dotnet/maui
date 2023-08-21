// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Enumerates possible layout directions.
	/// </summary>
	public enum LayoutDirection
	{
		/// <summary>The requested layout direction is unknown.</summary>
		Unknown,

		/// <summary>The requested layout direction is left-to-right.</summary>
		LeftToRight,

		/// <summary>The requested layout direction is right-to-left.</summary>
		RightToLeft
	}
}

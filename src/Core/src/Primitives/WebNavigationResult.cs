// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Enumerates values that indicate the outcome of a web navigation.
	/// </summary>
	public enum WebNavigationResult
	{
		/// <summary>
		/// The navigation succeeded.
		/// </summary>
		Success = 1,
		/// <summary>
		/// The navigation was cancelled.
		/// </summary>
		Cancel = 2,
		/// <summary>
		/// The navigation timed out.
		/// </summary>
		Timeout = 3,
		/// <summary>
		/// The navigation failed.
		/// </summary>
		Failure = 4
	}
}

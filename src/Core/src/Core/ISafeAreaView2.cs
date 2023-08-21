// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality for the Page's SafeAreaInsets that may be changed in the future.
	/// </summary>
	/// <remarks>
	/// This interface is only recognized on the iOS/Mac Catalyst platforms; other platforms will ignore it.
	/// </remarks>
	internal interface ISafeAreaView2
	{
		/// <summary>
		/// Internal property for the Page's SafeAreaInsets Thickness that may be changed in the future.
		/// </summary>
		internal Thickness SafeAreaInsets { set; }
	}
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Xaml.UnitTests.InternalsVisibleAssembly
{
	internal static class Gh16923InternalLibrary
	{
		internal const string InternalLibraryConstant = nameof(InternalLibraryConstant);

		internal static class Nested
		{
			internal const string InternalNestedLibraryConstant = nameof(InternalNestedLibraryConstant);
		}
	}
}

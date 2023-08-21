// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly
{
	public static class Gh16923Library
	{
		public const string LibraryConstant = nameof(LibraryConstant);

		public static class Nested
		{
			public const string NestedLibraryConstant = nameof(NestedLibraryConstant);
		}
	}
}

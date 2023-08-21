// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the properties for a row in a GridLayout.
	/// </summary>
	public interface IGridRowDefinition
	{
		/// <summary>
		/// Gets the height of the row.
		/// </summary>
		GridLength Height { get; }
	}
}
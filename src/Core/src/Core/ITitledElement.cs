// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Represent the title content used in Navigation Views.
	/// </summary>
	public interface ITitledElement : IElement
	{
		/// <summary>
		/// Gets the title of this element.
		/// </summary>
		string? Title { get; }
	}
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IMenuElement : IElement, IImageSourcePart, IText
	{
		/// <summary>
		/// Represents a shortcut key for a MenuItem.
		/// </summary>
#if NETSTANDARD2_0
		IReadOnlyList<IAccelerator>? Accelerators { get; }
#else
		IReadOnlyList<IAccelerator>? Accelerators => null;
#endif

		/// <summary>
		/// Gets a value indicating whether this View is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Occurs when the IMenuElement is clicked.
		/// </summary>
		void Clicked();
	}
}

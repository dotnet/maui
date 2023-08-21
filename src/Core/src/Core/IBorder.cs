// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui;

/// <summary>
/// Provides functionality to provide a border.
/// </summary>
public interface IBorder
{
	/// <summary>
	///  Define how the Shape outline is painted.
	/// </summary>
	IBorderStroke Border { get; }
}

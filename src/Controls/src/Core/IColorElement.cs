// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	interface IColorElement
	{
		//note to implementor: implement this property publicly
		Color Color { get; }
	}
}
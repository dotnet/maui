// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
	internal interface IControlsElement : Maui.IElement
	{
		event EventHandler<HandlerChangingEventArgs>? HandlerChanging;
		event EventHandler? HandlerChanged;
	}
}

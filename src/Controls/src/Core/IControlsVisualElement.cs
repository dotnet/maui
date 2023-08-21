// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
	internal interface IControlsVisualElement : IControlsElement, IView
	{
		event EventHandler? WindowChanged;
		Window? Window { get; }
		event EventHandler? PlatformContainerViewChanged;
	}
}

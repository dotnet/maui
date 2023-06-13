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

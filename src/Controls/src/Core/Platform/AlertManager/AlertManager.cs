// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls.Platform
{
	internal partial class AlertManager
	{
		readonly Window _window;

		public AlertManager(Window window)
		{
			_window = window;
		}

		public void Subscribe() => Subscribe(_window);

		public void Unsubscribe() => Unsubscribe(_window);

	}
}

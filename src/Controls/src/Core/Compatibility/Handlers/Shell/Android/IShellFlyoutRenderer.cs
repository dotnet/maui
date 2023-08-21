// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellFlyoutRenderer
	{
		AView AndroidView { get; }

		void AttachFlyout(IShellContext context, AView content);
	}
}
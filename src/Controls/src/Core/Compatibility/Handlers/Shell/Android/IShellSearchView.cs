// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellSearchView : IDisposable
	{
		AView View { get; }

		SearchHandler SearchHandler { get; set; }

		void LoadView();

		event EventHandler SearchConfirmed;
	}
}
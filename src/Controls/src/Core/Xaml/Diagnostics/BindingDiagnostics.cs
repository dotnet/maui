#nullable disable
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics
{
	/// <summary>
	/// Provides diagnostic events for debugging data binding failures.
	/// </summary>
	public class BindingDiagnostics
	{
		// Backed by WeakEventManager so instance subscribers that forget to
		// unsubscribe are not pinned by the multicast delegate for the process
		// lifetime. See https://github.com/dotnet/maui/issues/37245.
		static readonly WeakEventManager _weakEventManager = new WeakEventManager();

		public static event EventHandler<BindingBaseErrorEventArgs> BindingFailed
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		internal static void SendBindingFailure(BindingBase binding, string errorCode, string message, params object[] messageArgs)
		{
			if (RuntimeFeature.EnableMauiDiagnostics == false)
			{
				return;
			}
			Application.Current?.FindMauiContext()?.CreateLogger<BindingDiagnostics>()?.LogWarning(message, messageArgs);
			_weakEventManager.HandleEvent(null, new BindingBaseErrorEventArgs(VisualDiagnostics.GetSourceInfo(binding), binding, errorCode, message, messageArgs), nameof(BindingFailed));
		}

		internal static void SendBindingFailure(BindingBase binding, object source, BindableObject bo, BindableProperty bp, string errorCode, string message, params object[] messageArgs)
		{
			if (RuntimeFeature.EnableMauiDiagnostics == false)
			{
				return;
			}
			Application.Current?.FindMauiContext()?.CreateLogger<BindingDiagnostics>()?.LogWarning(message, messageArgs);
			_weakEventManager.HandleEvent(null, new BindingErrorEventArgs(VisualDiagnostics.GetSourceInfo(binding), binding, source, bo, bp, errorCode, message, messageArgs), nameof(BindingFailed));
		}
	}
}

#nullable disable
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingDiagnostics.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.Diagnostics.BindingDiagnostics']/Docs/*" />
	public class BindingDiagnostics
	{
		public static event EventHandler<BindingBaseErrorEventArgs> BindingFailed;

		internal static void SendBindingFailure(BindingBase binding, string errorCode, string message, params object[] messageArgs)
		{
			if (RuntimeFeature.EnableMauiDiagnostics == false)
			{
				return;
			}
			Application.Current?.FindMauiContext()?.CreateLogger<BindingDiagnostics>()?.LogWarning(message, messageArgs);
			BindingFailed?.Invoke(null, new BindingBaseErrorEventArgs(VisualDiagnostics.GetSourceInfo(binding), binding, errorCode, message, messageArgs));
		}

		internal static void SendBindingFailure(BindingBase binding, object source, BindableObject bo, BindableProperty bp, string errorCode, string message, params object[] messageArgs)
		{
			if (RuntimeFeature.EnableMauiDiagnostics == false)
			{
				return;
			}
			Application.Current?.FindMauiContext()?.CreateLogger<BindingDiagnostics>()?.LogWarning(message, messageArgs);
			BindingFailed?.Invoke(null, new BindingErrorEventArgs(VisualDiagnostics.GetSourceInfo(binding), binding, source, bo, bp, errorCode, message, messageArgs));
		}
	}
}

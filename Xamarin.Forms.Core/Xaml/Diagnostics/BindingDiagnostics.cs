// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml.Diagnostics
{
	public class BindingDiagnostics
	{
		public static event EventHandler<BindingBaseErrorEventArgs> BindingFailed;

		internal static void SendBindingFailure(BindingBase binding, string errorCode, string message, params object[] messageArgs)
		{
			Log.Warning(errorCode, message, messageArgs);
			BindingFailed?.Invoke(null, new BindingBaseErrorEventArgs(VisualDiagnostics.GetXamlSourceInfo(binding), binding, errorCode, message, messageArgs));
		}

		internal static void SendBindingFailure(BindingBase binding, object source, BindableObject bo, BindableProperty bp, string errorCode, string message, params object[] messageArgs)
		{
			Log.Warning(errorCode, message, messageArgs);
			BindingFailed?.Invoke(null, new BindingErrorEventArgs(VisualDiagnostics.GetXamlSourceInfo(binding), binding, source, bo, bp, errorCode, message, messageArgs));
		}
	}
}

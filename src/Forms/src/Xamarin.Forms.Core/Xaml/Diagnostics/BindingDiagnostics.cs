// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Xamarin.Forms.Xaml.Diagnostics
{
	public class BindingDiagnostics
	{
		internal static void SendBindingFailure(BindingBase binding, string errorCode, string message, object[] messageArgs) =>
			BindingFailed?.Invoke(binding, new BindingBaseErrorEventArgs(VisualDiagnostics.GetXamlSourceInfo(binding), binding, errorCode, message, messageArgs));

		internal static void SendBindingFailure(BindingBase binding, object source, BindableObject bo, BindableProperty bp, string errorCode, string message, object[] messageArgs) =>
			BindingFailed?.Invoke(binding, new BindingErrorEventArgs(VisualDiagnostics.GetXamlSourceInfo(binding), binding, source, bo, bp, errorCode, message, messageArgs));

		public static event EventHandler<BindingBaseErrorEventArgs> BindingFailed;
	}
}

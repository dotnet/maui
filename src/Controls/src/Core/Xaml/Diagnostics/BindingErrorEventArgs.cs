#nullable disable
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics
{
	/// <summary>
	/// Event arguments containing information about a binding failure.
	/// </summary>
	public class BindingBaseErrorEventArgs : EventArgs
	{
		internal BindingBaseErrorEventArgs(SourceInfo xamlSourceInfo, BindingBase binding, string errorCode, string message, object[] messageArgs)
		{
			XamlSourceInfo = xamlSourceInfo;
			Binding = binding;
			ErrorCode = errorCode;
			Message = message;
			MessageArgs = messageArgs;
		}

		/// <summary>Gets the XAML source location where the binding was defined.</summary>
		public SourceInfo XamlSourceInfo { get; }
		/// <summary>Gets the binding that failed.</summary>
		public BindingBase Binding { get; }
		/// <summary>Gets the error code identifying the type of failure.</summary>
		public string ErrorCode { get; }
		/// <summary>Gets the error message describing the failure.</summary>
		public string Message { get; }
		/// <summary>Gets the arguments for formatting the error message.</summary>
		public object[] MessageArgs { get; }
	}

	/// <summary>
	/// Extended event arguments for binding failures with source and target information.
	/// </summary>
	public class BindingErrorEventArgs : BindingBaseErrorEventArgs
	{
		internal BindingErrorEventArgs(
			SourceInfo xamlSourceInfo,
			BindingBase binding,
			object bindingsource,
			BindableObject target,
			BindableProperty property,
			string errorCode,
			string message,
			object[] messageArgs) : base(xamlSourceInfo, binding, errorCode, message, messageArgs)
		{
			Source = bindingsource;
			Target = target;
			TargetProperty = property;
		}

		/// <summary>Gets the binding source object.</summary>
		public object Source { get; }
		/// <summary>Gets the target object that the binding was applied to.</summary>
		public BindableObject Target { get; }
		/// <summary>Gets the target property that the binding was applied to.</summary>
		public BindableProperty TargetProperty { get; }
	}
}

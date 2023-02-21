#nullable disable
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingBaseErrorEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.Diagnostics.BindingBaseErrorEventArgs']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingBaseErrorEventArgs.xml" path="//Member[@MemberName='XamlSourceInfo']/Docs/*" />
		public SourceInfo XamlSourceInfo { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingBaseErrorEventArgs.xml" path="//Member[@MemberName='Binding']/Docs/*" />
		public BindingBase Binding { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingBaseErrorEventArgs.xml" path="//Member[@MemberName='ErrorCode']/Docs/*" />
		public string ErrorCode { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingBaseErrorEventArgs.xml" path="//Member[@MemberName='Message']/Docs/*" />
		public string Message { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingBaseErrorEventArgs.xml" path="//Member[@MemberName='MessageArgs']/Docs/*" />
		public object[] MessageArgs { get; }
	}

	/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingErrorEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.Diagnostics.BindingErrorEventArgs']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingErrorEventArgs.xml" path="//Member[@MemberName='Source']/Docs/*" />
		public object Source { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingErrorEventArgs.xml" path="//Member[@MemberName='Target']/Docs/*" />
		public BindableObject Target { get; }
		/// <include file="../../../../docs/Microsoft.Maui.Controls.Xaml.Diagnostics/BindingErrorEventArgs.xml" path="//Member[@MemberName='TargetProperty']/Docs/*" />
		public BindableProperty TargetProperty { get; }
	}
}

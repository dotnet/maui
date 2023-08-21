// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls.Xaml
{
	public interface IMarkupExtension<out T> : IMarkupExtension
	{
		new T ProvideValue(IServiceProvider serviceProvider);
	}

	public interface IMarkupExtension
	{
		object ProvideValue(IServiceProvider serviceProvider);
	}

	/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/AcceptEmptyServiceProviderAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.AcceptEmptyServiceProviderAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class AcceptEmptyServiceProviderAttribute : Attribute
	{
	}
}
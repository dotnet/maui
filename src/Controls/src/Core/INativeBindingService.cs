// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls.Xaml.Internals
{

	public interface INativeBindingService
	{
		bool TrySetBinding(object target, string propertyName, BindingBase binding);
		bool TrySetBinding(object target, BindableProperty property, BindingBase binding);
		bool TrySetValue(object target, BindableProperty property, object value);
	}
}
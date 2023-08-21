// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	internal class ValueViewContainer<T> : ViewContainer<T> where T : View
	{
		public ValueViewContainer(Enum formsMember, T view, string bindingPath, Func<object, object> converterAction) : base(formsMember, view)
		{

			var valueLabel = new Label { BindingContext = View };
			valueLabel.SetBinding(Label.TextProperty, bindingPath, converter: new GenericValueConverter(converterAction));

			ContainerLayout.Children.Add(valueLabel);
		}
	}
}
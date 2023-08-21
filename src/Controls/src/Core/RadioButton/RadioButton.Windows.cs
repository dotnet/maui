// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.UI.Xaml;
using Windows.Foundation;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			if (radioButton.ResolveControlTemplate() != null)
			{
				handler.PlatformView.Style =
					UI.Xaml.Application.Current.Resources["RadioButtonControlStyle"] as UI.Xaml.Style;
			}
			else
			{
				handler.PlatformView.ClearValue(FrameworkElement.StyleProperty);
			}

			RadioButtonHandler.MapContent(handler, radioButton);
		}
	}
}

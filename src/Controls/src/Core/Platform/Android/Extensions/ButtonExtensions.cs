#nullable disable
using System;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Google.Android.Material.Button;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;
using static Microsoft.Maui.Controls.Button;
using AButton = AndroidX.AppCompat.Widget.AppCompatButton;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this MaterialButton platformButton, Button button)
		{
			var text = TextTransformUtilites.GetTransformedText(button.Text, button.TextTransform);
			platformButton.Text = text;

			// Content layout depends on whether or not the text is empty; changing the text means
			// we may need to update the content layout
			platformButton.UpdateContentLayout(button);
		}

		public static void UpdateContentLayout(this MaterialButton materialButton, Button button)
		{
			var context = materialButton.Context;
			if (context == null)
				return;

			var icon = materialButton.Icon ??
						TextViewCompat.GetCompoundDrawablesRelative(materialButton)[3];

			if (icon != null &&
				!String.IsNullOrEmpty(button.Text))
			{
				var contentLayout = button.ContentLayout;

				// IconPadding calls materialButton.CompoundDrawablePadding				
				// Which is why we don't have to worry about calling setCompoundDrawablePadding
				// ourselves for our custom implemented IconGravityBottom
				materialButton.IconPadding = (int)context.ToPixels(contentLayout.Spacing);

				// For IconGravityTextEnd and IconGravityTextStart, setting the Icon twice
				// is needed to work around the Android behavior that caused
				// https://github.com/dotnet/maui/issues/11755
				switch (contentLayout.Position)
				{
					case ButtonContentLayout.ImagePosition.Top:
						materialButton.Icon = null;
						materialButton.IconGravity = MaterialButton.IconGravityTop;
						materialButton.Icon = icon;
						break;
					case ButtonContentLayout.ImagePosition.Bottom:
						materialButton.Icon = null;
						TextViewCompat.SetCompoundDrawablesRelative(materialButton, null, null, null, icon);
						materialButton.IconGravity = MauiMaterialButton.IconGravityBottom;
						break;
					case ButtonContentLayout.ImagePosition.Left:
						materialButton.Icon = null;
						materialButton.IconGravity = MaterialButton.IconGravityTextStart;
						materialButton.Icon = icon;
						break;
					case ButtonContentLayout.ImagePosition.Right:
						materialButton.Icon = null;
						materialButton.IconGravity = MaterialButton.IconGravityTextEnd;
						materialButton.Icon = icon;
						break;
				}
			}
			else
			{
				// Don't remove this otherwise the button occasionally measures wrong
				// on first load
				materialButton.Icon = icon;
				materialButton.IconPadding = 0;
				materialButton.IconGravity = MaterialButton.IconGravityTextStart;
			}
		}

		public static void UpdateLineBreakMode(this AButton nativeControl, Button button)
		{
			nativeControl.SetLineBreakMode(button.LineBreakMode);
		}
	}
}
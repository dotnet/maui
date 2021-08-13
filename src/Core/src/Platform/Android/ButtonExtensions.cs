using System;
using Android.Content;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Google.Android.Material.Button;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this AppCompatButton appCompatButton, IButton button) =>
			appCompatButton.Text = button.Text;

		public static void UpdatePadding(this AppCompatButton appCompatButton, IButton button, Thickness? defaultPadding = null)
		{
			var context = appCompatButton.Context;
			if (context == null)
				return;

			// TODO: have a way to use default padding
			//       Windows keeps the default as a base but this is also wrong.
			// var padding = defaultPadding ?? new Thickness();
			var padding = new Thickness();
			padding.Left += context.ToPixels(button.Padding.Left);
			padding.Top += context.ToPixels(button.Padding.Top);
			padding.Right += context.ToPixels(button.Padding.Right);
			padding.Bottom += context.ToPixels(button.Padding.Bottom);

			appCompatButton.SetPadding(
				(int)padding.Left,
				(int)padding.Top,
				(int)padding.Right,
				(int)padding.Bottom);
		}

		public static void UpdateContentLayout(this MaterialButton materialButton, IButton button)
		{
			var context = materialButton.Context;
			if (context == null)
				return;

			var icon = materialButton.Icon ??
						TextViewCompat.GetCompoundDrawablesRelative(materialButton)[3];

			if (icon != null &&
				!String.IsNullOrEmpty(button.Text) &&
				button is IButtonContentLayout cl)
			{
				var contentLayout = cl.ContentLayout;

				// IconPadding calls materialButton.CompoundDrawablePadding				
				// Which is why we don't have to worry about calling setCompoundDrawablePadding
				// ourselves for our custom implemented IconGravityBottom
				materialButton.IconPadding = (int)context.ToPixels(contentLayout.Spacing);

				switch (contentLayout.Position)
				{
					case ButtonContentLayout.ImagePosition.Top:
						materialButton.Icon = icon;
						materialButton.IconGravity = MaterialButton.IconGravityTop;
						break;
					case ButtonContentLayout.ImagePosition.Bottom:
						materialButton.Icon = null;
						TextViewCompat.SetCompoundDrawablesRelative(materialButton, null, null, null, icon);
						icon?.SetBounds(0, 0, icon.IntrinsicWidth, icon.IntrinsicHeight);
						materialButton.IconGravity = MauiMaterialButton.IconGravityBottom;
						break;
					case ButtonContentLayout.ImagePosition.Left:
						materialButton.Icon = icon;
						materialButton.IconGravity = MaterialButton.IconGravityStart;
						break;
					case ButtonContentLayout.ImagePosition.Right:
						materialButton.Icon = icon;
						materialButton.IconGravity = MaterialButton.IconGravityEnd;
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
	}
}
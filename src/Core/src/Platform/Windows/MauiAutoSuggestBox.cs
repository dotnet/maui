#nullable enable
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class MauiAutoSuggestBox
	{
		public static void InvalidateAttachedProperties(DependencyObject obj)
		{
			OnIsReadOnlyPropertyChanged(obj);
		}

		// IsReadOnly

		public static bool GetIsReadOnly(DependencyObject obj) =>
			(bool)obj.GetValue(IsReadOnlyProperty);

		public static void SetIsReadOnly(DependencyObject obj, bool value) =>
			obj.SetValue(IsReadOnlyProperty, value);

		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.RegisterAttached(
			"IsReadOnly", typeof(bool), typeof(MauiTextBox),
			new PropertyMetadata(false, OnIsReadOnlyPropertyChanged));

		static void OnIsReadOnlyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs? e = null)
		{
			var element = d as FrameworkElement;

			if (element is null)
			{
				return;
			}

			bool isReadOnly = e?.NewValue is bool val ? val : GetIsReadOnly(d);

			Action applyIsReadOnly = () =>
			{
				var textBox = element.GetDescendantByName<TextBox>("TextBox");

				if (textBox is not null)
				{
					textBox.IsReadOnly = isReadOnly;
				}
			};

			if (element.IsLoaded)
			{
				applyIsReadOnly();
			}
			else
			{
				element.OnLoaded(applyIsReadOnly);
			}
		}
	}
}
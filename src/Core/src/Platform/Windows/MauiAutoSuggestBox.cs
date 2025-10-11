#nullable enable
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
			new PropertyMetadata(true, OnIsReadOnlyPropertyChanged));

		static void OnIsReadOnlyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs? e = null)
		{
			var element = d as FrameworkElement;
			var textBox = element?.GetDescendantByName<TextBox>("TextBox");
			textBox?.IsReadOnly = true;
		}
	}
}
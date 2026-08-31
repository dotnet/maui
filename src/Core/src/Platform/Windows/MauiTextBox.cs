using System;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public static class MauiTextBox
	{
		const string ContentElementName = "ContentElement";
		const string PlaceholderTextContentPresenterName = "PlaceholderTextContentPresenter";
		const string DeleteButtonElementName = "DeleteButton";
		static readonly ConditionalWeakTable<FrameworkElement, TextBoxTemplateCache> s_templateCaches = new();

		sealed class TextBoxTemplateCache
		{
			public WeakReference<ScrollViewer>? ContentElement { get; set; }
		}

		public static void InvalidateAttachedProperties(DependencyObject obj)
		{
			OnVerticalTextAlignmentPropertyChanged(obj);
			OnIsDeleteButtonEnabledPropertyChanged(obj);
		}

		// VerticalTextAlignment

		public static VerticalAlignment GetVerticalTextAlignment(DependencyObject obj) =>
			(VerticalAlignment)obj.GetValue(VerticalTextAlignmentProperty);

		public static void SetVerticalTextAlignment(DependencyObject obj, VerticalAlignment value) =>
			obj.SetValue(VerticalTextAlignmentProperty, value);

		public static readonly DependencyProperty VerticalTextAlignmentProperty = DependencyProperty.RegisterAttached(
			"VerticalTextAlignment", typeof(VerticalAlignment), typeof(MauiTextBox),
			new PropertyMetadata(VerticalAlignment.Center, OnVerticalTextAlignmentPropertyChanged));

		static void OnVerticalTextAlignmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs? e = null)
		{
			if (d is not FrameworkElement element)
			{
				return;
			}

			var verticalAlignment = GetVerticalTextAlignment(d);

			var scrollViewer = GetContentElement(element);
			scrollViewer?.VerticalAlignment = verticalAlignment;

			var placeholder = element.GetDescendantByName<TextBlock>(PlaceholderTextContentPresenterName);
			placeholder?.VerticalAlignment = verticalAlignment;
		}

		static ScrollViewer? GetContentElement(FrameworkElement element)
		{
			var cache = s_templateCaches.GetOrCreateValue(element);

			if (cache.ContentElement?.TryGetTarget(out var contentElement) == true &&
				IsDescendantOf(contentElement, element))
			{
				return contentElement;
			}

			contentElement = element.GetDescendantByName<ScrollViewer>(ContentElementName);
			cache.ContentElement = contentElement is null ? null : new(contentElement);
			return contentElement;
		}

		static bool IsDescendantOf(DependencyObject element, DependencyObject ancestor)
		{
			var current = VisualTreeHelper.GetParent(element);
			while (current is not null)
			{
				if (current == ancestor)
					return true;

				current = VisualTreeHelper.GetParent(current);
			}

			return false;
		}

		public static bool GetIsDeleteButtonEnabled(DependencyObject obj) =>
			(bool)obj.GetValue(IsDeleteButtonEnabledProperty);

		public static void SetIsDeleteButtonEnabled(DependencyObject obj, bool value) =>
			obj.SetValue(IsDeleteButtonEnabledProperty, value);

		public static readonly DependencyProperty IsDeleteButtonEnabledProperty = DependencyProperty.RegisterAttached(
			"IsDeleteButtonEnabled", typeof(bool), typeof(MauiTextBox),
			new PropertyMetadata(true, OnIsDeleteButtonEnabledPropertyChanged));

		static void OnIsDeleteButtonEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs? e = null)
		{
			if (d is not FrameworkElement element)
			{
				return;
			}

			Button? deleteButton = element.GetDescendantByName<Button>(DeleteButtonElementName);

			// Adjust the second column's width to 'Auto' when the delete button is enabled, and set it to zero when disabled.
			// Disables the delete button when ClearButtonVisibility is set to Never, and enables it otherwise.
			// In WinUI, they set the opacity to '0' when the button is disabled. Here, we use IsEnabled to manage visibility.
			if (deleteButton?.Parent is Grid rootGrid && rootGrid.ColumnDefinitions.Count > 1)
			{
				int deleteButtonColumnIndex = Grid.GetColumn(deleteButton);
				if (GetIsDeleteButtonEnabled(element))
				{
					rootGrid.ColumnDefinitions[deleteButtonColumnIndex].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Auto);
					deleteButton.IsEnabled = true;
				}
				else
				{
					rootGrid.ColumnDefinitions[deleteButtonColumnIndex].Width = new UI.Xaml.GridLength(0);
					deleteButton.IsEnabled = false;
				}
			}
		}
	}
}
﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public static class MauiTextBox
	{
		const string ContentElementName = "ContentElement";
		const string PlaceholderTextContentPresenterName = "PlaceholderTextContentPresenter";
		const string DeleteButtonElementName = "DeleteButton";

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
			// TODO: cache the scrollViewer value on the textBox

			var element = d as FrameworkElement;
			var verticalAlignment = GetVerticalTextAlignment(d);

			var scrollViewer = element?.GetDescendantByName<ScrollViewer>(ContentElementName);
			if (scrollViewer is not null)
			{
				scrollViewer.VerticalAlignment = verticalAlignment;
			}

			var placeholder = element?.GetDescendantByName<TextBlock>(PlaceholderTextContentPresenterName);
			if (placeholder is not null)
			{
				placeholder.VerticalAlignment = verticalAlignment;
			}
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
			if (deleteButton?.Parent is Grid rootGrid && rootGrid.ColumnDefinitions.Count > 1)
			{
				if (GetIsDeleteButtonEnabled(element))
				{
					rootGrid.ColumnDefinitions[1].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Auto);
				}
				else
				{
					rootGrid.ColumnDefinitions[1].Width = new UI.Xaml.GridLength(0);
				}
			}
		}
	}
}
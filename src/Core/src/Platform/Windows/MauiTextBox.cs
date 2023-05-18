#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class MauiTextBox
	{
		const string ContentElementName = "ContentElement";
		const string PlaceholderTextContentPresenterName = "PlaceholderTextContentPresenter";
		const string DeleteButtonElementName = "DeleteButton";
		const string ButtonStatesName = "ButtonStates";
		const string ButtonVisibleStateName = "ButtonVisible";

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
				scrollViewer.VerticalAlignment = verticalAlignment;

			var placeholder = element?.GetDescendantByName<TextBlock>(PlaceholderTextContentPresenterName);
			if (placeholder is not null)
				placeholder.VerticalAlignment = verticalAlignment;
		}

		// IsDeleteButtonEnabled

		public static bool GetIsDeleteButtonEnabled(DependencyObject obj) =>
			(bool)obj.GetValue(IsDeleteButtonEnabledProperty);

		public static void SetIsDeleteButtonEnabled(DependencyObject obj, bool value) =>
			obj.SetValue(IsDeleteButtonEnabledProperty, value);

		public static readonly DependencyProperty IsDeleteButtonEnabledProperty = DependencyProperty.RegisterAttached(
			"IsDeleteButtonEnabled", typeof(bool), typeof(MauiTextBox),
			new PropertyMetadata(true, OnIsDeleteButtonEnabledPropertyChanged));

		static void OnIsDeleteButtonEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs? e = null)
		{
			// TODO: cache the buttonStates and buttonVisibleState values on the textBox

			var element = d as FrameworkElement;

			VisualStateGroup? buttonStates = null;
			VisualState? buttonVisibleState = null;
			var deleteButton = element?.GetDescendantByName<Button>(DeleteButtonElementName);
			if (deleteButton?.Parent is Grid rootGrid)
				(buttonStates, buttonVisibleState) = InterceptDeleteButtonVisualStates(rootGrid);

			var states = buttonStates?.States;
			if (element is not null && states is not null && buttonVisibleState is not null)
			{
				var isEnabled = GetIsDeleteButtonEnabled(element);
				var contains = states.Contains(buttonVisibleState);
				if (isEnabled && !contains)
					states.Add(buttonVisibleState);
				else if (!isEnabled && contains)
					states.Remove(buttonVisibleState);
			}
		}

		static (VisualStateGroup? Group, VisualState? State) InterceptDeleteButtonVisualStates(FrameworkElement? element)
		{
			// not the content we expected
			if (element is null)
				return (null, null);

			// find "ButtonStates"
			var visualStateGroups = VisualStateManager.GetVisualStateGroups(element);
			VisualStateGroup? buttonStates = null;
			foreach (var group in visualStateGroups)
			{
				if (group.Name == ButtonStatesName)
					buttonStates = group;
			}

			// no button states
			if (buttonStates is null)
				return (null, null);

			// find and return the "ButtonVisible" state
			foreach (var state in buttonStates.States)
			{
				if (state.Name == ButtonVisibleStateName)
					return (buttonStates, state);
			}

			// no button visible state
			return (null, null);
		}
	}
}
#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public interface IMauiTextBox
	{
		VerticalAlignment VerticalTextAlignment { get; set; }

		bool IsDeleteButtonEnabled { get; set; }
	}

	public class MauiTextBox : TextBox, IMauiTextBox
	{
		const string ContentElementName = "ContentElement";
		const string DeleteButtonElementName = "DeleteButton";
		const string ButtonStatesName = "ButtonStates";
		const string ButtonVisibleStateName = "ButtonVisible";

		ScrollViewer? _scrollViewer;
		VisualStateGroup? _buttonStates;
		VisualState? _buttonVisibleState;

		public static DependencyProperty VerticalTextAlignmentProperty { get; } = DependencyProperty.Register(
			nameof(VerticalTextAlignment), typeof(VerticalAlignment), typeof(MauiTextBox),
			new PropertyMetadata(VerticalAlignment.Center, OnVerticalTextAlignmentPropertyChanged));

		public static DependencyProperty IsDeleteButtonEnabledProperty { get; } = DependencyProperty.Register(
			nameof(IsDeleteButtonEnabled), typeof(bool), typeof(MauiTextBox),
			new PropertyMetadata(true, OnIsDeleteButtonEnabledPropertyChanged));

		static void OnVerticalTextAlignmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MauiTextBox mauiEntryTextBox)
				mauiEntryTextBox.UpdateVerticalTextAlignment();
		}

		static void OnIsDeleteButtonEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MauiTextBox mauiEntryTextBox)
				mauiEntryTextBox.UpdateIsDeleteButtonEnabled();
		}

		public VerticalAlignment VerticalTextAlignment
		{
			get => (VerticalAlignment)GetValue(VerticalTextAlignmentProperty);
			set => SetValue(VerticalTextAlignmentProperty, value);
		}

		public bool IsDeleteButtonEnabled
		{
			get => (bool)GetValue(IsDeleteButtonEnabledProperty);
			set => SetValue(IsDeleteButtonEnabledProperty, value);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_scrollViewer = GetTemplateChild(ContentElementName) as ScrollViewer;

			var deleteButton = GetTemplateChild(DeleteButtonElementName) as Button;
			if (deleteButton?.Parent is Grid rootGrid)
				(_buttonStates, _buttonVisibleState) = InterceptDeleteButtonVisualStates(rootGrid);

			UpdateVerticalTextAlignment();
			UpdateIsDeleteButtonEnabled();
		}

		void UpdateVerticalTextAlignment()
		{
			if (_scrollViewer is not null)
				_scrollViewer.VerticalAlignment = VerticalTextAlignment;
		}

		void UpdateIsDeleteButtonEnabled()
		{
			var states = _buttonStates?.States;
			if (states is not null && _buttonVisibleState is not null)
			{
				var contains = states.Contains(_buttonVisibleState);
				if (IsDeleteButtonEnabled && !contains)
					states.Add(_buttonVisibleState);
				else if (!IsDeleteButtonEnabled && contains)
					states.Remove(_buttonVisibleState);
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
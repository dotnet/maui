#nullable enable
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using WGridLength = Microsoft.UI.Xaml.GridLength;
using WGridUnitType = Microsoft.UI.Xaml.GridUnitType;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Platform
{
	internal sealed partial class MauiPicker : ComboBox
	{
		Panel? _selectionParent;
		Grid? _selectionHost;
		ContentPresenter? _selectionPresenter;
		Border? _popupBorder;
		FrameworkElement? _popupContent;
		int _popupContentRow;
		bool _clearButtonsEnabled;
		bool _showClearButtons;

		internal MauiPicker()
		{
			Loaded += OnLoaded;
		}

		internal Button? ClearButton { get; private set; }
		internal Button? PopupClearButton { get; private set; }
		internal Action? ClearSelectionRequested { get; set; }

		internal void UpdateClearButtons(IPicker picker)
		{
			_clearButtonsEnabled = picker is IPickerClearButton { IsClearButtonVisible: true };
			_showClearButtons = picker.ShouldShowClearButton();

			if (!_clearButtonsEnabled)
			{
				RemoveClearButtons();
				return;
			}

			// Handler mappings can run before WinUI has resolved the control's theme template.
			if (!IsLoaded)
				return;

			ApplyTemplate();
			AddClearButtons();
			UpdateClearButtonVisibility();
		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (!_clearButtonsEnabled)
				return;

			ApplyTemplate();
			AddClearButtons();
			UpdateClearButtonVisibility();
		}

		internal void DisconnectClearButtons()
		{
			ClearSelectionRequested = null;
			_clearButtonsEnabled = false;
			RemoveClearButtons();
		}

		protected override void OnApplyTemplate()
		{
			RemoveClearButtons();
			base.OnApplyTemplate();

			if (_clearButtonsEnabled && IsLoaded)
			{
				AddClearButtons();
				UpdateClearButtonVisibility();
			}
		}

		void AddClearButtons()
		{
			if (ClearButton is not null)
				return;

			if (GetTemplateChild("ContentPresenter") is not ContentPresenter presenter)
			{
				throw new NotSupportedException(
					"Picker.IsClearButtonVisible requires a ComboBox template with a ContentPresenter named 'ContentPresenter'.");
			}

			if (presenter.Parent is not Panel parent)
			{
				throw new NotSupportedException(
					"Picker.IsClearButtonVisible requires the ComboBox template's ContentPresenter to be a child of a Panel.");
			}

			if (GetTemplateChild("PopupBorder") is not Border popupBorder)
			{
				throw new NotSupportedException(
					"Picker.IsClearButtonVisible requires a ComboBox template with a Border named 'PopupBorder'.");
			}

			if (popupBorder.Child is not FrameworkElement popupContent)
			{
				throw new NotSupportedException(
					"Picker.IsClearButtonVisible requires the ComboBox template's PopupBorder to have a FrameworkElement child.");
			}

			var clearButton = CreateClearButton("MauiPickerClearButton");
			clearButton.SetBinding(ForegroundProperty, new Binding { Source = this, Path = new PropertyPath(nameof(Foreground)) });
			var transparent = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
			clearButton.Background = transparent;
			clearButton.BorderThickness = new WThickness(0);
			clearButton.Resources["ButtonBackground"] = transparent;
			clearButton.Resources["ButtonBackgroundPointerOver"] = transparent;
			clearButton.Resources["ButtonBackgroundPressed"] = transparent;
			clearButton.Resources["ButtonBackgroundDisabled"] = transparent;
			clearButton.Content = new FontIcon
			{
				FontFamily = new FontFamily("Segoe MDL2 Assets"),
				Glyph = "\uE10A",
				FontSize = 12
			};
			var popupButtonTemplate = (DataTemplate)Application.Current.Resources["PickerPopupClearButtonTemplate"];
			var popupClearButton = CreateClearButton("MauiPickerPopupClearButton", (Button)popupButtonTemplate.LoadContent());
			var selectionHost = new Grid();
			selectionHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new WGridLength(1, WGridUnitType.Star) });
			selectionHost.ColumnDefinitions.Add(new ColumnDefinition { Width = WGridLength.Auto });
			Grid.SetRow(selectionHost, Grid.GetRow(presenter));
			Grid.SetColumn(selectionHost, Grid.GetColumn(presenter));
			Grid.SetRowSpan(selectionHost, Grid.GetRowSpan(presenter));
			Grid.SetColumnSpan(selectionHost, Grid.GetColumnSpan(presenter));

			var children = GetChildren(parent);
			var index = children.IndexOf(presenter);
			children.RemoveAt(index);
			Grid.SetRow(presenter, 0);
			Grid.SetColumn(presenter, 0);
			Grid.SetRowSpan(presenter, 1);
			Grid.SetColumnSpan(presenter, 1);
			var selectionChildren = GetChildren(selectionHost);
			selectionChildren.Add(presenter);
			Grid.SetColumn(clearButton, 1);
			selectionChildren.Add(clearButton);
			children.Insert(index, selectionHost);

			var popupHost = new Grid();
			popupHost.RowDefinitions.Add(new RowDefinition { Height = WGridLength.Auto });
			popupHost.RowDefinitions.Add(new RowDefinition { Height = new WGridLength(1, WGridUnitType.Star) });
			popupBorder.Child = null;
			_popupContentRow = Grid.GetRow(popupContent);
			Grid.SetRow(popupContent, 1);
			popupClearButton.HorizontalAlignment = HorizontalAlignment.Stretch;
			popupClearButton.Content = AutomationProperties.GetName(popupClearButton);
			var popupChildren = GetChildren(popupHost);
			popupChildren.Add(popupClearButton);
			popupChildren.Add(popupContent);
			popupBorder.Child = popupHost;

			_selectionParent = parent;
			_selectionHost = selectionHost;
			_selectionPresenter = presenter;
			_popupBorder = popupBorder;
			_popupContent = popupContent;
			ClearButton = clearButton;
			PopupClearButton = popupClearButton;
		}

		Button CreateClearButton(string name, Button? button = null)
		{
			button ??= new Button();
			button.Name = name;
			button.Content = new SymbolIcon(Symbol.Cancel);
			button.MinWidth = 32;
			button.MinHeight = 32;
			button.Padding = new WThickness(4);
			button.VerticalAlignment = VerticalAlignment.Center;
			AutomationProperties.SetAutomationId(button, name);
			AutomationProperties.SetName(button, (string)Application.Current.Resources["PickerClearSelectionAutomationName"]);
			button.SetBinding(IsEnabledProperty, new Binding { Source = this, Path = new PropertyPath(nameof(IsEnabled)) });
			button.Click += OnClearButtonClicked;
			return button;
		}

		void UpdateClearButtonVisibility()
		{
			var visibility = _showClearButtons ? WVisibility.Visible : WVisibility.Collapsed;
			if (ClearButton is not null)
				ClearButton.Visibility = visibility;
			if (PopupClearButton is not null)
				PopupClearButton.Visibility = visibility;
		}

		void OnClearButtonClicked(object sender, RoutedEventArgs e)
		{
			if (!IsEnabled || !_showClearButtons)
				return;

			// Move focus before hiding the button, without closing or reopening the popup.
			Focus(FocusState.Programmatic);
			ClearSelectionRequested?.Invoke();
		}

		void RemoveClearButtons()
		{
			ReleaseButton(ClearButton);
			ReleaseButton(PopupClearButton);

			if (_selectionParent is not null && _selectionHost is not null && _selectionPresenter is not null)
			{
				var children = GetChildren(_selectionParent);
				var index = children.IndexOf(_selectionHost);
				GetChildren(_selectionHost).Clear();
				Grid.SetRow(_selectionPresenter, Grid.GetRow(_selectionHost));
				Grid.SetColumn(_selectionPresenter, Grid.GetColumn(_selectionHost));
				Grid.SetRowSpan(_selectionPresenter, Grid.GetRowSpan(_selectionHost));
				Grid.SetColumnSpan(_selectionPresenter, Grid.GetColumnSpan(_selectionHost));
				children.RemoveAt(index);
				children.Insert(index, _selectionPresenter);
			}

			if (_popupBorder is not null && _popupContent is not null)
			{
				if (_popupBorder.Child is Grid popupHost)
					GetChildren(popupHost).Clear();
				Grid.SetRow(_popupContent, _popupContentRow);
				_popupBorder.Child = _popupContent;
			}

			ClearButton = null;
			PopupClearButton = null;
			_selectionParent = null;
			_selectionHost = null;
			_selectionPresenter = null;
			_popupBorder = null;
			_popupContent = null;
		}

		void ReleaseButton(Button? button)
		{
			if (button is null)
				return;

			button.Click -= OnClearButtonClicked;
			button.ClearValue(IsEnabledProperty);
			button.ClearValue(ForegroundProperty);
		}

		static UIElementCollection GetChildren(Panel panel)
		{
			// Native template panels cannot be replaced with MauiPanel; access only during template setup/cleanup.
#pragma warning disable RS0030
			return panel.Children;
#pragma warning restore RS0030
		}
	}
}

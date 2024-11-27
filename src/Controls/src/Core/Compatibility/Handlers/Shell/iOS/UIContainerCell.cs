#nullable disable
using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class UIContainerCell : UITableViewCell
	{
		IPlatformViewHandler _renderer;
		object _bindingContext;

		internal Action<UIContainerCell> ViewMeasureInvalidated { get; set; }
		internal NSIndexPath IndexPath { get; set; }
		internal UITableView TableView { get; set; }

		internal UIContainerCell(string cellId, View view, Shell shell, object context) : base(UITableViewCellStyle.Default, cellId)
		{
			View = view;
			View.MeasureInvalidated += MeasureInvalidated;
			SelectionStyle = UITableViewCellSelectionStyle.None;

			_renderer = (IPlatformViewHandler)view.Handler;

			if (_renderer == null)
			{
				_renderer = (IPlatformViewHandler)view.ToHandler(view.FindMauiContext() ?? shell.FindMauiContext());
			}

			var platformView = view.ToPlatform();
			ContentView.AddSubview(platformView);
			platformView.AccessibilityTraits |= UIAccessibilityTrait.Button;

			_renderer.PlatformView.ClipsToBounds = true;
			ContentView.ClipsToBounds = true;

			BindingContext = context;

			if (BindingContext is BaseShellItem bsi)
				bsi.AddLogicalChild(View);
			else if (shell != null)
				shell.AddLogicalChild(View);
		}

		public UIContainerCell(string cellId, View view) : this(cellId, view, null, null)
		{
		}

		void MeasureInvalidated(object sender, System.EventArgs e)
		{
			if (View == null || TableView == null)
				return;

			ViewMeasureInvalidated?.Invoke(this);
		}

		internal void ReloadRow()
		{
			TableView.ReloadRows(new[] { IndexPath }, UITableViewRowAnimation.Automatic);
		}

		internal void Disconnect(Shell shell = null, bool keepRenderer = false)
		{
			ViewMeasureInvalidated = null;
			View.MeasureInvalidated -= MeasureInvalidated;
			if (_bindingContext != null && _bindingContext is BaseShellItem baseShell)
				baseShell.PropertyChanged -= OnElementPropertyChanged;

			if (View.Parent is BaseShellItem bsi)
				bsi.RemoveLogicalChild(View);
			else if (shell != null)
				shell.RemoveLogicalChild(View);

			_bindingContext = null;

			if (!keepRenderer)
				View.Handler = null;


			View = null;
			TableView = null;
		}

		public View View { get; private set; }

		public object BindingContext
		{
			get => _bindingContext;
			set
			{
				if (value == _bindingContext)
					return;

				if (_bindingContext != null && _bindingContext is BaseShellItem baseShell)
					baseShell.PropertyChanged -= OnElementPropertyChanged;

				_bindingContext = value;
				View.BindingContext = value;

				if (_bindingContext != null && _bindingContext is BaseShellItem baseShell2)
				{
					baseShell2.PropertyChanged += OnElementPropertyChanged;
					UpdateVisualState();
				}
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (View is IView view)
			{
				_renderer.PlatformView.Frame = Bounds;
				view.Arrange(Bounds.ToRectangle());
			}
		}

		void UpdateVisualState()
		{
			if (BindingContext is BaseShellItem baseShellItem && baseShellItem != null)
			{
				if (baseShellItem.IsChecked)
					VisualStateManager.GoToState(View, "Selected");
				else
					VisualStateManager.GoToState(View, "Normal");
			}
		}

		void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BaseShellItem.IsCheckedProperty.PropertyName)
			{
				UpdateVisualState();
			}
		}
	}
}
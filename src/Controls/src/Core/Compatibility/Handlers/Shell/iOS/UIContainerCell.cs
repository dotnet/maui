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
			platformView.TranslatesAutoresizingMaskIntoConstraints = false;

			var margin = view.Margin;
			var constraints = new NSLayoutConstraint[]
			{
				platformView.LeadingAnchor.ConstraintEqualTo(ContentView.LeadingAnchor, (nfloat)margin.Left),
				platformView.TrailingAnchor.ConstraintEqualTo(ContentView.TrailingAnchor, (nfloat)(-margin.Right)),
				platformView.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, (nfloat)margin.Top),
				platformView.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, (nfloat)(-margin.Bottom))
			};
			NSLayoutConstraint.ActivateConstraints(constraints);

			_renderer.PlatformView.ClipsToBounds = true;
			ContentView.ClipsToBounds = true;

			BindingContext = context;

			if (BindingContext is BaseShellItem bsi)
				bsi.AddLogicalChild(View);
			else
				shell?.AddLogicalChild(View);
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
			else
				shell?.RemoveLogicalChild(View);

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
				var targetState = baseShellItem.IsChecked ? "Selected" : "Normal";
				
				// Force a complete refresh of DynamicResource bindings in visual states.
				// VisualStateManager.GoToState() optimizes by returning early if already in the target state,
				// so DynamicResource changes are not re-applied. We work around this by always transitioning
				// through Normal first, which forces setters to be unapplied and re-applied.
				// This fixes an issue where dynamic resources don't update when cells are reused after
				// flyout is reopened or when application resources change at runtime.
				// See: https://github.com/dotnet/maui/issues/34931
				if (targetState != "Normal")
				{
					VisualStateManager.GoToState(View, "Normal");
				}
				
				// Always apply the target state to handle transitions and ensure proper state
				VisualStateManager.GoToState(View, targetState);
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
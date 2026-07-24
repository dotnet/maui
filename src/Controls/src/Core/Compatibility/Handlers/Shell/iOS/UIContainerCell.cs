#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class UIContainerCell : UITableViewCell
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Renderer is owned by the container cell and disconnected in Disconnect.")]
		IPlatformViewHandler _renderer;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Binding context is unsubscribed from PropertyChanged and cleared in Disconnect.")]
		object _bindingContext;
		IElementDefinition _viewResource;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Measure callback is cleared in Disconnect before the cell is released.")]
		internal Action<UIContainerCell> ViewMeasureInvalidated { get; set; }
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Index path is cleared in Disconnect when the cell is released.")]
		internal NSIndexPath IndexPath { get; set; }
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Table view reference is cleared in Disconnect when the cell is released.")]
		internal UITableView TableView { get; set; }

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "View MeasureInvalidated subscription is removed in Disconnect.")]
		internal UIContainerCell(string cellId, View view, Shell shell, object context) : base(UITableViewCellStyle.Default, cellId)
		{
			View = view;
			_viewResource = view as IElementDefinition;
			_viewResource?.AddResourcesChangedListener(OnResourcesChanged);
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
			_viewResource?.RemoveResourcesChangedListener(OnResourcesChanged);
			_viewResource = null;
			if (_bindingContext != null && _bindingContext is BaseShellItem baseShell)
				baseShell.PropertyChanged -= OnElementPropertyChanged;

			if (View.Parent is BaseShellItem bsi)
				bsi.RemoveLogicalChild(View);
			else
				shell?.RemoveLogicalChild(View);

			_bindingContext = null;

			if (!keepRenderer)
				View.Handler = null;


			_renderer = null;
			IndexPath = null;
			View = null;
			TableView = null;
		}

		public View View { get; private set; }

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "BaseShellItem PropertyChanged subscription is removed when BindingContext changes and in Disconnect.")]
		public object BindingContext
		{
			get => _bindingContext;
			set
			{
				if (value == _bindingContext)
				{
					return;
				}

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
			if (BindingContext is BaseShellItem bsi)
			{
				VisualStateManager.GoToState(View, bsi.IsChecked ? "Selected" : "Normal", force: true);
			}
		}

		void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BaseShellItem.IsCheckedProperty.PropertyName)
				UpdateVisualState();
		}

		void OnResourcesChanged(object sender, ResourcesChangedEventArgs e)
		{
			UpdateVisualState();
		}
	}
}
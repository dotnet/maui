using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[Obsolete]
	public class UIContainerCell : UITableViewCell
	{
		IVisualElementRenderer _renderer;
		object _bindingContext;

		internal Action<UIContainerCell> ViewMeasureInvalidated { get; set; }
		internal NSIndexPath IndexPath { get; set; }
		internal UITableView TableView { get; set; }

		internal UIContainerCell(string cellId, View view, Shell shell, object context) : base(UITableViewCellStyle.Default, cellId)
		{
			View = view;
			View.MeasureInvalidated += MeasureInvalidated;
			SelectionStyle = UITableViewCellSelectionStyle.None;

			_renderer = Platform.GetRenderer(view);

			if (_renderer == null)
			{
				_renderer = Platform.CreateRenderer(view);
				Platform.SetRenderer(view, _renderer);
			}

			ContentView.AddSubview(_renderer.NativeView);
			_renderer.NativeView.ClipsToBounds = true;
			ContentView.ClipsToBounds = true;

			BindingContext = context;
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

			_bindingContext = null;

			if (!keepRenderer)
				Platform.SetRenderer(View, null);

			shell?.RemoveLogicalChild(shell);

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
			View?.Layout(Bounds.ToRectangle());
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
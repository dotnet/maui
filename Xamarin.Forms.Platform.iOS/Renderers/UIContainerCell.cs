using System;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class UIContainerCell : UITableViewCell
	{
		IVisualElementRenderer _renderer;
		object _bindingContext;
		internal Action<UIContainerCell> ViewMeasureInvalidated { get; set; }
		internal NSIndexPath IndexPath { get; set; }
		internal UITableView TableView { get; set; }

		public UIContainerCell(string cellId, View view) : base(UITableViewCellStyle.Default, cellId)
		{
			View = view;
			View.MeasureInvalidated += MeasureInvalidated;
			SelectionStyle = UITableViewCellSelectionStyle.None;

			_renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, _renderer);

			ContentView.AddSubview(_renderer.NativeView);
			_renderer.NativeView.ClipsToBounds = true;
			ContentView.ClipsToBounds = true;
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

		internal void Disconnect()
		{
			ViewMeasureInvalidated = null;
			View.MeasureInvalidated -= MeasureInvalidated;
			if (_bindingContext != null && _bindingContext is BaseShellItem baseShell)
				baseShell.PropertyChanged -= OnElementPropertyChanged;

			_bindingContext = null;
			Platform.SetRenderer(View, null);
			View = null;
			TableView = null;
		}

		public View View { get; private set; }

		public object BindingContext
		{
			get => _bindingContext;
			set {
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
			if(View != null)
				View.Layout(Bounds.ToRectangle());
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
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class UIContainerCell : UITableViewCell
	{
		IVisualElementRenderer _renderer;
		object _bindingContext;

		public UIContainerCell(string cellId, View view) : base(UITableViewCellStyle.Default, cellId)
		{
			View = view;

			SelectionStyle = UITableViewCellSelectionStyle.None;

			_renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, _renderer);

			AddSubview(_renderer.NativeView);
		}

		public View View { get; }

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
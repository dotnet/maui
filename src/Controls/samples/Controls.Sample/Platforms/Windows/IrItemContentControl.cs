using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Controls
{
	public partial class IrItemContentControl : ContentControl
	{
		public IView View { get; private set; }

		internal IrDataWrapper Data { get; private set; }

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Data = DataContext as IrDataWrapper;

			if (View == null)
			{
				View = Data.positionalViewSelector.ViewSelector.CreateView(Data.position, Data.data);

				var frameworkElement = View.ToNative(Data.context);

				Content = frameworkElement;
			}

			this.Loaded += IrItemContentControl_Loaded;
			this.Unloaded += IrItemContentControl_Unloaded;	
			Data.positionalViewSelector?.ViewSelector?.RecycleView(Data.position, Data.data, View);
		}

		private void IrItemContentControl_Unloaded(object sender, UI.Xaml.RoutedEventArgs e)
		{
			Data.virtualListView.ViewSelector.ViewDetached(Data.position, View);
		}

		private void IrItemContentControl_Loaded(object sender, UI.Xaml.RoutedEventArgs e)
		{
			Data.virtualListView.ViewSelector.ViewAttached(Data.position, View);
		}

		protected override void OnTapped(TappedRoutedEventArgs e)
		{
			base.OnTapped(e);

			if (Data?.position != null)
				Data.position.IsSelected = !Data.position.IsSelected;

			var itemPos = new ItemPosition(Data?.position?.SectionIndex ?? 0, Data?.position?.ItemIndex ?? 0);

			if (Data?.position?.IsSelected ?? false)
				Data?.virtualListView?.SetSelected(itemPos);
			else
				Data?.virtualListView?.SetDeselected(itemPos);

			if (Data?.data is IPositionInfo dataPositionInfo)
				dataPositionInfo.SetPositionInfo(Data.position);
		}

	}
}

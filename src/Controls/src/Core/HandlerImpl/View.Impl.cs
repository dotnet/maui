using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.HotReload;

namespace Microsoft.Maui.Controls
{
	public partial class View
	{
		protected PropertyMapper propertyMapper;

		protected PropertyMapper<T> GetRendererOverrides<T>() where T : IView => (PropertyMapper<T>)(propertyMapper as PropertyMapper<T> ?? (propertyMapper = new PropertyMapper<T>()));
		PropertyMapper IPropertyMapperView.GetPropertyMapperOverrides() => propertyMapper;

		Primitives.LayoutAlignment IView.HorizontalLayoutAlignment => HorizontalOptions.ToCore();
		Primitives.LayoutAlignment IView.VerticalLayoutAlignment => VerticalOptions.ToCore();

		double IView.Width => WidthRequest;
		double IView.Height => HeightRequest;

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			if (width >= 0 && height >= 0)
			{
				// This is a temporary measure to keep the old layouts working 
				Handler?.NativeArrange(Bounds);
			}
		}

		#region HotReload

		IFrameworkElement IReplaceableView.ReplacedView => MauiHotReloadHelper.GetReplacedView(this) ?? this;

		IReloadHandler IHotReloadableView.ReloadHandler { get; set; }

		void IHotReloadableView.TransferState(IFrameworkElement newView)
		{
			//TODO: LEt you hot reload the the ViewModel
			if (newView is View v)
				v.BindingContext = BindingContext;
		}

		void IHotReloadableView.Reload()
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				this.CheckHandlers();
				//Handler = null;
				var reloadHandler = ((IHotReloadableView)this).ReloadHandler;
				reloadHandler?.Reload();
				//TODO: if reload handler is null, Do a manual reload?
			});
		}
		#endregion
	}
}

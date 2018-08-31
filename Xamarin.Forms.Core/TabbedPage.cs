using System;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_TabbedPageRenderer))]
	public class TabbedPage : MultiPage<Page>, IBarElement, IElementConfiguration<TabbedPage>
	{
		public static readonly BindableProperty BarBackgroundColorProperty = BarElement.BarBackgroundColorProperty;

		public static readonly BindableProperty BarTextColorProperty = BarElement.BarTextColorProperty;

		readonly Lazy<PlatformConfigurationRegistry<TabbedPage>> _platformConfigurationRegistry;

		public Color BarBackgroundColor {
			get => (Color)GetValue(BarElement.BarBackgroundColorProperty);
			set => SetValue(BarElement.BarBackgroundColorProperty, value);
		}

		public Color BarTextColor {
			get => (Color)GetValue(BarElement.BarTextColorProperty);
			set => SetValue(BarElement.BarTextColorProperty, value);
		}

		protected override Page CreateDefault(object item)
		{
			var page = new Page();
			if (item != null)
				page.Title = item.ToString();

			return page;
		}

		public TabbedPage()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<TabbedPage>>(() => new PlatformConfigurationRegistry<TabbedPage>(this));
		}

		public new IPlatformElementConfiguration<T, TabbedPage> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}
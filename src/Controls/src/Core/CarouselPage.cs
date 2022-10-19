using System;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	internal class CarouselPage : MultiPage<ContentPage>, IElementConfiguration<CarouselPage>
	{
		readonly Lazy<PlatformConfigurationRegistry<CarouselPage>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/CarouselPage.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public CarouselPage()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<CarouselPage>>(() => new PlatformConfigurationRegistry<CarouselPage>(this));
		}

		/// <inheritdoc/>
		public new IPlatformElementConfiguration<T, CarouselPage> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override ContentPage CreateDefault(object item)
		{
			var page = new ContentPage();
			if (item != null)
				page.Title = item.ToString();

			return page;
		}
	}
}
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_CarouselPageRenderer))]
	public class CarouselPage : MultiPage<ContentPage>
	{
		protected override ContentPage CreateDefault(object item)
		{
			var page = new ContentPage();
			if (item != null)
				page.Title = item.ToString();

			return page;
		}
	}
}
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_TabbedPageRenderer))]
	public class TabbedPage : MultiPage<Page>
	{
		protected override Page CreateDefault(object item)
		{
			var page = new Page();
			if (item != null)
				page.Title = item.ToString();

			return page;
		}
	}
}
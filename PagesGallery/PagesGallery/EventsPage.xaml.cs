using Xamarin.Forms.Pages;

namespace PagesGallery
{
	public partial class EventsPage : ListDataPage
	{
		public EventsPage()
		{
			InitializeComponent();
		}
	}

	public class EventDetailsPage : DataPage
	{
		public EventDetailsPage()
		{
			((IDataSourceProvider)this).MaskKey("_id");
			((IDataSourceProvider)this).MaskKey("guid");
			((IDataSourceProvider)this).MaskKey("index");
			((IDataSourceProvider)this).MaskKey("isPublic");
		}
	}
}
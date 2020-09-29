using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7035, "[Bug][iOS] CarouselView last element is clipped",
		PlatformAffected.iOS)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue7035 : ContentPage
	{
		List<AdItem> announcements = new List<AdItem>();

		public Issue7035()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			for (int i = 0; i < 20; i++)
			{
				announcements.Add(new AdItem("no_artwork", "Card Title", "SUBHEAD"));
			}
			CV.ItemsSource = announcements;
			CV2.ItemsSource = announcements;
		}

	}

	public class AdItem
	{
		public string ImgUrl { get; set; }
		public string Title { get; set; }
		public string SubTitle { get; set; }
		public AdItem()
		{

		}
		public AdItem(string img, string ttl, string sttl)
		{
			ImgUrl = img;
			Title = ttl;
			SubTitle = sttl;
		}
	}
#endif
}
using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39987, "Bug 39987 - MapView not working correctly on iOS 9.3")]
	public class Bugzilla39987 : TestTabbedPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			Children.Add(new CustomMapPage(new CustomMapView(), "Teste 1"));
			Children.Add(new CustomMapPage(new CustomMapView(), "Teste 2"));
			Children.Add(new CustomMapPage(new CustomMapView(), "Teste 3"));
		}

	}

	public class CustomMapView : View
	{
		public CustomMapView()
		{

		}

	}

	public class CustomMapPage : ContentPage
	{
		private CustomMapView _customMapView;


		public CustomMapPage(CustomMapView customMapView, string title)
		{
			Title = title;
			_customMapView = customMapView;
			_customMapView.HorizontalOptions = LayoutOptions.FillAndExpand;
			_customMapView.VerticalOptions = LayoutOptions.FillAndExpand;
			Content = _customMapView;
		}

	}
}

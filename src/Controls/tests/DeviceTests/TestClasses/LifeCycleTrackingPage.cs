using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public class LifeCycleTrackingPage : ContentPage
	{
		public NavigatedFromEventArgs NavigatedFromArgs { get; private set; }
		public NavigatingFromEventArgs NavigatingFromArgs { get; private set; }
		public NavigatedToEventArgs NavigatedToArgs { get; private set; }
		public int AppearingCount { get; private set; }
		public int DisappearingCount { get; private set; }
		public int OnNavigatedToCount { get; private set; }
		public int OnNavigatingFromCount { get; private set; }
		public int OnNavigatedFromCount { get; private set; }

		public void ClearNavigationArgs()
		{
			NavigatedFromArgs = null;
			NavigatingFromArgs = null;
			NavigatedToArgs = null;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			AppearingCount++;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			DisappearingCount++;
		}

		protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
		{
			base.OnNavigatedFrom(args);
			NavigatedFromArgs = args;
			OnNavigatedFromCount++;
		}

		protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
		{
			base.OnNavigatingFrom(args);
			NavigatingFromArgs = args;
			OnNavigatingFromCount++;
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);
			NavigatedToArgs = args;
			OnNavigatedToCount++;
		}

		public void AssertLifeCycleCounts()
		{
			if (!this.HasAppeared)
			{
				Assert.Equal(AppearingCount, DisappearingCount);
				Assert.Equal(DisappearingCount, OnNavigatedToCount);
				Assert.Equal(OnNavigatingFromCount, OnNavigatedToCount);
			}
			else
			{
				Assert.Equal(AppearingCount, OnNavigatedToCount);
				Assert.Equal(DisappearingCount, AppearingCount - 1);
				Assert.Equal(OnNavigatingFromCount, AppearingCount - 1);
			}

			Assert.Equal(DisappearingCount, OnNavigatingFromCount);
		}
	}
}

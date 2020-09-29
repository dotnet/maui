using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
#if UITEST
	[Category(UITestCategories.Maps)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Issue(IssueTracker.Bugzilla, 39489, "Memory leak when using NavigationPage with Maps", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Bugzilla39489 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new Bz39489Content());
		}

#if UITEST

		protected override bool Isolate => true;

		[Test]
		public async Task Bugzilla39489Test()
		{
			// Original bug report (https://bugzilla.xamarin.com/show_bug.cgi?id=39489) had a crash (OOM) after 25-30
			// page loads. Obviously it's going to depend heavily on the device and amount of available memory, but
			// if this starts failing before 50 we'll know we've sprung another serious leak
			int iterations = 50;

			for (int n = 0; n < iterations; n++)
			{
				RunningApp.WaitForElement(q => q.Marked("New Page"));
				RunningApp.Tap(q => q.Marked("New Page"));
				RunningApp.WaitForElement(q => q.Marked("New Page"));
				await Task.Delay(1000);
				RunningApp.Back();
			}
		}
#endif
	}

	public class Bz39489Map : Map
	{
		static int s_count;

		public Bz39489Map()
		{
			Interlocked.Increment(ref s_count);
			Debug.WriteLine($"++++++++ Bz39489Map : Constructor, count is {s_count}");
		}

		~Bz39489Map()
		{
			Interlocked.Decrement(ref s_count);
			Debug.WriteLine($"-------- Bz39489Map: Destructor, count is {s_count}");
		}
	}

	[Preserve(AllMembers = true)]
	public class Bz39489Content : ContentPage
	{
		static int s_count;

		public Bz39489Content()
		{
			Interlocked.Increment(ref s_count);
			Debug.WriteLine($">>>>> Bz39489Content Bz39489Content 54: Constructor, count is {s_count}");

			var button = new Button { Text = "New Page" };

			var gcbutton = new Button { Text = "GC" };

			var map = new Bz39489Map();

			button.Clicked += Button_Clicked;
			gcbutton.Clicked += GCbutton_Clicked;

			Content = new StackLayout { Children = { button, gcbutton, map } };
		}

		void GCbutton_Clicked(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(">>>>>>>> Running Garbage Collection");
			GarbageCollectionHelper.Collect();
			System.Diagnostics.Debug.WriteLine($">>>>>>>> GC.GetTotalMemory = {GC.GetTotalMemory(true):n0}");
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Bz39489Content());
		}

		~Bz39489Content()
		{
			Interlocked.Decrement(ref s_count);
			Debug.WriteLine($">>>>> Bz39489Content ~Bz39489Content 82: Destructor, count is {s_count}");
		}
	}
}
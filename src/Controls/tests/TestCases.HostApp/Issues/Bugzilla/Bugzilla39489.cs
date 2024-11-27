using System.Diagnostics;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 39489, "Memory leak when using NavigationPage with Maps", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Bugzilla39489 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new Bz39489Content());
		}
	}

	public class Bz39489Map : Microsoft.Maui.Controls.Maps.Map
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


	public class Bz39489Content : ContentPage
	{
		static int s_count;

		public Bz39489Content()
		{
			Interlocked.Increment(ref s_count);
			Debug.WriteLine($">>>>> Bz39489Content Bz39489Content 54: Constructor, count is {s_count}");

			var button = new Button { AutomationId = "NewPage", Text = "New Page" };

			var gcbutton = new Button { AutomationId = "GC", Text = "GC" };

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
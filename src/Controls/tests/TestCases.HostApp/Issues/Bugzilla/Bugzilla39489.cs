using System.Diagnostics;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 39489, "Memory leak when using NavigationPage with Maps", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Bugzilla39489 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new Bz39489Root());
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

	public class Bz39489Root : ContentPage
	{
		public Bz39489Root()
		{
			var button = new Button { AutomationId = "NewPage", Text = "New Page" };
			var gcbutton = new Button { AutomationId = "GC", Text = "GC" };
			button.Clicked += Button_Clicked;
			gcbutton.Clicked += GCbutton_Clicked;
			Content = new VerticalStackLayout { button, gcbutton };
		}

		void GCbutton_Clicked(object sender, EventArgs e)
		{
			Debug.WriteLine(">>>>>>>> Running Garbage Collection");
			GarbageCollectionHelper.Collect();
			Debug.WriteLine($">>>>>>>> GC.GetTotalMemory = {GC.GetTotalMemory(true):n0}");
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Bz39489Content());
		}
	}


	public class Bz39489Content : ContentPage
	{
		static int s_count;

		public Bz39489Content()
		{
			Interlocked.Increment(ref s_count);
			Debug.WriteLine($">>>>> Bz39489Content: Constructor, count is {s_count}");

			var label = new Label { AutomationId = "StubLabel", Text = "Now press the back button." };

			var map = new Bz39489Map();

			Content = new StackLayout { Children = { label, map } };
		}

		~Bz39489Content()
		{
			Interlocked.Decrement(ref s_count);
			Debug.WriteLine($">>>>> Bz39489Content: Destructor, count is {s_count}");
		}
	}
}
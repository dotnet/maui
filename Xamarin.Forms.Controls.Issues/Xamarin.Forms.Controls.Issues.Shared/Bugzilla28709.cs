using System;

using Xamarin.Forms.CustomAttributes;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 28709, "Application.Properties saving crash ")]
	public class Bugzilla28709 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{

			var btn = new Button () {
				Text = "Save Properties",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			var btn1 = new Button () {
				Text = "Save Properties Multiple Threads",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			btn.Clicked += OnButtonClicked;
			btn1.Clicked += (sender, e) => {
				Task.Run(() => {System.Diagnostics.Debug.WriteLine ("thread 1"); OnButtonClicked1("thread1",new EventArgs());});
				Task.Run(() => {System.Diagnostics.Debug.WriteLine ("thread 2"); OnButtonClicked1("thread2",new EventArgs());});
				Task.Run(() => {System.Diagnostics.Debug.WriteLine ("thread 3"); OnButtonClicked1("thread3",new EventArgs());});

			};
			Content = new StackLayout { Children =  { btn, btn1 }};
		}

		void OnButtonClicked (object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"OnButtonClicked {sender.ToString()}");
				
			int j = -1;
			var properties = Application.Current.Properties;
			int seed = 13;
			while (++j < 300) {
				seed = ((seed * 257) + 41) % 65536;
				int i = seed % 20;

				int previousClickTotal = -1;
				if (properties.ContainsKey ("PreviousClickTotal" + i.ToString ()))
					previousClickTotal = (int)(Application.Current.Properties ["PreviousClickTotal" + i.ToString ()]);

				string clickTotal = "0";
				if (properties.ContainsKey ("ClickTotal" + i.ToString ()))
					clickTotal = (string)Application.Current.Properties ["ClickTotal" + i.ToString ()];

				double nextClickTotal = 1.0;
				if (properties.ContainsKey ("NextClickTotal" + i.ToString ()))
					nextClickTotal = (double)(Application.Current.Properties ["NextClickTotal" + i.ToString ()]);

				Application.Current.Properties ["PreviousClickTotal" + i.ToString ()] = ++previousClickTotal;
				Application.Current.Properties ["ClickTotal" + i.ToString ()] = previousClickTotal.ToString ();
				Application.Current.Properties ["NextClickTotal" + i.ToString ()] = ++nextClickTotal;

				SaveAllProperties ();
			}
		}

		async void OnButtonClicked1 (object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"OnButtonClicked {sender.ToString()}");
			//Application.Current.Properties[sender.ToString()] = 1;
			await Application.Current.SavePropertiesAsync ();
			System.Diagnostics.Debug.WriteLine ($"OnButtonClicked {sender.ToString()} done");
		}

		async void SaveAllProperties ()
		{
			await Application.Current.SavePropertiesAsync ();
		}


		#if UITEST
		[Test]
		public void Bugzilla28709Test ()
		{
			RunningApp.Tap (q => q.Marked ("Save Properties"));
		}
#endif
	}
}

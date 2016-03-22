using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class AppLifeCycle : Application
	{
		protected override void OnStart ()
		{
			base.OnStart ();
		}

		protected override void OnSleep ()
		{
			base.OnSleep ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
		}

		public AppLifeCycle ()
		{
			MainPage = new ContentPage {
				Content = new Label {
					Text = "Testing Lifecycle events"
				}
			};
		}
	}

	public class SimpleApp : Application
	{
		Label label;

		public SimpleApp ()
		{
			label = new Label { VerticalOptions = LayoutOptions.CenterAndExpand };

			if (Current.Properties.ContainsKey ("LabelText")) {
				label.Text = (string) Current.Properties["LabelText"] + " Restored!";
				Debug.WriteLine ("Initialized");
			} else {
				Current.Properties["LabelText"] = "Wowza";
				label.Text = (string) Current.Properties["LabelText"] + " Set!";
				Debug.WriteLine ("Saved");
			}

			MainPage = new ContentPage {
				Content = new StackLayout {
					Children = { 
						label 
					} 
				} 
			};

			SerializeProperties ();
		}

		async void SerializeProperties ()
		{
			await Current.SavePropertiesAsync ();
		}
	}

	public class App : Application
	{
		public string InsightsApiKey = Secrets["InsightsApiKey"];
		public const string AppName = "XamarinFormsControls";
		public const string AppVersion = "1.4.3";
		public static int IOSVersion = -1;
		readonly ITestCloudService _testCloudService;
		// make sure serialized data is available here

		//protected override void OnStart ()
		//{
		//	// called right after property store is populated
		//	MainPage.BackgroundColor = Color.Green;
		//	Current.Properties["TimesStarted"] = ((int)Current.Properties["TimesStarted"]) + 1;
		//	((MainPageLifeCycleTests)MainPage).UpdateLabels ();
		//}

		//protected override void OnResume ()
		//{
		//	MainPage.BackgroundColor = Color.White;
		//	Current.Properties["TimesResumed"] = ((int)Current.Properties["TimesResumed"]) + 1;
		//	((MainPageLifeCycleTests)MainPage).UpdateLabels ();
		//}

		//protected override void OnSleep ()
		//{
		//	MainPage.BackgroundColor = Color.Red;
		//	Current.Properties["TimesSlept"] = ((int)Current.Properties["TimesSlept"]) + 1;
		//	((MainPageLifeCycleTests)MainPage).UpdateLabels ();
		//}
		public static List<string> AppearingMessages = new List<string>();

		public static ContentPage MenuPage { get; set; }

		

		public App ()
		{
			_testCloudService = DependencyService.Get<ITestCloudService> ();
			InitInsights ();
			// MainPage = new MainPageLifeCycleTests ();
			MainPage = new MasterDetailPage () {
				Master = new ContentPage { Title = "Master", BackgroundColor = Color.Red },
				Detail = CoreGallery.GetMainPage ()
			};
		}

		public void SetMainPage (Page rootPage)
		{
			MainPage = rootPage;
		}

		void InitInsights ()
		{
			if (Insights.IsInitialized) {
				Insights.ForceDataTransmission = true;
				if (_testCloudService != null && _testCloudService.IsOnTestCloud ())
					Insights.Identify (_testCloudService.GetTestCloudDevice (), "Name", _testCloudService.GetTestCloudDeviceName ());
				else
					Insights.Identify ("DemoUser", "Name", "Demo User");
			}
		}

		public static Assembly GetAssembly (out string assemblystring)
		{
			assemblystring = typeof(App).AssemblyQualifiedName.Split (',')[1].Trim ();
			var assemblyname = new AssemblyName (assemblystring);
			return Assembly.Load (assemblyname);
		}

		public static async Task<string> LoadResource (string filename)
		{
			string assemblystring;
			Assembly assembly = GetAssembly (out assemblystring);

			Stream stream = assembly.GetManifestResourceStream ($"{assemblystring}.{filename}");
			string text;
			using (var reader = new StreamReader (stream)) {
				text = await reader.ReadToEndAsync ();
			}
			return text;
		}

		public static void InitSecrets ()
		{
			secrets = new Dictionary<string, string> ();

			string keyData = LoadResource ("secrets.txt").Result;
			string[] entries = keyData.Split ("\n\r".ToCharArray (), StringSplitOptions.RemoveEmptyEntries);
			foreach (string entry in entries) {
				string[] parts = entry.Split (':');
				if (parts.Length < 2) {
					continue;
				}

				secrets.Add (parts[0].Trim (), parts[1].Trim ());
			}
		}

		private static Dictionary<string, string> secrets;

		public static Dictionary<string, string> Secrets
		{
			get
			{
				if (secrets == null) {
					InitSecrets ();
				}

				return secrets;
			}
		} 
	}

	// Not quite sure how to turn this into a test case, effectively replace the normal Application with this to repro issues reported.
	// Full repro requires assignment to MainPage, hence the issue
	public class NavReproApp : Application
	{
		NavigationPage navPage1 = new NavigationPage ();

		public NavReproApp ()
		{

			var btn = new Button () { Text = "Start" };

			btn.Clicked += Btn_Clicked;

			navPage1.PushAsync (new ContentPage () { Content = btn });

			MainPage = navPage1;

		}

		async void Btn_Clicked (object sender, EventArgs e)
		{
			await navPage1.PushAsync (new ContentPage () { Content = new Label () { Text = "Page 2" } });
			await navPage1.PushAsync (new ContentPage () { Content = new Label () { Text = "Page 3" } });


			var navPage2 = new NavigationPage ();

			var btn = new Button () { Text = "Start Next" };
			btn.Clicked += Btn_Clicked1;

			await navPage2.PushAsync (new ContentPage () { Content = btn });

			MainPage = navPage2;


		}

		async void Btn_Clicked1 (object sender, EventArgs e)
		{
			MainPage = navPage1;
			await navPage1.PopAsync ();


			await navPage1.PushAsync (new ContentPage () { Content = new Label () { Text = "Page 3a" } });
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}

	public class MainPageLifeCycleTests : ContentPage
	{
		int numTimesStarted;
		int numTimesSlept;
		int numTimesResumed;

		StackLayout numTimesStartedLayout;
		StackLayout numTimesSleptLayout;
		StackLayout numTimesResumedLayout;

		public MainPageLifeCycleTests ()
		{
			object timesStarted;
			if (!Application.Current.Properties.TryGetValue ("TimesStarted", out timesStarted)) {
				Application.Current.Properties["TimesStarted"] = 0;
			} 
			var numTimesStarted = (int)Application.Current.Properties["TimesStarted"];
			

			object timesSlept;
			if (!Application.Current.Properties.TryGetValue ("TimesSlept", out timesSlept)) {
				Application.Current.Properties["TimesSlept"] = 0;
			}
			var numTimesSlept = (int)Application.Current.Properties["TimesSlept"];
	

			object timesResumed;
			if (!Application.Current.Properties.TryGetValue ("TimesResumed", out timesResumed)) {
				Application.Current.Properties["TimesResumed"] = 0;
			}
			var numTimesResumed = (int)Application.Current.Properties["TimesResumed"];

			numTimesStartedLayout = BuildLabelLayout ("TimesStarted", numTimesStarted);
			numTimesSleptLayout = BuildLabelLayout ("TimesSlept", numTimesSlept);
			numTimesResumedLayout = BuildLabelLayout ("TimesResumed", numTimesResumed);

			var layout = new StackLayout {
				Children = {
					numTimesStartedLayout,
					numTimesSleptLayout,
					numTimesResumedLayout
				}
			};

			Content = layout;
		}

		StackLayout BuildLabelLayout (string title, int property)
		{
			var labelTitle = new Label {
				Text = title
			};

			var valueLabel = new Label {
				Text = property.ToString ()
			};

			return new StackLayout {
				Children = {
					labelTitle,
					valueLabel
				}
			};
		}

		public void UpdateLabels ()
		{
			((Label)numTimesStartedLayout.Children[1]).Text = ((int)Application.Current.Properties["TimesStarted"]).ToString ();
			((Label)numTimesSleptLayout.Children[1]).Text = ((int)Application.Current.Properties["TimesSlept"]).ToString ();
			((Label)numTimesResumedLayout.Children[1]).Text = ((int)Application.Current.Properties["TimesResumed"]).ToString ();
		}
	}
}

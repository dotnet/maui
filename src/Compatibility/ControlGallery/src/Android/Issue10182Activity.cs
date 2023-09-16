//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

//namespace Microsoft.Maui.Controls.ControlGallery.Android
//{
//	[Activity(Label = "Issue10182Activity", Icon = "@drawable/icon", Theme = "@style/MyTheme",
//		MainLauncher = false, HardwareAccelerated = true,
//		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
//	public class Issue10182Activity : Microsoft.Maui.Controls.Compatibility.Platform.Android.FormsAppCompatActivity
//	{
//		Activity1 _activity1;
//		protected override void OnCreate(Bundle savedInstanceState)
//		{
//			var currentApplication = Microsoft.Maui.Controls.Application.Current;
//			TabLayoutResource = Resource.Layout.Tabbar;
//			ToolbarResource = Resource.Layout.Toolbar;

//			base.OnCreate(savedInstanceState);

//#pragma warning disable CS0612 // Type or member is obsolete
//			Forms.Init(new MauiContext(MauiApplication.Current.Services, this));
//#pragma warning restore CS0612 // Type or member is obsolete
//			LoadApplication(new Issue10182Application());

//			_activity1 = (Activity1)DependencyService.Resolve<Context>();

//			Device.BeginInvokeOnMainThread(async () =>
//			{
//				try
//				{

//					await Task.Delay(1000);
//					{
//						Intent intent = new Intent(_activity1, typeof(Activity1));
//						intent.AddFlags(ActivityFlags.ReorderToFront);
//						_activity1.StartActivity(intent);
//					}

//					await Task.Delay(1000);
//					{
//						Intent intent = new Intent(this, typeof(Issue10182Activity));
//						intent.AddFlags(ActivityFlags.ReorderToFront);
//						StartActivity(intent);
//					}

//					await Task.Delay(1000);
//					{
//						Intent intent = new Intent(_activity1, typeof(Activity1));
//						intent.AddFlags(ActivityFlags.ReorderToFront);
//						_activity1.StartActivity(intent);
//					}
//				}
//				finally
//				{
//					this.Finish();
//					_activity1.ReloadApplication();
//					currentApplication.MainPage = new Issue10182.Issue10182SuccessPage();
//				}
//			});
//		}

//		public class Issue10182Test : ContentPage
//		{
//			public Issue10182Test()
//			{
//				Content = new StackLayout()
//				{
//					Children =
//					{
//						new Label()
//						{
//							Text = "Hold Please. Activity should vanish soon and you'll see a success label",
//							AutomationId = "Loaded"
//						}
//					}
//				};
//			}
//		}

//		public class Issue10182Application : Application
//		{
//			protected override void OnStart()
//			{
//				var contentPage = new Issue10182Test();
//				base.OnStart();
//				MainPage = contentPage;
//			}

//			protected override void OnSleep()
//			{
//				base.OnSleep();
//				MainPage = new NavigationPage(new ContentPage());
//			}
//		}
//	}
//}
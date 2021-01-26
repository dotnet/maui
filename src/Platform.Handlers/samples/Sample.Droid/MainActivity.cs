using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using Xamarin.Forms;
using AndroidX.Core.Widget;
using Xamarin.Platform;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Sample.Droid
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		ViewGroup _page;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			SetContentView(Resource.Layout.activity_main);

			AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			_page = FindViewById<ViewGroup>(Resource.Id.Page);

			var app = new MyApp();

			var view = app.CreateView();

			Add(view);


			// In 5 seconds, add and remove some controls so we can see that working
			Task.Run(async () => {

				await Task.Delay(5000).ConfigureAwait(false);

				void addLabel()
				{
					(view as VerticalStackLayout).Add(new Label { Text = "I show up after 5 seconds" });
					var first = (view as VerticalStackLayout).Children.First();
					(view as VerticalStackLayout).Remove(first);
				};

				new Handler(Looper.MainLooper).Post(addLabel);

			});
		}

		void Add(params IView[] views)
		{
			foreach (var view in views)
			{
				_page.AddView(view.ToNative(this), new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent , ViewGroup.LayoutParams.MatchParent));
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}
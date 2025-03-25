using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Navigation;
using AndroidX.Navigation.UI;
using Google.Android.Material.AppBar;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;

namespace Maui.Controls.Sample.Droid;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/Theme.MyApplication")]
public class MainActivity : AppCompatActivity
{
	AppBarConfiguration? appBarConfiguration;

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		SetContentView(Resource.Layout.activity_main);

		var toolbar = FindViewById<MaterialToolbar>(Resource.Id.toolbar)!;
		SetSupportActionBar(toolbar);

		var navController = Navigation.FindNavController(this, Resource.Id.nav_host_fragment_content_main);
		appBarConfiguration = new AppBarConfiguration.Builder(navController.Graph).Build();
		NavigationUI.SetupActionBarWithNavController(this, navController, appBarConfiguration);

		var fab = FindViewById<FloatingActionButton>(Resource.Id.fab)!;
		fab.Click += (s, e) =>
		{
			var snackbar = Snackbar.Make(fab, "Replace with your own action", Snackbar.LengthLong);
			snackbar.SetAnchorView(Resource.Id.fab);
			snackbar.SetAction("Action", _ => { });
			snackbar.Show();
		};
	}

	public override bool OnCreateOptionsMenu(IMenu? menu)
	{
		// Inflate the menu; this adds items to the action bar if it is present.
		MenuInflater.Inflate(Resource.Menu.menu_main, menu);
		return true;
	}

	public override bool OnOptionsItemSelected(IMenuItem item)
	{
		// Handle action bar item clicks here. The action bar will
		// automatically handle clicks on the Home/Up button, so long
		// as you specify a parent activity in AndroidManifest.xml.
		var id = item.ItemId;

		if (id == Resource.Id.SettingsFragment)
		{
			var navController = Navigation.FindNavController(this, Resource.Id.nav_host_fragment_content_main);
			if (NavigationUI.OnNavDestinationSelected(item, navController))
				return true;
		}

		return base.OnOptionsItemSelected(item);
	}

	public override bool OnSupportNavigateUp()
	{
		var navController = Navigation.FindNavController(this, Resource.Id.nav_host_fragment_content_main);
		return
			NavigationUI.NavigateUp(navController, appBarConfiguration!) ||
			base.OnSupportNavigateUp();
	}
}

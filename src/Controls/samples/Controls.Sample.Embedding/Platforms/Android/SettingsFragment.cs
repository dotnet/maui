using Android.Runtime;
using Android.Views;
using AndroidX.Navigation.Fragment;
using static Android.Views.ViewGroup.LayoutParams;
using Button = Android.Widget.Button;
using Fragment = AndroidX.Fragment.App.Fragment;
using View = Android.Views.View;

namespace Maui.Controls.Sample.Droid;

[Register("com.microsoft.maui.sample.emdedding." + nameof(SettingsFragment))]
public class SettingsFragment : Fragment
{
	public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState) =>
		inflater.Inflate(Resource.Layout.fragment_settings, container, false);

	public override void OnViewCreated(View view, Bundle? savedInstanceState)
	{
		base.OnViewCreated(view, savedInstanceState);

		var buttonSettings = view.FindViewById<Button>(Resource.Id.button_settings)!;
		buttonSettings.Click += (s, e) =>
		{
			NavHostFragment.FindNavController(this).NavigateUp();
		};

		var button3 = view.FindViewById<Button>(Resource.Id.button3)!;
		button3.Click += OnMagicClicked;
	}

	public override void OnDestroyView()
	{
		base.OnDestroyView();
	}

	private void OnMagicClicked(object? sender, EventArgs e)
	{
	}
}

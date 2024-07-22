using Android.Runtime;
using Android.Views;
using AndroidX.Navigation.Fragment;
using Button = Android.Widget.Button;
using Fragment = AndroidX.Fragment.App.Fragment;
using View = Android.Views.View;
using static Android.Views.ViewGroup.LayoutParams;

namespace Maui.Controls.Sample.Droid;

[Register("com.microsoft.maui.sample.emdedding." + nameof(FirstFragment))]
public class FirstFragment : Fragment
{
	public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState) =>
		inflater.Inflate(Resource.Layout.fragment_first, container, false);

	public override void OnViewCreated(View view, Bundle? savedInstanceState)
	{
		base.OnViewCreated(view, savedInstanceState);

		var buttonFirst = view.FindViewById<Button>(Resource.Id.button_first)!;
		buttonFirst.Click += (s, e) =>
		{
			NavHostFragment.FindNavController(this).Navigate(Resource.Id.action_FirstFragment_to_SecondFragment);
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

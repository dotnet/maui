using Android.Runtime;
using Android.Views;
using AndroidX.Navigation.Fragment;
using Button = Android.Widget.Button;
using Fragment = AndroidX.Fragment.App.Fragment;
using View = Android.Views.View;

namespace Maui.Controls.Sample.Droid;

[Register("com.microsoft.maui.sample.emdedding." + nameof(SecondFragment))]
public class SecondFragment : Fragment
{
	public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState) =>
		inflater.Inflate(Resource.Layout.fragment_second, container, false);

	public override void OnViewCreated(View view, Bundle? savedInstanceState)
	{
		base.OnViewCreated(view, savedInstanceState);

		var buttonSecond = view.FindViewById<Button>(Resource.Id.button_second)!;
		buttonSecond.Click += (s, e) =>
		{
			NavHostFragment.FindNavController(this).Navigate(Resource.Id.action_SecondFragment_to_FirstFragment);
		};
	}
}

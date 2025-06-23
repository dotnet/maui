using Android.Runtime;
using Android.Views;
using AndroidX.Navigation.Fragment;
using static Android.Views.ViewGroup.LayoutParams;
using Button = Android.Widget.Button;
using Fragment = AndroidX.Fragment.App.Fragment;
using View = Android.Views.View;

namespace Maui.Controls.Sample.Droid;

[Register("com.microsoft.maui.sample.emdedding." + nameof(FirstFragment))]
public class FirstFragment : Fragment
{
	EmbeddingScenarios.IScenario? _scenario;
	MyMauiContent? _mauiView;
	Android.Views.View? _nativeView;

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

		// Uncomment the scenario to test:
		//_scenario = new EmbeddingScenarios.Scenario1_Basic();
		//_scenario = new EmbeddingScenarios.Scenario2_Scoped();
		_scenario = new EmbeddingScenarios.Scenario3_Correct();

		// Create the view and (maybe) the window
		(_mauiView, _nativeView) = _scenario!.Embed(Activity!);

		// Add the new view to the UI
		var rootLayout = view.FindViewById<LinearLayout>(Resource.Id.layout_first)!;
		rootLayout.AddView(_nativeView, 1, new LinearLayout.LayoutParams(MatchParent, WrapContent));
	}

	public override void OnDestroyView()
	{
		base.OnDestroyView();

		// Remove the view from the UI
		var rootLayout = View!.FindViewById<LinearLayout>(Resource.Id.layout_first)!;
		rootLayout.RemoveView(_nativeView);

		// If we used a window, then clean that up
		if (_mauiView?.Window is IWindow window)
			window.Destroying();

		base.OnStop();
	}

	private async void OnMagicClicked(object? sender, EventArgs e)
	{
		if (_mauiView?.DotNetBot is not Image bot)
			return;

		await bot.RotateTo(360, 1000);
		bot.Rotation = 0;

		bot.HeightRequest = 90;
	}
}

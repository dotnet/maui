namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37441, "Concurrent bindable property updates and off-thread visual state changes", PlatformAffected.All)]
public class Issue37441 : ContentPage
{
	static readonly TimeSpan ScenarioDuration = TimeSpan.FromSeconds(3);

	readonly ContentView _targetHost = new();
	readonly Button _scenarioAButton;
	readonly Button _scenarioBButton;
	readonly Label _scenarioAStatus;
	readonly Label _scenarioBStatus;

	public Issue37441()
	{
		_scenarioAButton = new Button
		{
			AutomationId = "ScenarioAButton",
			Text = "Run Scenario A"
		};
		_scenarioAButton.Clicked += OnRunScenarioA;

		_scenarioBButton = new Button
		{
			AutomationId = "ScenarioBButton",
			Text = "Run Scenario B"
		};
		_scenarioBButton.Clicked += OnRunScenarioB;

		_scenarioAStatus = new Label
		{
			AutomationId = "ScenarioAStatus",
			Text = "Not run"
		};
		_scenarioBStatus = new Label
		{
			AutomationId = "ScenarioBStatus",
			Text = "Not run"
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = 16,
				Spacing = 12,
				Children =
				{
					new Label { Text = "Bindable property concurrency regression" },
					_scenarioAButton,
					_scenarioAStatus,
					_scenarioBButton,
					_scenarioBStatus,
					_targetHost
				}
			}
		};
	}

	async void OnRunScenarioA(object sender, EventArgs e)
	{
		_scenarioAButton.IsEnabled = false;
		_scenarioAStatus.Text = "RUNNING";

		try
		{
			var target = new Issue37441RaceLabel { Text = "Scenario A target" };
			_targetHost.Content = target;

			using var cancellation = new CancellationTokenSource();
			using var start = new ManualResetEventSlim();

			var alpha = Task.Run(() =>
			{
				start.Wait(cancellation.Token);
				for (var value = 0; !cancellation.IsCancellationRequested; value++)
					target.RaceAlpha = value;
			});

			var beta = Task.Run(() =>
			{
				start.Wait(cancellation.Token);
				for (var value = 0; !cancellation.IsCancellationRequested; value++)
					target.RaceBeta = -value;
			});

			start.Set();
			var work = Task.WhenAll(alpha, beta);
			await Task.WhenAny(work, Task.Delay(ScenarioDuration));
			cancellation.Cancel();
			await work;

			_scenarioAStatus.Text = "PASS";
		}
		catch (Exception exception)
		{
			_scenarioAStatus.Text = $"FAIL: {exception.GetType().Name}";
		}
		finally
		{
			_scenarioAButton.IsEnabled = true;
		}
	}

	async void OnRunScenarioB(object sender, EventArgs e)
	{
		_scenarioBButton.IsEnabled = false;
		_scenarioBStatus.Text = "RUNNING";

		try
		{
			var target = CreateScenarioBTarget();
			_targetHost.Content = target;

			using var cancellation = new CancellationTokenSource();
			var stateChanges = Task.Run(() =>
			{
				for (var index = 0; !cancellation.IsCancellationRequested; index++)
				{
					var state = index % 2 == 0 ? "Off" : "On";
					if (!VisualStateManager.GoToState(target, state))
						throw new InvalidOperationException($"Visual state '{state}' was not found.");
				}
			});
			var animation = FadeLoopAsync(target, cancellation.Token);
			var work = Task.WhenAll(stateChanges, animation);

			await Task.WhenAny(work, Task.Delay(ScenarioDuration));
			cancellation.Cancel();
			await work;

			_scenarioBStatus.Text = "PASS";
		}
		catch (Exception exception)
		{
			_scenarioBStatus.Text = $"FAIL: {exception.GetType().Name}";
		}
		finally
		{
			_scenarioBButton.IsEnabled = true;
		}
	}

	static Issue37441RaceLabel CreateScenarioBTarget()
	{
		var target = new Issue37441RaceLabel
		{
			Text = "Scenario B target",
			TextColor = Colors.Blue
		};

		var states = new VisualStateGroup { Name = "RaceStates" };
		states.States.Add(CreateState("Off", Colors.Blue));
		states.States.Add(CreateState("On", Colors.HotPink));
		VisualStateManager.SetVisualStateGroups(target, new VisualStateGroupList { states });

		return target;
	}

	static VisualState CreateState(string name, Color textColor)
	{
		var state = new VisualState { Name = name };
		state.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = textColor });
		return state;
	}

	static async Task FadeLoopAsync(VisualElement target, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			await target.FadeToAsync(0.2, 100);
			if (!cancellationToken.IsCancellationRequested)
				await target.FadeToAsync(1, 100);
		}
	}
}

class Issue37441RaceLabel : Label
{
	public static readonly BindableProperty RaceAlphaProperty =
		BindableProperty.Create(nameof(RaceAlpha), typeof(int), typeof(Issue37441RaceLabel), 0);

	public static readonly BindableProperty RaceBetaProperty =
		BindableProperty.Create(nameof(RaceBeta), typeof(int), typeof(Issue37441RaceLabel), 0);

	public int RaceAlpha
	{
		get => (int)GetValue(RaceAlphaProperty);
		set => SetValue(RaceAlphaProperty, value);
	}

	public int RaceBeta
	{
		get => (int)GetValue(RaceBetaProperty);
		set => SetValue(RaceBetaProperty, value);
	}
}

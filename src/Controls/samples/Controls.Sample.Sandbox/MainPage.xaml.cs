using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	const int ItemCount = 280;

	readonly StringBuilder _results = new();
	LayoutPerfRunner? _runner;
	bool _running;

	public MainPage()
	{
		InitializeComponent();
	}

	LayoutPerfRunner Runner => _runner ??= new LayoutPerfRunner(this, OverlayHost, OnLine, ItemCount);

	async void OnRunAllClicked(object? sender, EventArgs e)
	{
		if (_running)
		{
			return;
		}

		_running = true;
		_results.Clear();
		ResultsLabel.Text = string.Empty;

		try
		{
			await Runner.RunAllAsync(SetStatus);
			SetStatus("COMPLETE");
			ResultsLabel.Text = Runner.Summary;
			Console.WriteLine(LayoutPerfRunner.LogPrefix + "|summary|" + Runner.Summary.Replace(Environment.NewLine, " ; ", StringComparison.Ordinal));
		}
		catch (Exception ex)
		{
			SetStatus("FAILED: " + ex.GetType().Name + " " + ex.Message);
			Console.WriteLine(LayoutPerfRunner.LogPrefix + "|error|" + ex);
		}
		finally
		{
			_running = false;
		}
	}

	async void OnRunSingleClicked(object? sender, EventArgs e)
	{
		if (_running)
		{
			return;
		}

		_running = true;

		try
		{
			var items = FeedDataFactory.Create(ItemCount);
			var tree = PerfContentFactory.Build(LayoutPerfRunner.Scenarios[0], items);

			var closeButton = new Button
			{
				AutomationId = "ClosePopupButton",
				Text = "Close",
				Margin = new Thickness(12, 40, 12, 4),
			};

			var root = new Grid
			{
				RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) },
			};

			root.Add(closeButton, 0, 0);
			root.Add(tree.Root, 0, 1);

			var page = new ContentPage { BackgroundColor = Color.FromArgb("#101828"), Content = root };
			NavigationPage.SetHasNavigationBar(page, false);
			closeButton.Clicked += async (_, _) => await Navigation.PopModalAsync(false);

			await Navigation.PushModalAsync(page, false);
			SetStatus("Popup shown");
		}
		finally
		{
			_running = false;
		}
	}

	void SetStatus(string text)
	{
		StatusLabel.Text = text;
		Console.WriteLine(LayoutPerfRunner.LogPrefix + "|status|" + text);
	}

	void OnLine(string line)
	{
		_results.AppendLine(line);
	}
}

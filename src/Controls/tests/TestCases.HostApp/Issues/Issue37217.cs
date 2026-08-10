namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37217, "ShellContent.Content — shared Page.PropertyChanged retains prior ShellContent instances", PlatformAffected.All)]
public class Issue37217 : ContentPage
{
	const int ShellContentsToCreate = 30;

	readonly Label _createdCountLabel;
	readonly Label _summaryLabel;
	readonly List<WeakReference> _trackedShellContents = new();

	// A single long-lived Page shared across many transient ShellContent instances,
	// exactly like the retention path described in the issue.
	readonly ContentPage _sharedPage = new ContentPage { Title = "Shared Page" };

	public Issue37217()
	{
		BackgroundColor = Colors.White;

		_createdCountLabel = new Label
		{
			Text = "ShellContents created: 0",
			AutomationId = "CreatedCountLabel"
		};

		_summaryLabel = new Label
		{
			Text = "Alive count: 0/0",
			AutomationId = "SummaryLabel"
		};

		var createShellContentsButton = new Button
		{
			Text = "Create ShellContents",
			AutomationId = "CreateShellContentsButton"
		};
		createShellContentsButton.Clicked += OnCreateShellContents;

		var forceGcButton = new Button
		{
			Text = "Force GC",
			AutomationId = "ForceGCButton"
		};
		forceGcButton.Clicked += OnForceGc;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(16),
				Spacing = 12,
				Children =
				{
					new Label
					{
						Text = "ShellContent.Content Memory Leak Test",
						FontSize = 22,
						FontAttributes = FontAttributes.Bold,
						AutomationId = "TitleLabel"
					},
					new Label
					{
						Text = "Assigns the same long-lived Page to many transient ShellContent instances, drops them, and forces a GC. Alive count should reach 0 if the leak is fixed.",
						AutomationId = "DescriptionLabel"
					},
					createShellContentsButton,
					forceGcButton,
					_createdCountLabel,
					_summaryLabel,
				}
			}
		};
	}

	void OnCreateShellContents(object sender, EventArgs e)
	{
		for (var i = 0; i < ShellContentsToCreate; i++)
		{
			var shellContent = new ShellContent
			{
				Content = _sharedPage
			};

			_trackedShellContents.Add(new WeakReference(shellContent));
		}

		_createdCountLabel.Text = $"ShellContents created: {_trackedShellContents.Count}";
		UpdateAliveCount();
	}

	async void OnForceGc(object sender, EventArgs e)
	{
		try
		{
			await GarbageCollectionHelper.WaitForGC(2000, _trackedShellContents.ToArray());
		}
		catch
		{
			// Keep the count visible for UITest assertion when GC does not complete in time.
		}

		UpdateAliveCount();
	}

	void UpdateAliveCount()
	{
		var alive = 0;
		foreach (var reference in _trackedShellContents)
		{
			if (reference.IsAlive)
			{
				alive++;
			}
		}

		_summaryLabel.Text = $"Alive count: {alive}/{_trackedShellContents.Count}";
	}
}

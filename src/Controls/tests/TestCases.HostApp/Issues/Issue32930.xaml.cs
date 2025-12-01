namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32930, "[iOS] SearchHandler suggestions list does not follow the search bar upward movement on iPhone, creating a layout gap", PlatformAffected.iOS)]
public partial class Issue32930 : Shell
{
	public Issue32930()
	{
		InitializeComponent();
	}
}

public class Issue32930SearchHandler : SearchHandler
{
	public Issue32930SearchHandler()
	{
		ItemsSource = new List<string>
		{
			"Apple",
			"Banana", 
			"Cherry",
			"Date",
			"Elderberry",
			"Fig",
			"Grape"
		};
	}

	protected override void OnQueryChanged(string oldValue, string newValue)
	{
		base.OnQueryChanged(oldValue, newValue);
		
		if (string.IsNullOrEmpty(newValue))
		{
			ItemsSource = new List<string>
			{
				"Apple",
				"Banana",
				"Cherry",
				"Date",
				"Elderberry",
				"Fig",
				"Grape"
			};
		}
		else
		{
			ItemsSource = new List<string>
			{
				"Apple",
				"Banana",
				"Cherry",
				"Date",
				"Elderberry",
				"Fig",
				"Grape"
			}.Where(s => s.Contains(newValue, StringComparison.OrdinalIgnoreCase)).ToList();
		}
	}
}

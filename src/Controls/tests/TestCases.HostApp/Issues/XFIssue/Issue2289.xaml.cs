using System.Diagnostics;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 2289, "TextCell IsEnabled property not disabling element in TableView", PlatformAffected.iOS)]
public partial class Issue2289 : TestContentPage
{
	public Issue2289()
	{
		InitializeComponent();
	}

	protected override void Init()
	{
		MoreCommand = new Command<MenuItem>((menuItem) =>
		{
			Debug.WriteLine("More! Command Called!");
		});

		DeleteCommand = new Command<MenuItem>((menuItem) =>
		{
			Debug.WriteLine("Delete Command Called!");
		});
		BindingContext = this;
	}

	public ICommand MoreCommand { get; protected set; }

	public ICommand DeleteCommand { get; protected set; }
}


using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30483, "[iOS] Flyout Menu CollectionView First Item Misaligned", PlatformAffected.iOS)]
public partial class Issue30483 : Shell
{
	public ObservableCollection<FlyoutMenuItem> MenuItems { get; set; }

	public Issue30483()
	{
		InitializeComponent();
		
		MenuItems = new ObservableCollection<FlyoutMenuItem>
		{
			new FlyoutMenuItem { Title = "First Item", AutomationId = "FirstItem", LabelAutomationId = "FirstItemLabel" },
			new FlyoutMenuItem { Title = "Second Item", AutomationId = "SecondItem", LabelAutomationId = "SecondItemLabel" },
			new FlyoutMenuItem { Title = "Third Item", AutomationId = "ThirdItem", LabelAutomationId = "ThirdItemLabel" },
			new FlyoutMenuItem { Title = "Fourth Item", AutomationId = "FourthItem", LabelAutomationId = "FourthItemLabel" }
		};
		
		BindingContext = this;
		
		// Open flyout after a delay to ensure everything is loaded
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
		{
			FlyoutIsPresented = true;
		});
	}
}

public class FlyoutMenuItem
{
	public string Title { get; set; }
	public string AutomationId { get; set; }
	public string LabelAutomationId { get; set; }
}

public class Issue30483Content : ContentPage
{
	public Issue30483Content()
	{
		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Label { Text = "Issue 30483 Test", FontSize = 18, FontAttributes = FontAttributes.Bold },
				new Label { Text = "Open the flyout menu to see the first item alignment", TextColor = Colors.Gray },
				new Button 
				{ 
					Text = "Open Flyout", 
					AutomationId = "OpenFlyoutButton",
					Command = new Command(() =>
					{
						if (Shell.Current != null)
						{
							Shell.Current.FlyoutIsPresented = true;
						}
					})
				}
			}
		};
	}
}

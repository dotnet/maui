namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14572, "WinUI: TabbedPage pages provided by a DataTemplate crash when swapping to a different tab", PlatformAffected.UWP)]

public class Issue14572 : TabbedPage
{
	public Issue14572()
	{
		ItemsSource = new List<object> { new Issue14572Vm1(), new Issue14572Vm2(), new Issue14572Vm3() };
		ItemTemplate = new Issue14572TabbedItemTemplateSelector();
	}
	
	public class Issue14572Vm1 { }
	public class Issue14572Vm2 { }
	public class Issue14572Vm3 { }
	class Issue14572TabbedItemTemplateSelector : DataTemplateSelector
	{
		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is Issue14572Vm1)
				return new DataTemplate(() => new ContentPage() { Title = "Type 1" });
			if (item is Issue14572Vm2)
				return new DataTemplate(() => new ContentPage() { Title = "Type 2", Content = new Label { Text = "Type 2 Content", AutomationId = "Type2Content" } });
			if (item is Issue14572Vm3)
				return new DataTemplate(() => new ContentPage() { Title = "Type 3" });

			return null;
		}
	}
}
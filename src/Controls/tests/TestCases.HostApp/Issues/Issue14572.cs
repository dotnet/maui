namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14572, "WinUI: TabbedPage pages provided by a DataTemplate crash when swapping to a different tab", PlatformAffected.UWP)]

public class Issue14572 : TabbedPage
{
	public Issue14572()
	{
		ItemsSource = new List<object> { new Vm1(), new Vm2(), new Vm3() };
		ItemTemplate = new TabbedItemTemplateSelector();
	}
	
	public class Vm1 { }
	public class Vm2 { }
	public class Vm3 { }
	class TabbedItemTemplateSelector : DataTemplateSelector
	{
		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is Vm1)
				return new DataTemplate(() => new ContentPage() { Title = "Type 1" });
			if (item is Vm2)
				return new DataTemplate(() => new ContentPage() { Title = "Type 2", Content = new Label { Text = "Type 2 Content" }, AutomationId="Type2Content" });
			if (item is Vm3)
				return new DataTemplate(() => new ContentPage() { Title = "Type 3" });

			return null;
		}
	}
}
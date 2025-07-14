using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26820, "ListView items height are not proper in initial loading with HasUnevenRows set as True", PlatformAffected.Android)]
	public class Issue26820 : TestContentPage
	{
		ListView list;
		StackLayout stackLayout;

		ObservableCollection<Person1> Data;
		public static int Count = 0;


		protected override void Init()
		{
			list = new ListView() { AutomationId = "listView" };

			var template = new DataTemplate(typeof(UnevenViewCell));
			list.ItemTemplate = template;

			Data = new ObservableCollection<Person1>();
			list.ItemsSource = Data;
			list.HasUnevenRows = true;

			for (int i = 0; i < 50; i++)
			{
				Data.Add(new Person1
				{
					FullName = "Andrew",
					Address = "404 Somewhere"
				});
			}



			stackLayout = new StackLayout();
			stackLayout.Children.Add(list);

			this.Content = stackLayout;
		}

	}
	class UnevenViewCell : ViewCell
	{
		public UnevenViewCell()
		{

			var label = new Label();
			label.SetBinding(Label.TextProperty, "FullName");
			Height = Issue26820.Count % 2 == 0 ? 50 : 100;
			View = label;
			View.BackgroundColor = Issue26820.Count % 2 == 0 ? Colors.Pink : Colors.LightYellow;
			Issue26820.Count++;
		}
	}


	class Person1
	{
		public string FullName { get; set; }
		public string Address { get; set; }
	}

}
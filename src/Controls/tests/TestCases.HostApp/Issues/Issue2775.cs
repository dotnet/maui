namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2775, "ViewCell background conflicts with ListView Semi-Transparent and Transparent backgrounds")]
	public class Issue2775 : TestContentPage
	{
		protected override void Init()
		{
			var list = new ListView
			{
				AutomationId = "TestReady",
				ItemsSource = GetList("Normal BG Blue"),
				BackgroundColor = Colors.Blue,
				ItemTemplate = new DataTemplate(typeof(NormalCell))
			};

			var listTransparent = new ListView
			{
				ItemsSource = GetList("Normal BG Transparent"),
				BackgroundColor = Colors.Transparent,
				ItemTemplate = new DataTemplate(typeof(NormalCell))
			};

			var listSemiTransparent = new ListView
			{
				ItemsSource = GetList("Normal BG SEMI Transparent"),
				BackgroundColor = Color.FromArgb("#801B2A39"),
				ItemTemplate = new DataTemplate(typeof(NormalCell))
			};

			var listContextActions = new ListView
			{
				ItemsSource = GetList("ContextActions BG PINK"),
				BackgroundColor = Colors.Pink,
				ItemTemplate = new DataTemplate(typeof(ContextActionsCell))
			};

			var listContextActionsTransparent = new ListView
			{
				ItemsSource = GetList("ContextActions BG Transparent"),
				BackgroundColor = Colors.Transparent,
				ItemTemplate = new DataTemplate(typeof(ContextActionsCell))
			};

			var listContextActionsSemiTransparent = new ListView
			{
				ItemsSource = GetList("ContextActions BG Semi Transparent"),
				BackgroundColor = Color.FromArgb("#801B2A39"),
				ItemTemplate = new DataTemplate(typeof(ContextActionsCell))
			};

			Content = new StackLayout
			{
				Children = {
					list,
					listTransparent,
					listSemiTransparent,
					listContextActions,
					listContextActionsTransparent,
					listContextActionsSemiTransparent
				},
				BackgroundColor = Colors.Red
			};
		}


		internal class ContextActionsCell : ViewCell
		{
			public ContextActionsCell()
			{
				ContextActions.Add(new MenuItem { Text = "action" });
				var label = new Label();
				label.SetBinding(Label.TextProperty, "Name");
				View = label;
			}
		}


		internal class NormalCell : ViewCell
		{
			public NormalCell()
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, "Name");
				View = label;
			}
		}

		List<ListItemViewModel> GetList(string description)
		{
			var itemList = new List<ListItemViewModel>();
			for (var i = 1; i < 3; i++)
			{
				itemList.Add(new ListItemViewModel() { Name = description });
			}
			return itemList;
		}


		public class ListItemViewModel
		{
			public string Name { get; set; }
		}
	}
}

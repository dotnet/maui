using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4933, "Grid size incorrect when using with Image", PlatformAffected.All)]
	public class Issue4493 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			// Initialize ui here instead of ctor
			BackgroundColor = Color.Gray;
			var contentGrid = new Grid
			{
				AutomationId = "IssuePageGrid",
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Color.Maroon,
				RowSpacing = 0,
				RowDefinitions = new RowDefinitionCollection()
				{
					new RowDefinition(){Height = GridLength.Auto},
					new RowDefinition(){Height = 20}
				}
			};
			contentGrid.AddChild(new Image() { Source = "photo.jpg", AutomationId = "IssuePageImage" }, 0, 0);
			contentGrid.AddChild(new Label() { Text = "test message", BackgroundColor = Color.Blue }, 0, 1);
			Content = contentGrid;
		}
	}
}
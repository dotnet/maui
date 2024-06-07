using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2615, "iOS Cell Reuse screws up when cells are both ViewCell with different children", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	public class Issue2615 : ContentPage
	{
		public Issue2615()
		{
			Title = "Test Blank Rows";

			var tableView = new TableView();
			tableView.HasUnevenRows = true;

			var tableHeaderSection = new TableSection();

			var viewHeaderCell = new ViewCell();

			var headerCellLayout = new StackLayout();
			headerCellLayout.Orientation = StackOrientation.Vertical;
			headerCellLayout.Spacing = 6;
			headerCellLayout.HorizontalOptions = LayoutOptions.Fill;
			headerCellLayout.VerticalOptions = LayoutOptions.Fill;

			var largeNumberLabel = new Label();
			largeNumberLabel.FontFamily = "HelveticaNeue-Light";
			largeNumberLabel.FontSize = 52;
			largeNumberLabel.Text = "90";
			largeNumberLabel.TextColor = Color.FromRgb(0.00392156885936856, 0.47843137383461, 0.996078431606293);
			largeNumberLabel.HorizontalOptions = LayoutOptions.Center;
			largeNumberLabel.VerticalOptions = LayoutOptions.Fill;
			headerCellLayout.Children.Add(largeNumberLabel);

			var nameLabel = new Label();
			nameLabel.FontFamily = "HelveticaNeue-Light";
			nameLabel.FontSize = 17;
			nameLabel.Text = "Name: John Doe";
			nameLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
			nameLabel.VerticalOptions = LayoutOptions.Center;
			headerCellLayout.Children.Add(nameLabel);

			viewHeaderCell.Height = 100;
			viewHeaderCell.View = headerCellLayout;
			tableHeaderSection.Add(viewHeaderCell);
			tableView.Root.Add(tableHeaderSection);

			for (int sectionNumber = 1; sectionNumber < 11; sectionNumber++)
			{
				var tableSection = new TableSection("Section #" + sectionNumber);

				for (int cellNumber = 1; cellNumber < 11; cellNumber++)
				{

					var viewCell = new ViewCell();
					var viewCellLayout = new StackLayout();
					viewCellLayout.Orientation = StackOrientation.Horizontal;
					viewCellLayout.Spacing = 6;
					viewCellLayout.Padding = new Thickness(20, 10);
					viewCellLayout.HorizontalOptions = LayoutOptions.Center;
					viewCellLayout.VerticalOptions = LayoutOptions.Center;

					var titleLabel = new Label();
					titleLabel.FontFamily = "HelveticaNeue-Light";
					titleLabel.FontSize = 17;
					titleLabel.Text = "Cell #" + cellNumber;
					viewCellLayout.Children.Add(titleLabel);

					viewCell.View = viewCellLayout;

					tableSection.Add(viewCell);
				}

				tableView.Root.Add(tableSection);

			}

			Content = tableView;

		}
	}
}

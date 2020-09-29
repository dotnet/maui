using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class TableViewCoreGalleryPage : CoreGalleryPage<TableView>
	{
		// TODO
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var tableSectionContainer = new ViewContainer<TableView>(Test.TableView.TableSection, new TableView());
			var section = new TableSection("Test")
			{
				TextColor = Color.Red
			};

			section.Add(new TextCell { Text = "Worked!" });

			var section1 = new TableSection("Testing")
			{
				TextColor = Color.Green
			};

			section1.Add(new TextCell { Text = "Workeding!" });

			var section2 = new TableSection("Test old")
			{
				new TextCell { Text = "Worked old!" }
			};

			tableSectionContainer.View.Root.Add(section);
			tableSectionContainer.View.Root.Add(section1);
			tableSectionContainer.View.Root.Add(section2);

			Add(tableSectionContainer);
		}
	}
}
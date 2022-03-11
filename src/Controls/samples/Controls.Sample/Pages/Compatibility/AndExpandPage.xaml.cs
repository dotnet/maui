namespace Maui.Controls.Sample.Pages
{
	public partial class AndExpandPage
	{
		int _counter = 0;

		public AndExpandPage()
		{
			InitializeComponent();

			Toggle.Clicked += (sender, args) =>
			{

#pragma warning disable CS0618 // Type or member is obsolete
				switch (_counter % 5)
				{
					case 1:
						ExpandLabel.VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.CenterAndExpand;
						ExpandLabel.Text = "Expanded Label with CenterAndExpand";
						break;
					case 2:
						ExpandLabel.VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.StartAndExpand;
						ExpandLabel.Text = "Expanded Label with StartAndExpand";
						break;
					case 3:
						ExpandLabel.VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.EndAndExpand;
						ExpandLabel.Text = "Expanded Label with EndAndExpand";
						break;
					case 4:
						ExpandLabel.VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.FillAndExpand;
						ExpandLabel.Text = "Expanded Label with FillAndExpand";
						break;
					default:
						ExpandLabel.VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Center;
						ExpandLabel.Text = "Not Expanded";
						break;
				}
#pragma warning restore CS0618 // Type or member is obsolete

				_counter += 1;
			};
		}
	}
}
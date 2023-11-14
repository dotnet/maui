using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ZIndexPage
	{
		public ZIndexPage()
		{
			InitializeComponent();

			Label? target = null;

			for (int n = 0; n < 10; n++)
			{
				var label = new Label
				{
					Text = $"This is Label {n}, z-index {n}",
					ZIndex = n,
					HeightRequest = 100,
					WidthRequest = 200,
					HorizontalOptions = LayoutOptions.Start,
					VerticalOptions = LayoutOptions.Start,
					Margin = new Thickness(n * 15, n * 15, 0, 0),
					BackgroundColor = PickColor(n)
				};

				Root.Add(label);
				Root.SetRow(label, 1);

				if (n == 5)
				{
					target = label;
				}
			}

			CurrentZIndex.Text = $"Z-Index of Label 5: {target!.ZIndex}";
			ZIndexStepper.Value = target.ZIndex;
			ZIndexStepper.ValueChanged += (sender, args) =>
			{
				target.ZIndex = (int)ZIndexStepper.Value;
				CurrentZIndex.Text = $"Z-Index of Label 5: {target.ZIndex}";
				target.Text = $"This is Label 5, z-index {target.ZIndex}";
			};
		}

		Color[] _colors = new Color[] {
			Colors.Aquamarine, Colors.Orange, Colors.MediumOrchid, Colors.Red, Colors.Green, Colors.Blue
		};

		Color PickColor(int n)
		{
			return _colors[n % _colors.Length];
		}
	}
}
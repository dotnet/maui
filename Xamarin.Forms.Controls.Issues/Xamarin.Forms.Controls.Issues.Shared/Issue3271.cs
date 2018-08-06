using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3271, "The order of transformations is not correct", PlatformAffected.WPF)]
	public class Issue3271 : TestContentPage//TestTabbedPage
	{
		Grid grid;

		void AddChild(string desc, int row, Action<double> onChanged)
		{
			var sliderLabel = new Label();
			var slider = new Slider { Maximum = 360 };
			slider.ValueChanged += (sender, e) =>
			{
				onChanged(e.NewValue);
				sliderLabel.Text = $"{desc} = {(int)e.NewValue}";
			};
			grid.AddChild(slider, 0, row);
			grid.AddChild(sliderLabel, 1, row);
		}

		protected override void Init()
		{
			Content = grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star } );
			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto } );
			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto } );
			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto } );
			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto } );
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star } );
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto } );

			var label = new Label
			{
				Text = "TEXT",
				Rotation = 90,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			grid.AddChild(label, 0, 0);

			AddChild("Scale", 1, v => label.Scale = v);
			AddChild("Rotation", 2, v => label.Rotation = v);
			AddChild("TranslationX", 3, v => label.TranslationX = v);
			AddChild("TranslationY", 4, v => label.TranslationY = v);
		}
	}
}

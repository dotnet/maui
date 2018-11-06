using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1097, "Not resizing elements on rotation", PlatformAffected.iOS)]
	public class Issue1097 : ContentPage
	{
		public Issue1097 ()
		{
			Grid grid = new Grid {
				RowSpacing = 0,
				ColumnSpacing = 0,
			};

			grid.AddRowDef(count: 2);
			grid.AddColumnDef(count : 2);

			grid.Children.Add(new BoxView() { Color = Color.Red });

			var v2 = new BoxView { Color = Color.Blue };
			Grid.SetColumn(v2, 1);
			grid.Children.Add(v2);

			var v3 = new BoxView { Color = Color.Green };
			Grid.SetRow(v3, 1);
			grid.Children.Add(v3);

			var v4 = new BoxView { Color = Color.Purple };
			Grid.SetRow(v4, 1);
			Grid.SetColumn(v4, 1);
			grid.Children.Add(v4);

			Content = grid;
		}
	}
	public static class GridExtensions
	{
		public static void AddRowDef(this Grid grid, double size = 1, GridUnitType type = GridUnitType.Star, int count = 1)
		{
			for (int i = 0; i < count; i++) {
				grid.RowDefinitions.Add(new RowDefinition() {
					Height = new GridLength(size, type)
				});
			}
		}

		public static void AddColumnDef(this Grid grid, double size = 1, GridUnitType type = GridUnitType.Star, int count = 1)
		{
			for (int i = 0; i < count; i++) {
				grid.ColumnDefinitions.Add(new ColumnDefinition() {
					Width = new GridLength(size, type)
				});
			}
		}
	}
}


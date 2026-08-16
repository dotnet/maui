using System;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	public class Issue14305
	{
		const string IssueNumber = "14305";

		[Fact]
		public void ImageRemainsWithinAssignedStarRow()
		{
			if (!string.Equals(
				Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			const double width = 731.43;
			const double height = 331.43;
			const double fixedRowHeight = 150;

			var grid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = fixedRowHeight },
				},
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = 150 },
				},
			};

			var header = new FixedSizeImage
			{
				WidthRequest = 200,
				HeightRequest = 27.05,
			};
			Grid.SetColumnSpan(header, 3);

			var image = new FixedSizeImage
			{
				WidthRequest = 200,
				HeightRequest = 200,
				HorizontalOptions = LayoutOptions.Center,
			};
			Grid.SetRow(image, 1);
			Grid.SetColumn(image, 1);

			grid.Add(header);
			grid.Add(image);

			grid.CrossPlatformMeasure(width, height);
			grid.CrossPlatformArrange(new Rect(0, 0, width, height));

			double starRowTop = header.Bounds.Bottom;
			double starRowBottom = height - fixedRowHeight;

			Assert.True(
				image.Bounds.Top >= starRowTop && image.Bounds.Bottom <= starRowBottom,
				"Image bounds must remain within the assigned star row.");
		}

		sealed class FixedSizeImage : Image
		{
			protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
			{
				return new Size(WidthRequest, HeightRequest);
			}
		}
	}
}

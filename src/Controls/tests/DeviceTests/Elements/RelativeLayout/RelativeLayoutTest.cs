using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.DeviceTests.ImageAnalysis;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Xunit;
using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Layout)]
	public partial class RelativeLayoutTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.UseMauiCompatibility();
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<RelativeLayout, LayoutHandler>();
					handlers.AddHandler<RoundRectangle, RoundRectangleHandler>();
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});

				

			});
		}



		[Fact(DisplayName = "RelativeLayout content disappears when it has a border with a border stroke shape")]
		public async Task RelativeLayoutContentShouldBeAppeared()
		{
			const int radius = 20;

			var relativeLayout = new RelativeLayout()
			{
				BackgroundColor = Colors.Pink
			};

			var label = new Label()
			{
				Text = "Hello, World!",
				VerticalTextAlignment = TextAlignment.Center,
			};

			var shape = new RoundRectangle()
			{
				CornerRadius = new CornerRadius(radius),
			};

			var border = new Border()
			{
				StrokeShape = shape,
				BackgroundColor = Colors.Tan,

			};

			relativeLayout.Children.Add(border, Constraint.Constant(20), Constraint.Constant(20), Constraint.Constant(300), Constraint.Constant(300));
			var bitmap = await GetRawBitmap(relativeLayout, typeof(LayoutHandler));
			Assert.Equal(300, bitmap.Width, 2d);
			Assert.Equal(300, bitmap.Height, 2d);
		}
	}
}

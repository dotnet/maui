using System;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class LayoutChildIntoBoundingRegionTests : BaseTestFixture
	{
		const int Layout_Width = 100;
		const int Margin_Large = 12;
		const int Margin_None = 0;
		const int Margin_Small = 2;
		const int Region_Height = 50;
		const int Region_Width = 150;
		const int Region_X = 5;
		const int Region_Y = 5;
		const int View_Size = 20;

		int Expected_Empty_Region_Height => Region_Height - View_Size;
		int Expected_Empty_Region_Width => Region_Width - View_Size;
		int Expected_Height_Start_End => View_Size;
		int Expected_Region_Right => Region_X + Region_Width;
		int Expected_Width_Start_End => View_Size;
		int Expected_X_Center => Expected_Empty_Region_Width / 2 + Region_X;
		int Expected_Y_Center => Expected_Empty_Region_Height / 2 + Region_Y;

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void Default(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));

			var view = new MockView(margin);

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void Default_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));

			var view = new MockView(margin);

			layout.Children.Add(view);

			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_Region_Right, region.Right);
			Assert.Equal(Layout_Width, layout.Width);

			Assert.Equal(Expected_X_Fill_RTL_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalCenter(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));

			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.Center };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Center,
					"view.HorizontalOptions.Alignment should be Center");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 0.5,
					"view.HorizontalOptions.Alignment.ToDouble() should be 0.5");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Center, target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalCenter_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));

			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.Center };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;
			var center_margin = Math.Ceiling(margin / 2d);

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Center,
					"view.HorizontalOptions.Alignment should be Center");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 0.5,
					"view.HorizontalOptions.Alignment.ToDouble() should be 0.5");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Center_RTL_Plus_Margin(0), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalCenterAndExpand(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.CenterAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Center,
					"view.HorizontalOptions.Alignment should be Center");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 0.5,
					"view.HorizontalOptions.Alignment.ToDouble() should be 0.5");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Center, target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalCenterAndExpand_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.CenterAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Center,
					"view.HorizontalOptions.Alignment should be Center");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 0.5,
					"view.HorizontalOptions.Alignment.ToDouble() should be 0.5");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Center_RTL_Plus_Margin(0), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalEnd(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.End };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.End,
					"view.HorizontalOptions.Alignment should be End");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 1,
					"view.HorizontalOptions.Alignment.ToDouble() should be 1");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_End_Less_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalEnd_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.End };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.End,
					"view.HorizontalOptions.Alignment should be End");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 1,
					"view.HorizontalOptions.Alignment.ToDouble() should be 1");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_End_RTL_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalEndAndExpand(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.EndAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.End,
					"view.HorizontalOptions.Alignment should be End");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 1,
					"view.HorizontalOptions.Alignment.ToDouble() should be 1");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_End_Less_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalEndAndExpand_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.EndAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.End,
					"view.HorizontalOptions.Alignment should be End");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 1,
					"view.HorizontalOptions.Alignment.ToDouble() should be 1");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_End_RTL_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalStart(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.Start };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Start,
					"view.HorizontalOptions.Alignment should be Start");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 0,
					"view.HorizontalOptions.Alignment.ToDouble() should be 0");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalStart_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.Start };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Start,
					"view.HorizontalOptions.Alignment should be Start");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 0,
					"view.HorizontalOptions.Alignment.ToDouble() should be 0");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Start_Less_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalStartAndExpand(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.StartAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Start,
					"view.HorizontalOptions.Alignment should be Start");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 0,
					"view.HorizontalOptions.Alignment.ToDouble() should be 0");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void HorizontalStartAndExpand_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { HorizontalOptions = LayoutOptions.StartAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Start,
					"view.HorizontalOptions.Alignment should be Start");
			Assert.True(view.HorizontalOptions.Alignment.ToDouble() == 0,
					"view.HorizontalOptions.Alignment.ToDouble() should be 0");
			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Fill,
					"view.VerticalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Start_Less_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Start_End, target.Width);
			Assert.Equal(Expected_Height_Fill_Less_Thickness(thickness), target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalCenter(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));

			var view = new MockView(margin) { VerticalOptions = LayoutOptions.Center };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Center,
					"view.VerticalOptions.Alignment should be Center");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 0.5,
					"view.VerticalOptions.Alignment.ToDouble() should be 0.5");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Center, target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalCenter_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));

			var view = new MockView(margin) { VerticalOptions = LayoutOptions.Center };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Center,
					"view.VerticalOptions.Alignment should be Center");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 0.5,
					"view.VerticalOptions.Alignment.ToDouble() should be 0.5");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_RTL_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Center, target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalCenterAndExpand(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.CenterAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Center,
					"view.VerticalOptions.Alignment should be Center");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 0.5,
					"view.VerticalOptions.Alignment.ToDouble() should be 0.5");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Center, target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalCenterAndExpand_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.CenterAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Center,
					"view.VerticalOptions.Alignment should be Center");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 0.5,
					"view.VerticalOptions.Alignment.ToDouble() should be 0.5");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_RTL_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Center, target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalEnd(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.End };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.End,
					"view.VerticalOptions.Alignment should be End");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 1,
					"view.VerticalOptions.Alignment.ToDouble() should be 1");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_End_Less_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalEnd_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.End };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.End,
					"view.VerticalOptions.Alignment should be End");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 1,
					"view.VerticalOptions.Alignment.ToDouble() should be 1");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_RTL_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_End_Less_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalEndAndExpand(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.EndAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.End,
					"view.VerticalOptions.Alignment should be End");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 1,
					"view.VerticalOptions.Alignment.ToDouble() should be 1");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_End_Less_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalEndAndExpand_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.EndAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.End,
					"view.VerticalOptions.Alignment should be End");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 1,
					"view.VerticalOptions.Alignment.ToDouble() should be 1");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_RTL_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_End_Less_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalStart(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.Start };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Start,
					"view.VerticalOptions.Alignment should be Start");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 0,
					"view.VerticalOptions.Alignment.ToDouble() should be 0");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalStart_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.Start };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Start,
					"view.VerticalOptions.Alignment should be Start");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 0,
					"view.VerticalOptions.Alignment.ToDouble() should be 0");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_RTL_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalStartAndExpand(int margin)
		{
			var layout = new StackLayout();
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.StartAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Start,
					"view.VerticalOptions.Alignment should be Start");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 0,
					"view.VerticalOptions.Alignment.ToDouble() should be 0");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		[Theory]
		[InlineData(Margin_None)]
		[InlineData(Margin_Small)]
		[InlineData(Margin_Large)]
		public void VerticalStartAndExpand_RTL(int margin)
		{
			var layout = new StackLayout { FlowDirection = FlowDirection.RightToLeft };
			layout.MockBounds(new Rect(0, 0, Layout_Width, double.PositiveInfinity));
			var view = new MockView(margin) { VerticalOptions = LayoutOptions.StartAndExpand };

			layout.Children.Add(view);
			var region = new Rect(Region_X, Region_Y, Region_Width, Region_Height);

			Compatibility.Layout.LayoutChildIntoBoundingRegionInternal(view, region);

			var target = view.Bounds;
			var thickness = margin * 2;

			Assert.True(view.VerticalOptions.Alignment == LayoutAlignment.Start,
					"view.VerticalOptions.Alignment should be Start");
			Assert.True(view.VerticalOptions.Alignment.ToDouble() == 0,
					"view.VerticalOptions.Alignment.ToDouble() should be 0");
			Assert.True(view.HorizontalOptions.Alignment == LayoutAlignment.Fill,
					"view.HorizontalOptions.Alignment should be Fill");

			Assert.Equal(Expected_X_Fill_RTL_Plus_Margin(margin), target.X);
			Assert.Equal(Expected_Y_Fill_Plus_Margin(margin), target.Y);
			Assert.Equal(Expected_Width_Fill_Less_Thickness(thickness), target.Width);
			Assert.Equal(Expected_Height_Start_End, target.Height);
		}

		int Expected_Height_Fill_Less_Thickness(int thickness) => Region_Height - thickness;

		int Expected_Width_Fill_Less_Thickness(int thickness) => Region_Width - thickness;

		int Expected_X_Center_RTL_Plus_Margin(int margin) => Expected_Empty_Region_Width / 2 + Expected_X_Fill_RTL_Plus_Margin(margin);

		int Expected_X_End_Less_Margin(int margin) => Expected_Empty_Region_Width + Region_X - margin;

		int Expected_X_End_RTL_Plus_Margin(int margin) => Layout_Width - Region_Width - Region_X + margin;

		int Expected_X_Fill_Plus_Margin(int margin) => Region_X + margin;

		int Expected_X_Fill_RTL_Plus_Margin(int margin) => Layout_Width - Expected_Region_Right + margin;

		int Expected_X_Start_Less_Margin(int margin) => Layout_Width - View_Size - Region_X - margin;

		int Expected_Y_End_Less_Margin(int margin) => Expected_Empty_Region_Height + Region_Y - margin;

		int Expected_Y_Fill_Plus_Margin(int margin) => Region_Y + margin;

		class MockView : View
		{
			public MockView(int margin)
			{
				Margin = new Thickness(margin);
				WidthRequest = View_Size;
				HeightRequest = View_Size;
			}
		}
	}
}
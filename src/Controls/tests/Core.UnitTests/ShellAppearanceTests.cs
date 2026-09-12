using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellAppearanceTests : ShellTestBase
	{
		[Fact]
		public void ColorSetCorrectly()
		{
			var testShell = new TestShell(CreateShellItem<FlyoutItem>());
			testShell.Items[0].SetValue(Shell.DisabledColorProperty, Colors.Purple);

			ShellAppearance result = new ShellAppearance();
			result.Ingest(testShell.Items[0]);
			Assert.Equal(Colors.Purple, result.DisabledColor);
		}

		ShellAppearance IngestAppearance(Action<BindableObject> setValues)
		{
			var testShell = new TestShell(CreateShellItem<FlyoutItem>());
			setValues(testShell.Items[0]);
			var result = new ShellAppearance();
			result.Ingest(testShell.Items[0]);
			return result;
		}

		static LinearGradientBrush CreateGradient() =>
			new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop(Colors.Yellow, 0f),
					new GradientStop(Colors.Green, 1f)
				}
			};

		[Fact]
		public void EffectiveTabBarBackground_UsesTabBarBackgroundColor()
		{
			var result = IngestAppearance(item =>
				item.SetValue(Shell.TabBarBackgroundColorProperty, Colors.Red));

			var brush = Assert.IsType<SolidColorBrush>(result.EffectiveTabBarBackground);
			Assert.Equal(Colors.Red, brush.Color);
		}

		[Fact]
		public void EffectiveTabBarBackground_UsesBackgroundBrush()
		{
			var gradient = CreateGradient();
			var result = IngestAppearance(item =>
				item.SetValue(Shell.BackgroundProperty, gradient));

			Assert.Same(gradient, result.EffectiveTabBarBackground);
		}

		[Fact]
		public void EffectiveTabBarBackground_UsesBackgroundColor()
		{
			var result = IngestAppearance(item =>
				item.SetValue(Shell.BackgroundColorProperty, Colors.Green));

			var brush = Assert.IsType<SolidColorBrush>(result.EffectiveTabBarBackground);
			Assert.Equal(Colors.Green, brush.Color);
		}

		[Fact]
		public void EffectiveTabBarBackground_TabBarBackgroundColorWinsOverBackgroundBrush()
		{
			var result = IngestAppearance(item =>
			{
				item.SetValue(Shell.TabBarBackgroundColorProperty, Colors.Red);
				item.SetValue(Shell.BackgroundProperty, CreateGradient());
			});

			var brush = Assert.IsType<SolidColorBrush>(result.EffectiveTabBarBackground);
			Assert.Equal(Colors.Red, brush.Color);
		}

		[Fact]
		public void EffectiveTabBarBackground_BackgroundBrushWinsOverBackgroundColor()
		{
			var gradient = CreateGradient();
			var result = IngestAppearance(item =>
			{
				item.SetValue(Shell.BackgroundProperty, gradient);
				item.SetValue(Shell.BackgroundColorProperty, Colors.Green);
			});

			Assert.Same(gradient, result.EffectiveTabBarBackground);
		}

		[Fact]
		public void EffectiveTabBarBackground_NullWhenNothingSet()
		{
			var result = IngestAppearance(_ => { });

			Assert.Null(result.EffectiveTabBarBackground);
		}

		[Fact]
		public void EffectiveTabBarBackgroundColor_FallsBackToBackgroundColor()
		{
			var result = IngestAppearance(item =>
				item.SetValue(Shell.BackgroundColorProperty, Colors.Green));

			Assert.Equal(Colors.Green, ((IShellAppearanceElement)result).EffectiveTabBarBackgroundColor);
		}

		[Fact]
		public void EffectiveTabBarBackgroundColor_TabBarBackgroundColorWinsOverBackgroundColor()
		{
			var result = IngestAppearance(item =>
			{
				item.SetValue(Shell.TabBarBackgroundColorProperty, Colors.Red);
				item.SetValue(Shell.BackgroundColorProperty, Colors.Green);
			});

			Assert.Equal(Colors.Red, ((IShellAppearanceElement)result).EffectiveTabBarBackgroundColor);
		}
	}
}

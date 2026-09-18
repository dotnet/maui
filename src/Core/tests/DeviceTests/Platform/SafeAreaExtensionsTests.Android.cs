using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using AInsets = AndroidX.Core.Graphics.Insets;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Layout)]
	public class SafeAreaExtensionsTests : TestBase
	{
		const int TopInset = 100;

		[Theory]
		[InlineData(0, true, TopInset)]
		[InlineData(1, true, TopInset)]
		[InlineData(TopInset - 1, true, TopInset)]
		[InlineData(TopInset, true, TopInset)]
		[InlineData(TopInset + 1, true, TopInset)]
		[InlineData(1, false, 0)]
		public async Task TopInsetRetainedOnlyForFullHeightTransitions(int offset, bool fullHeight, int expectedTopInset)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var context = MauiProgram.DefaultContext;
				// Resolve the typed Java peer before the inset helper calls GetSystemService.
				var windowManager = context.GetActivity()?.WindowManager;
				Assert.NotNull(windowManager);
				Assert.IsAssignableFrom<IWindowManager>(context.GetSystemService(Context.WindowService));
				int screenWidth;
				int screenHeight;
				if (OperatingSystem.IsAndroidVersionAtLeast(30))
				{
					using var windowMetrics = windowManager.MaximumWindowMetrics;
					using var bounds = windowMetrics.Bounds;
					screenWidth = bounds.Width();
					screenHeight = bounds.Height();
				}
				else
				{
					using var metrics = new DisplayMetrics();
					Assert.NotNull(windowManager.DefaultDisplay);
					windowManager.DefaultDisplay.GetRealMetrics(metrics);
					screenWidth = metrics.WidthPixels;
					screenHeight = metrics.HeightPixels;
				}

				var height = fullHeight ? screenHeight : screenHeight / 2;
				var top = fullHeight ? offset : screenHeight - height + offset;
				using var view = new PositionedView(context, top);
				view.Layout(0, 0, screenWidth, height);

				using var builder = new WindowInsetsCompat.Builder();
				using var insets = builder
					.SetInsets(WindowInsetsCompat.Type.SystemBars(), AInsets.Of(0, TopInset, 0, 40))
					.Build();
				using var remainingInsets = SafeAreaExtensions.ApplyAdjustedSafeAreaInsetsPx(
					insets, new SafeAreaLayoutStub(), context, view);

				Assert.Equal(expectedTopInset, view.PaddingTop);
				Assert.NotNull(remainingInsets);
				Assert.Equal(expectedTopInset > 0 ? 0 : TopInset,
					remainingInsets.GetInsets(WindowInsetsCompat.Type.SystemBars()).Top);
			});
		}

		class SafeAreaLayoutStub : LayoutStub, ISafeAreaView2
		{
			public bool HasExplicitSafeAreaEdges => true;

			public Thickness SafeAreaInsets { get; set; }

			public SafeAreaRegions GetSafeAreaRegionsForEdge(int edge) => SafeAreaRegions.Container;
		}

		class PositionedView : AView
		{
			readonly int _top;

			public PositionedView(Context context, int top) : base(context)
			{
				_top = top;
			}

			public override void GetLocationOnScreen(int[] outLocation)
			{
				ArgumentNullException.ThrowIfNull(outLocation);
				outLocation[0] = 0;
				outLocation[1] = _top;
			}
		}
	}
}

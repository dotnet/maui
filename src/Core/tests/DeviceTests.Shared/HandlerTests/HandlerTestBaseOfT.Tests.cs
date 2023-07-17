using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Media;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class HandlerTestBase<THandler, TStub>
	{
		[Fact]
		public async Task DisconnectHandlerDoesntCrash()
		{
			var handler = await CreateHandlerAsync(new TStub()) as IPlatformViewHandler;
			await InvokeOnMainThreadAsync(() =>
			{
				handler.DisconnectHandler();
			});
		}

		[Fact(DisplayName = "Automation Id is set correctly")]
		public async Task SetAutomationId()
		{
			var view = new TStub
			{
				AutomationId = "TestId"
			};
			var id = await GetValueAsync(view, handler => GetAutomationId(handler));
			Assert.Equal(view.AutomationId, id);
		}

		[Theory(DisplayName = "FlowDirection is set correctly")]
		[InlineData(FlowDirection.LeftToRight)]
		[InlineData(FlowDirection.RightToLeft)]
		public async Task SetFlowDirection(FlowDirection flowDirection)
		{
			var view = new TStub
			{
				FlowDirection = flowDirection
			};

			var id = await GetValueAsync(view, handler => GetFlowDirection(handler));
			Assert.Equal(flowDirection, id);
		}

		[Theory(DisplayName = "Opacity is set correctly")]
		[InlineData(0)]
		[InlineData(0.25)]
		[InlineData(0.5)]
		[InlineData(0.75)]
		[InlineData(1)]
		public async Task SetOpacity(double opacity)
		{
			var view = new TStub
			{
				Opacity = opacity
			};
			var id = await GetValueAsync(view, handler => GetOpacity(handler));
			Assert.Equal(view.Opacity, id);
		}

		[Theory(DisplayName = "Visibility is set correctly")]
		[InlineData(Visibility.Collapsed)]
		[InlineData(Visibility.Hidden)]
		public virtual async Task SetVisibility(Visibility visibility)
		{
			var view = new TStub
			{
				Visibility = visibility
			};

			var id = await GetValueAsync(view, handler => GetVisibility(handler));
			Assert.Equal(view.Visibility, id);
		}

		[Fact(DisplayName = "Setting Semantic Description makes element accessible")]
		public async virtual Task SettingSemanticDescriptionMakesElementAccessible()
		{
			var view = new TStub();
			MockAccessibilityExpectations(view);

			view.Semantics.Description = "Test";
			var important = await GetValueAsync(view, handler => view.IsAccessibilityElement());

			Assert.True(important);
		}

		[Fact(DisplayName = "Setting Semantic Hint makes element accessible")]
		public async virtual Task SettingSemanticHintMakesElementAccessible()
		{
			var view = new TStub();
			MockAccessibilityExpectations(view);

			view.Semantics.Hint = "Test";
			var important = await GetValueAsync(view, handler => view.IsAccessibilityElement());

			Assert.True(important);
		}

		[Fact(DisplayName = "Semantic Description is set correctly"
#if ANDROID
			, Skip = "This value can't be validated through automated tests"
#endif
		)]
		public async Task SetSemanticDescription()
		{
			var view = new TStub();
			view.Semantics.Description = "Test";
			var id = await GetValueAsync(view, handler => GetSemanticDescription(handler));
			Assert.Equal(view.Semantics.Description, id);
		}

		[Fact(DisplayName = "Semantic Hint is set correctly"
#if ANDROID
			, Skip = "This value can't be validated through automated tests"
#endif
		)]
		public async Task SetSemanticHint()
		{
			var view = new TStub();
			view.Semantics.Hint = "Test";
			var id = await GetValueAsync(view, handler => GetSemanticHint(handler));
			Assert.Equal(view.Semantics.Hint, id);
		}

		[Fact(DisplayName = "Semantic Heading is set correctly")]
		public async Task SetSemanticHeading()
		{
			var view = new TStub();
			view.Semantics.HeadingLevel = SemanticHeadingLevel.Level1;
			var id = await GetValueAsync(view, handler => GetSemanticHeading(handler));
			Assert.Equal(view.Semantics.HeadingLevel, id);
		}

		[Fact(DisplayName = "Null Semantics Doesnt throw exception")]
		public async Task NullSemanticsClass()
		{
			var view = new TStub
			{
				Semantics = null,
				AutomationId = "CreationFailed"
			};
			var id = await GetValueAsync(view, handler => GetAutomationId(handler));
			Assert.Equal(view.AutomationId, id);
		}

		[Fact(DisplayName = "Clip Initializes ContainerView Correctly")]
		public async virtual Task ContainerViewInitializesCorrectly()
		{
			var view = new TStub
			{
				Height = 100,
				Width = 100,
				Background = new SolidPaintStub(Colors.Red),
				Clip = new EllipseGeometryStub(new Graphics.Point(50, 50), 50, 50)
			};

			await CreateHandlerAsync(view);
			await view.AssertHasContainer(true);
		}

		[Fact(DisplayName = "ContainerView Remains If Shadow Mapper Runs Again")]
		public virtual async Task ContainerViewRemainsIfShadowMapperRunsAgain()
		{
			var view = new TStub
			{
				Height = 100,
				Width = 100,
				Background = new SolidPaintStub(Colors.Red),
				Clip = new EllipseGeometryStub(new Graphics.Point(50, 50), 50, 50)
			};

			var handler = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(view);
				handler.UpdateValue(nameof(IView.Shadow));
				return handler;
			});

			await view.AssertHasContainer(true);
		}

		[Fact(DisplayName = "ContainerView Adds And Removes")]
		public virtual async Task ContainerViewAddsAndRemoves()
		{
			var view = new TStub
			{
				Height = 100,
				Width = 100
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(view);

				// This is a view that always has a container
				// so there's nothing to test here
				if (handler.HasContainer)
					return;

				await AttachAndRun(view,
					async (handler) =>
					{
						await view.AssertHasContainer(false);
						view.Clip = new EllipseGeometryStub(new Graphics.Point(50, 50), 50, 50);
						handler.UpdateValue(nameof(IView.Clip));
						await Task.Delay(10);
						await view.AssertHasContainer(true);
						view.Clip = null;
						handler.UpdateValue(nameof(IView.Clip));
						await Task.Delay(10);
						await view.AssertHasContainer(false);
					});

				return;
			});
		}

		[Theory(DisplayName = "Native View Bounds are not empty"
#if WINDOWS
			, Skip = "https://github.com/dotnet/maui/issues/9054"
#endif
		)]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(1000)]
		public async Task ReturnsNonEmptyPlatformViewBounds(int size)
		{
			var view = new TStub()
			{
				Height = size,
				Width = size,
			};

			var platformViewBounds = await GetValueAsync(view, handler => GetPlatformViewBounds(handler));
			Assert.NotEqual(platformViewBounds, new Graphics.Rect());
		}

		[Theory(DisplayName = "Native View Bounding Box is not empty"
#if WINDOWS
			, Skip = "https://github.com/dotnet/maui/issues/9054"
#endif
		)]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(1000)]
		public virtual async Task ReturnsNonEmptyNativeBoundingBox(int size)
		{
			var view = new TStub()
			{
				Height = size,
				Width = size,
			};

			var nativeBoundingBox = await GetValueAsync(view, handler => GetBoundingBox(handler));
			Assert.NotEqual(nativeBoundingBox, Graphics.Rect.Zero);

			// Currently there's an issue with label/progress where they don't set the frame size to
			// the explicit Width and Height values set
			// https://github.com/dotnet/maui/issues/7935
			if (view is ILabel)
			{
				// TODO:
			}
			else if (view is IBorderView)
			{
				// TODO:
			}
#if IOS || MACCATALYST
			else if (view is ISearchBar)
			{
				//Search bar currently only measures to one size
				//https://github.com/dotnet/maui/issues/11136
			}
#endif
#if ANDROID
			else if (size == 1 && view is ISwitch)
			{
				// https://github.com/dotnet/maui/issues/11020
			}
#endif
			else if (view is IProgress)
			{
				AssertWithinTolerance(size, nativeBoundingBox.Size.Width);
			}
			else
			{
				var expectedSize = new Size(size, size);
				AssertWithinTolerance(expectedSize, nativeBoundingBox.Size);
			}
		}

		[Theory(DisplayName = "PlatformView Transforms are not empty")]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(1000)]
		public async Task ReturnsNonEmptyPlatformViewTransforms(int size)
		{
			var view = new TStub()
			{
				Height = size,
				Width = size,
				Scale = .5,
				Rotation = size,
			};

			var platformViewTransform = await GetValueAsync(view, handler => GetViewTransform(handler));
			Assert.NotEqual(platformViewTransform, new System.Numerics.Matrix4x4());
		}

		[Theory(DisplayName = "View Renders To Image"
#if !ANDROID
			, Skip = "iOS and Windows can't render elements to images from test runner. It's missing the required root windows."
#endif
			)]
		[InlineData(ScreenshotFormat.Jpeg)]
		[InlineData(ScreenshotFormat.Png)]
		public async Task RendersAsImage(ScreenshotFormat type)
		{
			var view = new TStub()
			{
				Height = 100,
				Width = 100,
			};

			var result = await GetValueAsync(view, handler => handler.VirtualView.CaptureAsync());
			Assert.NotNull(result);

			using var stream = await result.OpenReadAsync(type);

			Assert.True(stream.Length > 0);
		}

		[Fact]
		public void HandlersHaveAllExpectedContructors()
		{
			bool hasBothMappers = false;
			var constructors = typeof(THandler).GetConstructors();

			foreach (var ctor in constructors)
			{
				var args = ctor.GetParameters();
				if (args.Length == 2)
				{
					if (args[0].ParameterType.IsAssignableTo(typeof(IPropertyMapper)) &&
						args[1].ParameterType.IsAssignableTo(typeof(CommandMapper)))
					{
						hasBothMappers = true;
					}
				}
			}

			Assert.True(hasBothMappers, "Missing constructor with IPropertyMapper and ICommandMapper");
		}
	}
}

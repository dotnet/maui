using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Regression tests for the initial-connect/reconnect de-duplication of the
	/// <c>BackgroundColor</c>, <c>BackgroundImageSource</c>, and <c>SemanticProperties.*</c> mapper
	/// entries in <see cref="VisualElement"/>.RemapForControls (see VisualElement.Mapper.cs).
	/// </summary>
	public class VisualElementMapperTests : BaseTestFixture
	{
		// The Background/Semantics de-duplication relies on the Core `nameof(IView.Background)` and
		// `nameof(IView.Semantics)` keys always running before the Controls-appended
		// `BackgroundColor`/`BackgroundImageSource`/`SemanticProperties.*` keys during the same bulk
		// mapping pass. Guard that invariant so a future mapper reordering fails loudly instead of
		// silently reintroducing the redundant work (or dropping the redirect entirely).
		[Fact]
		public void BackgroundKeyIsMappedBeforeControlsBackgroundColorAndImageSourceKeys()
		{
			var keys = ViewHandler.ViewMapper.GetKeys().ToList();

			var backgroundIndex = keys.IndexOf(nameof(IView.Background));
			var backgroundColorIndex = keys.IndexOf(nameof(VisualElement.BackgroundColor));
			var backgroundImageSourceIndex = keys.IndexOf(nameof(Page.BackgroundImageSource));

			Assert.True(backgroundIndex >= 0);
			Assert.True(backgroundColorIndex >= 0);
			Assert.True(backgroundImageSourceIndex >= 0);
			Assert.True(backgroundIndex < backgroundColorIndex);
			Assert.True(backgroundIndex < backgroundImageSourceIndex);
		}

		[Fact]
		public void SemanticsKeyIsMappedBeforeControlsSemanticPropertiesKeys()
		{
			var keys = ViewHandler.ViewMapper.GetKeys().ToList();

			var semanticsIndex = keys.IndexOf(nameof(IView.Semantics));
			var descriptionIndex = keys.IndexOf(SemanticProperties.DescriptionProperty.PropertyName);
			var hintIndex = keys.IndexOf(SemanticProperties.HintProperty.PropertyName);
			var headingLevelIndex = keys.IndexOf(SemanticProperties.HeadingLevelProperty.PropertyName);

			Assert.True(semanticsIndex >= 0);
			Assert.True(descriptionIndex >= 0);
			Assert.True(hintIndex >= 0);
			Assert.True(headingLevelIndex >= 0);
			Assert.True(semanticsIndex < descriptionIndex);
			Assert.True(semanticsIndex < hintIndex);
			Assert.True(semanticsIndex < headingLevelIndex);
		}

		[Fact]
		public void Connect_BackgroundColorOnly_AppliesComposedBackgroundExactlyOnce()
		{
			int backgroundCalls = 0;
			Paint capturedBackground = null;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Background)] = (h, v) =>
				{
					backgroundCalls++;
					capturedBackground = v.Background;
				},
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button { BackgroundColor = Colors.Red };

			handlerStub.SetVirtualView(button);

			Assert.Equal(1, backgroundCalls);
			// VisualElement's IView.Background implicitly converts the SolidColorBrush composite value
			// to a Microsoft.Maui.Graphics.SolidPaint (see Brush's implicit Paint conversion operator).
			var paint = Assert.IsType<SolidPaint>(capturedBackground);
			Assert.Equal(Colors.Red, paint.Color);
		}

		[Fact]
		public void Connect_BackgroundImageSourceOnly_AppliesComposedBackgroundExactlyOnce()
		{
			int backgroundCalls = 0;
			Paint capturedBackground = null;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Background)] = (h, v) =>
				{
					backgroundCalls++;
					capturedBackground = v.Background;
				},
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new PageHandlerStub(mapper, commandMapper);
			var page = new ContentPage { BackgroundImageSource = ImageSource.FromFile("some_image.png") };

			handlerStub.SetVirtualView(page);

			Assert.Equal(1, backgroundCalls);
			Assert.IsType<ImageSourcePaint>(capturedBackground);
		}

		[Fact]
		public void Connect_SemanticDescriptionOnly_AppliesSemanticsExactlyOnce()
		{
			int semanticsCalls = 0;
			Semantics capturedSemantics = null;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Semantics)] = (h, v) =>
				{
					semanticsCalls++;
					capturedSemantics = v.Semantics;
				},
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button();
			SemanticProperties.SetDescription(button, "Submit order");

			handlerStub.SetVirtualView(button);

			Assert.Equal(1, semanticsCalls);
			Assert.Equal("Submit order", capturedSemantics?.Description);
		}

		[Fact]
		public void Connect_UserAppendedBackgroundColorMapping_StillRunsDuringConnect()
		{
			bool appendCalled = false;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper);
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			// Simulate a consumer customization registered on top of the canonical BackgroundColor mapping.
			mapper.AppendToMapping<IView, IViewHandler>(nameof(VisualElement.BackgroundColor), (h, v) => appendCalled = true);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button { BackgroundColor = Colors.Blue };

			handlerStub.SetVirtualView(button);

			Assert.True(appendCalled);
		}

		[Fact]
		public void HandlerReuse_Reconnect_DoesNotDuplicateBackgroundMapping()
		{
			int backgroundCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Background)] = (h, v) => backgroundCalls++,
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var firstView = new Button { BackgroundColor = Colors.Red };
			handlerStub.SetVirtualView(firstView);

			Assert.Equal(1, backgroundCalls);

			// Simulate handler reuse (e.g. CollectionView/CarouselView cell recycling): the same handler
			// instance, whose PlatformView already exists, is bound directly to a new virtual view. This
			// puts the handler in the `Reconnecting` state rather than `Connecting`.
			backgroundCalls = 0;
			var secondView = new Button { BackgroundColor = Colors.Green };
			handlerStub.SetVirtualView(secondView);

			Assert.Equal(1, backgroundCalls);
		}

		[Fact]
		public void HandlerReuse_Reconnect_DoesNotDuplicateSemanticsMapping()
		{
			int semanticsCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Semantics)] = (h, v) => semanticsCalls++,
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var firstView = new Button();
			SemanticProperties.SetDescription(firstView, "First");
			handlerStub.SetVirtualView(firstView);

			Assert.Equal(1, semanticsCalls);

			semanticsCalls = 0;
			var secondView = new Button();
			SemanticProperties.SetDescription(secondView, "Second");
			handlerStub.SetVirtualView(secondView);

			Assert.Equal(1, semanticsCalls);
		}

		[Fact]
		public void DynamicUpdateAfterConnect_BackgroundColorChange_StillAppliesBackground()
		{
			int backgroundCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Background)] = (h, v) => backgroundCalls++,
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button { BackgroundColor = Colors.Red };
			handlerStub.SetVirtualView(button);

			// The handler is now fully `Connected`, so a dynamic property update must still forward
			// through to the canonical Background mapping and must not be suppressed by the
			// connect/reconnect de-duplication guard.
			backgroundCalls = 0;
			button.BackgroundColor = Colors.Purple;

			Assert.Equal(1, backgroundCalls);
		}

		[Fact]
		public void DynamicUpdateAfterConnect_SemanticDescriptionChange_StillAppliesSemantics()
		{
			int semanticsCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Semantics)] = (h, v) => semanticsCalls++,
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button();
			SemanticProperties.SetDescription(button, "Initial");
			handlerStub.SetVirtualView(button);

			semanticsCalls = 0;
			SemanticProperties.SetDescription(button, "Updated");

			Assert.Equal(1, semanticsCalls);
		}

		class PageHandlerStub : ViewHandler<ContentPage, object>
		{
			public PageHandlerStub(IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
			{
			}

			protected override object CreatePlatformView() => new object();
		}
	}
}

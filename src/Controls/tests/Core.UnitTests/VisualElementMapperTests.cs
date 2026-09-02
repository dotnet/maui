using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Regression tests for the initial-connect/reconnect de-duplication of the
	/// <c>BackgroundColor</c>, <c>BackgroundImageSource</c>, and <c>SemanticProperties.*</c> mapper
	/// entries in <see cref="VisualElement"/>.RemapForControls (see VisualElement.Mapper.cs). The
	/// de-duplication is scoped to a single bulk mapping pass (tracked per-handler), so it never
	/// suppresses an explicit/reentrant call, a same-value call, a call made before the canonical
	/// `Background`/`Semantics` mapping has ever run, or a call made on a subsequent connect/reconnect.
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

		// De-duplication is scoped to a single bulk mapping pass, not to whether the value itself changed.
		// An explicit call requesting the SAME, unchanged value - made outside of any pass, e.g. well after
		// connect has completed - must still go through every time it is made.
		[Fact]
		public void ExplicitSameValueBackgroundColorUpdateAfterConnect_StillApplies()
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
			Assert.Equal(1, backgroundCalls);

			// Same value as already applied, requested twice in a row, well outside of any bulk pass.
			handlerStub.UpdateValue(nameof(VisualElement.BackgroundColor));
			Assert.Equal(2, backgroundCalls);

			handlerStub.UpdateValue(nameof(VisualElement.BackgroundColor));
			Assert.Equal(3, backgroundCalls);
		}

		// Same reasoning as ExplicitSameValueBackgroundColorUpdateAfterConnect_StillApplies, but for the
		// SemanticProperties.Description redirect.
		[Fact]
		public void ExplicitSameValueSemanticDescriptionUpdateAfterConnect_StillApplies()
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
			SemanticProperties.SetDescription(button, "Same");

			handlerStub.SetVirtualView(button);
			Assert.Equal(1, semanticsCalls);

			handlerStub.UpdateValue(SemanticProperties.DescriptionProperty.PropertyName);
			Assert.Equal(2, semanticsCalls);

			handlerStub.UpdateValue(SemanticProperties.DescriptionProperty.PropertyName);
			Assert.Equal(3, semanticsCalls);
		}

		// Reproduces a mapper that runs *later* in the same bulk connect pass than BackgroundColor's own
		// turn and legitimately changes BackgroundColor, forcing a further, re-entrant handler notification
		// (property changes on VisualElement route straight to Handler.UpdateValue - see
		// Element.OnPropertyChanged). That call must not be suppressed just because BackgroundColor's own
		// turn already ran (and was a no-op) earlier in the very same pass.
		[Fact]
		public void Connect_LaterMapperChangesBackgroundColor_ReentrantUpdateStillApplies()
		{
			int backgroundCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Background)] = (h, v) => backgroundCalls++,
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			// A brand new key is guaranteed to be enumerated after every key RemapForControls/the real
			// ViewHandler.ViewMapper already registers (including BackgroundColor), simulating a mapper
			// that legitimately runs later in the same bulk pass. Setting BackgroundColor here
			// automatically triggers Handler.UpdateValue(nameof(BackgroundColor)) - see
			// Element.OnPropertyChanged - which is the re-entrant call under test.
			mapper.Add("__LaterMapperReentry", (h, v) => ((VisualElement)v).BackgroundColor = Colors.Green);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button { BackgroundColor = Colors.Red };

			handlerStub.SetVirtualView(button);

			// Once for the natural Background key entry (BackgroundColor's own turn is then a no-op and
			// is skipped), once more for the later mapper's re-entrant notification after BackgroundColor
			// legitimately changed.
			Assert.Equal(2, backgroundCalls);
			Assert.Equal(Colors.Green, button.BackgroundColor);
		}

		// Same as above, but for handler reuse/reconnect: the later-mapper re-entrant call must still not
		// be suppressed on a reused handler.
		[Fact]
		public void Reconnect_LaterMapperChangesBackgroundColor_ReentrantUpdateStillApplies()
		{
			int backgroundCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Background)] = (h, v) => backgroundCalls++,
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			mapper.Add("__LaterMapperReentry", (h, v) => ((VisualElement)v).BackgroundColor = Colors.Purple);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var firstView = new Button { BackgroundColor = Colors.Red };
			handlerStub.SetVirtualView(firstView);

			Assert.Equal(2, backgroundCalls);
			Assert.Equal(Colors.Purple, firstView.BackgroundColor);

			backgroundCalls = 0;
			var secondView = new Button { BackgroundColor = Colors.Blue };
			handlerStub.SetVirtualView(secondView);

			Assert.Equal(2, backgroundCalls);
			Assert.Equal(Colors.Purple, secondView.BackgroundColor);
		}

		// Same reasoning as Connect_LaterMapperChangesBackgroundColor_ReentrantUpdateStillApplies, but for
		// the SemanticProperties.Description redirect.
		[Fact]
		public void Connect_LaterMapperChangesSemanticDescription_ReentrantUpdateStillApplies()
		{
			int semanticsCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Semantics)] = (h, v) => semanticsCalls++,
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			mapper.Add("__LaterMapperReentry", (h, v) => SemanticProperties.SetDescription((VisualElement)v, "Changed later"));

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button();
			SemanticProperties.SetDescription(button, "Initial");

			handlerStub.SetVirtualView(button);

			Assert.Equal(2, semanticsCalls);
			Assert.Equal("Changed later", SemanticProperties.GetDescription(button));
		}

		// Reproduces a handler that explicitly primes BackgroundColor from within ConnectHandler - i.e.
		// before the Background key has ever been mapped for this element/handler. That explicit call must
		// not be suppressed just because the handler is in the Connecting state.
		[Fact]
		public void ConnectHandler_ExplicitEarlyBackgroundColorUpdate_IsNotSuppressed()
		{
			int backgroundCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Background)] = (h, v) => backgroundCalls++,
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new EarlyBackgroundColorConnectHandlerStub(mapper, commandMapper);
			var button = new Button { BackgroundColor = Colors.Red };

			handlerStub.SetVirtualView(button);

			// One call from ConnectHandler's explicit early UpdateValue(BackgroundColor) - which must not
			// be suppressed, since Background has never been mapped for this element yet - and one more
			// from the bulk pass's own, always-unconditional Background key entry. BackgroundColor's own
			// turn later in that same bulk pass is then a genuine no-op and is skipped.
			Assert.Equal(2, backgroundCalls);
		}

		// A consumer fully replacing the BackgroundColor mapping via ReplaceMapping must run their own
		// logic instead of MapBackgroundColorForControls; our de-duplication logic must not interfere.
		[Fact]
		public void ReplaceMapping_UserReplacesBackgroundColorMapping_UserMappingRunsInstead()
		{
			bool userMappingRan = false;
			int backgroundCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Background)] = (h, v) => backgroundCalls++,
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			mapper.ReplaceMapping<IView, IViewHandler>(nameof(VisualElement.BackgroundColor), (h, v) => userMappingRan = true);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button { BackgroundColor = Colors.Red };

			handlerStub.SetVirtualView(button);

			Assert.True(userMappingRan);
			// Only the natural Background key entry runs; MapBackgroundColorForControls (and our
			// de-dup logic) isn't reachable anymore since ReplaceMapping fully discarded it.
			Assert.Equal(1, backgroundCalls);
		}

		// A consumer using PrependToMapping on the Background key itself must have their mapping run
		// before the canonical mapping, and our de-duplication must keep working correctly afterward.
		[Fact]
		public void PrependToMapping_UserPrependsToBackgroundMapping_RunsBeforeCanonicalMappingAndDedupStillWorks()
		{
			var order = new List<string>();
			int backgroundCalls = 0;

			var mapper = new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IView.Background)] = (h, v) =>
				{
					order.Add("canonical");
					backgroundCalls++;
				},
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper);
			VisualElement.RemapForControls(mapper, commandMapper);

			mapper.PrependToMapping<IView, IViewHandler>(nameof(IView.Background), (h, v) => order.Add("prepend"));

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button { BackgroundColor = Colors.Red };

			handlerStub.SetVirtualView(button);

			Assert.Equal(new[] { "prepend", "canonical" }, order);
			// The redirect is still correctly deduplicated even with the user's prepended mapping.
			Assert.Equal(1, backgroundCalls);
		}

		class PageHandlerStub : ViewHandler<ContentPage, object>
		{
			public PageHandlerStub(IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
			{
			}

			protected override object CreatePlatformView() => new object();
		}

		class EarlyBackgroundColorConnectHandlerStub : ViewHandler<Button, object>
		{
			public EarlyBackgroundColorConnectHandlerStub(IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
			{
			}

			protected override object CreatePlatformView() => new object();

			protected override void ConnectHandler(object platformView)
			{
				base.ConnectHandler(platformView);

				// Simulate a handler that explicitly primes BackgroundColor from within ConnectHandler,
				// before the Background key has ever been mapped for this element/handler.
				UpdateValue(nameof(VisualElement.BackgroundColor));
			}
		}
	}
}

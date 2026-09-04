using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Regression tests for the bulk-pass de-duplication of the <c>BackgroundColor</c>,
	/// <c>BackgroundImageSource</c>, and <c>SemanticProperties.*</c> mapper entries in
	/// <see cref="VisualElement"/>.RemapForControls (see VisualElement.Mapper.cs). Each of those keys is a
	/// pure redirect into the canonical <c>Background</c>/<c>Semantics</c> key, and is registered after it,
	/// so its own step of a bulk <c>UpdateProperties</c> pass is provably redundant. Exactly that one step
	/// is skipped, via <see cref="Microsoft.Maui.PropertyMapperPassScope"/>.
	/// <para>
	/// The de-duplication is deliberately independent of <em>which</em> mapping the canonical key resolves
	/// to, so it behaves identically when a real derived handler (ButtonHandler, PageHandler, ...) owns and
	/// overrides that key. And it never suppresses anything else: not an explicit or re-entrant call, a
	/// same-value call, a call made before the canonical mapping has ever run, a call made while a nested
	/// pass is on top, a call triggered by `Background`/`Semantics` running outside of any bulk pass (e.g.
	/// `PickerHandler.Windows.MapTitle`), nor a call made after a pass was aborted by an exception.
	/// </para>
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

		// De-duplication must be scoped to *this specific* bulk UpdateProperties pass, not to "the next
		// time Background happens to run" regardless of context. If Background is (re)mapped by something
		// standalone, entirely outside of any bulk pass (e.g. a direct Handler.UpdateValue(nameof(Background))
		// call made well after connect has finished), nothing becomes skippable - there is no bulk pass in
		// which BackgroundColor's own redirect turn is still coming, so a later, completely unrelated
		// BackgroundColor update must still apply.
		[Fact]
		public void DirectBackgroundUpdateOutsideBulkPass_DoesNotSuppressLaterUnrelatedBackgroundColorUpdate()
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

			// Something remaps Background directly and standalone, well outside of the connect pass and
			// outside of either redirect (e.g. not through MapBackgroundColor/MapBackgroundImageSource).
			backgroundCalls = 0;
			handlerStub.UpdateValue(nameof(IView.Background));
			Assert.Equal(1, backgroundCalls);

			// A subsequent, unrelated explicit BackgroundColor update must still apply - it must not be
			// wrongly treated as the "redundant" follow-up to the direct Background update above, since
			// that update did not happen within any bulk UpdateProperties pass.
			handlerStub.UpdateValue(nameof(VisualElement.BackgroundColor));
			Assert.Equal(2, backgroundCalls);
		}

		// Mirrors PickerHandler.Windows.MapTitle, which reacts to Title changing - including well after
		// connect, entirely outside of any bulk UpdateProperties pass - by directly calling
		// handler.UpdateValue(nameof(IView.Semantics)). That call must not be able to suppress a later,
		// unrelated SemanticProperties.Description update.
		[Fact]
		public void HandlerDirectSemanticsUpdateOutsideBulkPass_DoesNotSuppressLaterUnrelatedSemanticDescriptionUpdate()
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

			Assert.Equal(1, semanticsCalls);

			// Register the Title mapper only now (after connect has fully finished), so that its own
			// invocation below happens strictly outside of any bulk UpdateProperties pass - simulating a
			// platform mapper, like PickerHandler.Windows.MapTitle, that reacts to an unrelated property
			// (here, a synthetic "Title" key) by directly calling handler.UpdateValue(nameof(IView.Semantics))
			// well after connect, e.g. because Title changed dynamically.
			mapper.Add("Title", (h, v) => h.UpdateValue(nameof(IView.Semantics)));

			// "Title" changes well after connect, entirely outside of any bulk pass - mirroring
			// PickerHandler.Windows.MapTitle firing from a standalone Title property change.
			semanticsCalls = 0;
			handlerStub.UpdateValue("Title");
			Assert.Equal(1, semanticsCalls);

			// A subsequent, unrelated explicit SemanticProperties.Description update must still apply.
			handlerStub.UpdateValue(SemanticProperties.DescriptionProperty.PropertyName);
			Assert.Equal(2, semanticsCalls);
		}

		// If a bulk UpdateProperties pass is aborted by an exception after Background has been mapped but
		// before BackgroundColor's redirect turn is ever reached, the pass must be popped on the way out -
		// so a later, unrelated BackgroundColor update made outside of that (now-ended) pass can never be
		// mistaken for its skippable turn.
		[Fact]
		public void ExceptionDuringBulkPass_DoesNotLeaveAStaleActivePassForLaterUnrelatedUpdate()
		{
			int backgroundCalls = 0;

			// Inserted (via the object initializer, before RemapForControls runs) directly between
			// Background and BackgroundColor's insertion-order position: RemapForControls only ever
			// replaces/appends keys, it never reorders or removes ones already present, and
			// PropertyMapper.GetKeys() yields a mapper's own keys in insertion order. Background is
			// inserted first (right here), this exploding key second, then RemapForControls appends
			// BackgroundColor after both.
			var mapper = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (h, v) => backgroundCalls++,
				["__ExplodingKey"] = (h, v) => throw new InvalidOperationException("Simulated mapper failure mid-pass."),
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>();
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			var button = new Button { BackgroundColor = Colors.Red };

			// The bulk pass aborts partway through: Background has already been mapped, but the pass ends -
			// via PropertyMapper.UpdateProperties' finally - before BackgroundColor's own redirect turn is
			// ever reached.
			Assert.Throws<InvalidOperationException>(() => handlerStub.SetVirtualView(button));
			Assert.Equal(1, backgroundCalls);

			// A later, unrelated explicit BackgroundColor update - entirely outside of the aborted pass -
			// must still apply, since PropertyMapperPassScope no longer reports any pass as current for
			// this handler.
			handlerStub.UpdateValue(nameof(VisualElement.BackgroundColor));
			Assert.Equal(2, backgroundCalls);
		}

		// Most real handlers (ButtonHandler, PageHandler/ContentViewHandler, LabelHandler, EntryHandler,
		// LayoutHandler, ...) declare their *own* `Background` key, which shadows ViewHandler.ViewMapper's
		// entry entirely. The de-duplication must therefore never depend on wrapping/observing the
		// canonical mapping itself - it only depends on the canonical key having already had its turn
		// earlier in the same bulk pass, whichever mapping that key resolves to.
		//
		// Note that VisualElement.RemapForControls is deliberately *not* applied to the mappers built
		// below: the Controls redirect keys reach them through the chained, globally-remapped
		// ViewHandler.ViewMapper, exactly as they do for every real handler at runtime.

		[Fact]
		public void RealDerivedHandlerMappers_MapCanonicalKeysBeforeTheControlsRedirectKeys()
		{
			var mappers = new (string Name, IPropertyMapper Mapper)[]
			{
				(nameof(ButtonHandler), ButtonHandler.Mapper),
				(nameof(PageHandler), PageHandler.Mapper),
				(nameof(ContentViewHandler), ContentViewHandler.Mapper),
				(nameof(LabelHandler), LabelHandler.Mapper),
				(nameof(EntryHandler), EntryHandler.Mapper),
				(nameof(LayoutHandler), LayoutHandler.Mapper),
				(nameof(ImageHandler), ImageHandler.Mapper),
			};

			foreach (var (name, mapper) in mappers)
			{
				var keys = mapper.GetKeys().Distinct().ToList();

				var backgroundIndex = keys.IndexOf(nameof(IView.Background));
				var semanticsIndex = keys.IndexOf(nameof(IView.Semantics));

				Assert.True(backgroundIndex >= 0, $"{name} is missing the Background key.");
				Assert.True(semanticsIndex >= 0, $"{name} is missing the Semantics key.");

				foreach (var redirectKey in new[] { nameof(VisualElement.BackgroundColor), nameof(Page.BackgroundImageSource) })
				{
					var redirectIndex = keys.IndexOf(redirectKey);
					Assert.True(redirectIndex >= 0, $"{name} is missing the {redirectKey} key.");
					Assert.True(backgroundIndex < redirectIndex, $"{name} maps {redirectKey} before Background.");
				}

				foreach (var redirectKey in new[]
				{
					SemanticProperties.DescriptionProperty.PropertyName,
					SemanticProperties.HintProperty.PropertyName,
					SemanticProperties.HeadingLevelProperty.PropertyName,
				})
				{
					var redirectIndex = keys.IndexOf(redirectKey);
					Assert.True(redirectIndex >= 0, $"{name} is missing the {redirectKey} key.");
					Assert.True(semanticsIndex < redirectIndex, $"{name} maps {redirectKey} before Semantics.");
				}
			}
		}

		[Fact]
		public void RealDerivedButtonHandler_OwningBackgroundMapping_MapsBackgroundExactlyOnceOnConnect()
		{
			int backgroundCalls = 0;
			Paint capturedBackground = null;

			var mapper = new PropertyMapper<IButton, IButtonHandler>(ButtonHandler.Mapper)
			{
				[nameof(IButton.Background)] = (h, v) =>
				{
					backgroundCalls++;
					capturedBackground = v.Background;
				},
			};

			var handler = new DerivedButtonHandlerStub(mapper);
			var button = new Button { BackgroundColor = Colors.Red };

			handler.SetVirtualView(button);

			Assert.Equal(1, backgroundCalls);
			var paint = Assert.IsType<SolidPaint>(capturedBackground);
			Assert.Equal(Colors.Red, paint.Color);
		}

		[Fact]
		public void RealDerivedButtonHandler_OwningBackgroundMapping_MapsSemanticsExactlyOnceOnConnect()
		{
			int semanticsCalls = 0;
			Semantics capturedSemantics = null;

			var mapper = new PropertyMapper<IButton, IButtonHandler>(ButtonHandler.Mapper)
			{
				[nameof(IButton.Background)] = (h, v) => { },
				[nameof(IView.Semantics)] = (h, v) =>
				{
					semanticsCalls++;
					capturedSemantics = v.Semantics;
				},
			};

			var handler = new DerivedButtonHandlerStub(mapper);
			var button = new Button();
			SemanticProperties.SetDescription(button, "Submit order");

			handler.SetVirtualView(button);

			Assert.Equal(1, semanticsCalls);
			Assert.Equal("Submit order", capturedSemantics?.Description);
		}

		[Fact]
		public void RealDerivedPageHandler_OwningBackgroundMapping_MapsBackgroundExactlyOnceOnConnect()
		{
			int backgroundCalls = 0;
			Paint capturedBackground = null;

			// Mirrors PageHandler on iOS/Tizen, where the page handler's own mapper owns Background.
			var mapper = new PropertyMapper<IContentView, IPageHandler>(PageHandler.Mapper)
			{
				[nameof(IContentView.Background)] = (h, v) =>
				{
					backgroundCalls++;
					capturedBackground = v.Background;
				},
			};

			var handler = new DerivedPageHandlerStub(mapper);

			// ContentPage.RemapForControls registers a HideSoftInputOnTapped mapping on PageHandler.Mapper
			// that resolves a service off the handler, so a real page handler needs a MauiContext.
			handler.SetMauiContext(new MauiContext(MauiApp.CreateBuilder().Build().Services));

			var page = new ContentPage { BackgroundImageSource = ImageSource.FromFile("some_image.png") };

			handler.SetVirtualView(page);

			Assert.Equal(1, backgroundCalls);
			Assert.IsType<ImageSourcePaint>(capturedBackground);
		}

		[Fact]
		public void RealDerivedButtonHandler_Reconnect_MapsBackgroundExactlyOncePerPass()
		{
			int backgroundCalls = 0;

			var mapper = new PropertyMapper<IButton, IButtonHandler>(ButtonHandler.Mapper)
			{
				[nameof(IButton.Background)] = (h, v) => backgroundCalls++,
			};

			var handler = new DerivedButtonHandlerStub(mapper);
			handler.SetVirtualView(new Button { BackgroundColor = Colors.Red });
			Assert.Equal(1, backgroundCalls);

			// Handler reuse (CollectionView/CarouselView recycling, Shell tab switching): the same handler
			// instance, whose platform view already exists, is bound to a new virtual view.
			backgroundCalls = 0;
			handler.SetVirtualView(new Button { BackgroundColor = Colors.Green });
			Assert.Equal(1, backgroundCalls);
		}

		[Fact]
		public void RealDerivedButtonHandler_DynamicBackgroundColorChangeAfterConnect_StillApplies()
		{
			int backgroundCalls = 0;

			var mapper = new PropertyMapper<IButton, IButtonHandler>(ButtonHandler.Mapper)
			{
				[nameof(IButton.Background)] = (h, v) => backgroundCalls++,
			};

			var handler = new DerivedButtonHandlerStub(mapper);
			var button = new Button { BackgroundColor = Colors.Red };
			handler.SetVirtualView(button);

			// The handler is fully connected now, so a dynamic change - and an explicit same-value update
			// after it - must both still reach the derived handler's own Background mapping.
			backgroundCalls = 0;
			button.BackgroundColor = Colors.Purple;
			Assert.Equal(1, backgroundCalls);

			handler.UpdateValue(nameof(VisualElement.BackgroundColor));
			Assert.Equal(2, backgroundCalls);
		}

		// A mapper that runs a *nested* bulk pass on the same handler (mid-pass) must not destroy the outer
		// pass's scope: once the nested pass ends, the outer pass's remaining redirect turns are still
		// provably redundant and must still be skipped.
		[Fact]
		public void NestedSameHandlerPass_RestoresOuterPassScope_AndOuterRedirectsStayDeduplicated()
		{
			int outerBackgroundCalls = 0;
			int nestedBackgroundCalls = 0;

			var nestedCommandMapper = new CommandMapper<IView, IViewHandler>();
			var nestedMapper = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (h, v) => nestedBackgroundCalls++,
			};
			VisualElement.RemapForControls(nestedMapper, nestedCommandMapper);

			// "__NestedPass" is inserted (via the object initializer, before RemapForControls runs) between
			// Background and the redirect keys RemapForControls appends afterwards, so the nested pass runs
			// *inside* the outer one - after Background has had its turn, but before BackgroundColor gets
			// its own.
			var commandMapper = new CommandMapper<IView, IViewHandler>();
			var mapper = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (h, v) => outerBackgroundCalls++,
				["__NestedPass"] = (h, v) => nestedMapper.UpdateProperties(h, v),
			};
			VisualElement.RemapForControls(mapper, commandMapper);

			var handlerStub = new HandlerStub(mapper, commandMapper);
			handlerStub.SetVirtualView(new Button { BackgroundColor = Colors.Red });

			// The nested pass deduplicates within itself, and the outer pass's own BackgroundColor/
			// BackgroundImageSource turns are still skipped after it returns.
			Assert.Equal(1, nestedBackgroundCalls);
			Assert.Equal(1, outerBackgroundCalls);
		}

		// While a nested bulk pass for a *different* handler is on top, a redirect update targeting the
		// outer handler is not that handler's own pass turn and must therefore still be applied.
		[Fact]
		public void NestedPassForAnotherHandler_DoesNotSuppressOuterHandlersRedirectUpdate()
		{
			int outerBackgroundCalls = 0;

			var otherCommandMapper = new CommandMapper<IView, IViewHandler>();
			var otherMapper = new PropertyMapper<IView, IViewHandler>();
			var otherHandler = new HandlerStub(otherMapper, otherCommandMapper);
			otherHandler.SetVirtualView(new Button());

			var commandMapper = new CommandMapper<IView, IViewHandler>();
			var mapper = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (h, v) => outerBackgroundCalls++,
			};
			VisualElement.RemapForControls(mapper, commandMapper);

			// Runs after every key RemapForControls appends, so the outer pass has already skipped its own
			// BackgroundColor turn by the time this runs.
			mapper.Add("__NestedPassForAnotherHandler", (h, v) =>
			{
				otherMapper.UpdateProperties(otherHandler, otherHandler.VirtualView);

				// An explicit redirect update for *our* handler, issued from a step that is not that
				// redirect key's own turn, must be honored - the nested pass must not have left anything
				// behind that makes it look skippable.
				h.UpdateValue(nameof(VisualElement.BackgroundColor));
			});

			var handlerStub = new HandlerStub(mapper, commandMapper);
			handlerStub.SetVirtualView(new Button { BackgroundColor = Colors.Red });

			// Once for the canonical Background key, once for the explicit redirect update above.
			Assert.Equal(2, outerBackgroundCalls);
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

		class DerivedButtonHandlerStub : ButtonHandler
		{
			public DerivedButtonHandlerStub(IPropertyMapper mapper) : base(mapper)
			{
			}

			protected override object CreatePlatformView() => new object();
		}

		class DerivedPageHandlerStub : PageHandler
		{
			public DerivedPageHandlerStub(IPropertyMapper mapper) : base(mapper)
			{
			}

			protected override object CreatePlatformView() => new object();
		}
	}
}

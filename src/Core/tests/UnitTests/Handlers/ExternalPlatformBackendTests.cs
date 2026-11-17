using Microsoft.Maui.Controls;
using Microsoft.Maui.ExternalBackend;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.UnitTests.Handlers
{
	/// <summary>
	/// Verifies that a platform backend living in an assembly outside of dotnet/maui — with native view
	/// types .NET MAUI knows nothing about — can implement, be recognized by, and be driven through the
	/// public handler contracts.
	/// </summary>
	/// <remarks>
	/// The fake backend lives in Microsoft.Maui.Core.ExternalBackend.TestSupport, which is deliberately
	/// absent from Core's InternalsVisibleTo list.
	/// </remarks>
	[Category(TestCategory.Core)]
	public class ExternalPlatformBackendTests
	{
		static IMauiContext CreateContext()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Label, FakeLabelHandler>();
					handlers.AddHandler<VerticalStackLayout, FakeLayoutHandler>();
					handlers.AddHandler<ContentView, FakeContentViewHandler>();
					handlers.AddHandler<Window, FakeWindowHandler>();
				})
				.Build();

			return new MauiContext(mauiApp.Services);
		}

		[Fact]
		public void ExternalViewHandlerSatisfiesTypedNeutralContract()
		{
			var handler = new FakeLabelHandler();

			IViewHandler<ILabel, FakeNativeLabel> typed = handler;

			Assert.Same(handler, typed);
		}

		[Fact]
		public void ExternalViewHandlerSatisfiesPlatformNeutralContract()
		{
			var handler = new FakeLabelHandler();

			Assert.IsAssignableFrom<IViewHandler<ILabel, object>>(handler);
			Assert.IsAssignableFrom<IElementHandler<ILabel, object>>(handler);
		}

		[Fact]
		public void ExternalElementHandlerSatisfiesPlatformNeutralContract()
		{
			var handler = new FakeWindowHandler();

			Assert.IsAssignableFrom<IElementHandler<IWindow, FakeNativeWindow>>(handler);
			Assert.IsAssignableFrom<IElementHandler<IWindow, object>>(handler);
		}

		[Fact]
		public void ExternalLayoutHandlerSatisfiesPlatformNeutralLayoutContract()
		{
			var handler = new FakeLayoutHandler();

			Assert.IsAssignableFrom<ILayoutHandler<FakeNativeLayoutView>>(handler);
			Assert.IsAssignableFrom<ILayoutHandler<object>>(handler);
		}

		[Fact]
		public void ExternalHandlerDoesNotImplementAliasedInterface()
		{
			// Runtime assertion only: these handlers do not implement the aliased interfaces. The
			// compile-time half of the claim - that they *cannot* - is proven by the Core.ExternalBackend
			// assembly, which compiles the same handler shapes against every target framework Core ships.
			// Attempting to implement ILabelHandler/ILayoutHandler there fails with CS0738 on a platform
			// target framework and CS9333 on the platform-neutral and netstandard ones.
			Assert.IsNotAssignableFrom<ILabelHandler>(new FakeLabelHandler());
			Assert.IsNotAssignableFrom<ILayoutHandler>(new FakeLayoutHandler());
		}

		[Fact]
		public void InBoxLabelHandlerSatisfiesPlatformNeutralContract()
		{
			var handler = new LabelHandler();

			Assert.IsAssignableFrom<IViewHandler<ILabel, object>>(handler);
			Assert.IsAssignableFrom<IElementHandler<ILabel, object>>(handler);
		}

		[Fact]
		public void InBoxContentViewHandlerSatisfiesPlatformNeutralContract()
		{
			Assert.IsAssignableFrom<IViewHandler<IContentView, object>>(new ContentViewHandler());
		}

		[Fact]
		public void InBoxWindowHandlerSatisfiesPlatformNeutralContract()
		{
			Assert.IsAssignableFrom<IElementHandler<IWindow, object>>(new WindowHandler());
		}

		[Fact]
		public void InBoxLayoutHandlerSatisfiesBothLayoutContracts()
		{
			var handler = new LayoutHandler();

			// Additive: the pre-existing interface keeps working...
			Assert.IsAssignableFrom<ILayoutHandler>(handler);

			// ...alongside the platform-neutral one.
			Assert.IsAssignableFrom<ILayoutHandler<object>>(handler);
		}

		[Fact]
		public void AliasedLayoutHandlerContractIsUnchanged()
		{
			// Backward compatibility: nothing moved off ILayoutHandler.
			var members = new[]
			{
				nameof(ILayoutHandler.Add),
				nameof(ILayoutHandler.Remove),
				nameof(ILayoutHandler.Clear),
				nameof(ILayoutHandler.Insert),
				nameof(ILayoutHandler.Update),
				nameof(ILayoutHandler.UpdateZIndex),
			};

			foreach (var name in members)
			{
				var method = typeof(ILayoutHandler).GetMethod(name);

				Assert.NotNull(method);
				Assert.Equal(typeof(ILayoutHandler), method.DeclaringType);
			}

			Assert.NotNull(typeof(ILayoutHandler).GetProperty(nameof(ILayoutHandler.PlatformView)));
			Assert.NotNull(typeof(ILayoutHandler).GetProperty(nameof(ILayoutHandler.VirtualView)));
		}

		[Fact]
		public void PlatformNeutralContractsAreCovariant()
		{
			ILayoutHandler<FakeNativeLayoutView> typed = new FakeLayoutHandler();

			ILayoutHandler<object> neutral = typed;

			Assert.Same(typed, neutral);
		}

		[Fact]
		public void ExternalHandlerIsResolvedThroughHandlerRegistration()
		{
			var context = CreateContext();

			var handler = new Label().ToHandler(context);

			Assert.IsType<FakeLabelHandler>(handler);
		}

		[Fact]
		public void ExternalHandlerPropertyMapperUpdatesNativeView()
		{
			var context = CreateContext();
			var label = new Label { Text = "hello" };

			var handler = (FakeLabelHandler)label.ToHandler(context);

			Assert.Equal("hello", handler.PlatformView.FakeText);

			label.Text = "goodbye";
			handler.UpdateValue(nameof(ILabel.Text));

			Assert.Equal("goodbye", handler.PlatformView.FakeText);
		}

		[Fact]
		public void ExternalHandlerInheritsViewMapperFromCore()
		{
			var context = CreateContext();
			var label = new Label { Text = "hello", Opacity = 0.5 };

			var handler = (FakeLabelHandler)label.ToHandler(context);

			// Opacity is chained off Core's ViewMapper, proving mapper composition still works.
			Assert.Equal(0.5, handler.PlatformView.FakeOpacity);
		}

		[Fact]
		public void ExternalHandlerIsReachableThroughPlatformNeutralContract()
		{
			var context = CreateContext();
			var label = new Label { Text = "hello" };

			label.ToHandler(context);

			var neutral = Assert.IsAssignableFrom<IViewHandler<ILabel, object>>(label.Handler);

			Assert.Same(label, neutral.VirtualView);
			Assert.IsType<FakeNativeLabel>(neutral.PlatformView);
		}

		[Fact]
		public void ControlsLayoutCommandsReachExternalLayoutHandler()
		{
			var context = CreateContext();
			var layout = new VerticalStackLayout();

			var handler = (FakeLayoutHandler)layout.ToHandler(context);

			layout.Add(new Label { Text = "first" });
			layout.Add(new Label { Text = "second" });
			Assert.Equal(2, handler.PlatformView.FakeChildren.Count);

			layout.Insert(0, new Label { Text = "inserted" });
			Assert.Equal(3, handler.PlatformView.FakeChildren.Count);
			Assert.Equal("inserted", Assert.IsType<FakeNativeLabel>(handler.PlatformView.FakeChildren[0]).FakeText);

			layout.RemoveAt(0);
			Assert.Equal(2, handler.PlatformView.FakeChildren.Count);

			layout[0] = new Label { Text = "replaced" };
			Assert.Equal("replaced", Assert.IsType<FakeNativeLabel>(handler.PlatformView.FakeChildren[0]).FakeText);

			layout.Clear();
			Assert.Empty(handler.PlatformView.FakeChildren);
		}

		[Fact]
		public void ExternalLayoutHandlerIsDrivenThroughNeutralContract()
		{
			var context = CreateContext();
			var layout = new VerticalStackLayout();

			layout.ToHandler(context);

			var neutral = Assert.IsAssignableFrom<ILayoutHandler<object>>(layout.Handler);

			var child = new Label { Text = "one" };
			child.ToHandler(context);
			neutral.Add(child);

			var platformView = Assert.IsType<FakeNativeLayoutView>(neutral.PlatformView);
			Assert.Single(platformView.FakeChildren);

			neutral.Clear();
			Assert.Empty(platformView.FakeChildren);
		}

		[Fact]
		public void ExternalWindowHandlerRunsThroughElementMapper()
		{
			var context = CreateContext();
			var window = new Window { Title = "external" };

			var handler = (FakeWindowHandler)((IElement)window).ToHandler(context);

			Assert.Equal("external", handler.PlatformView.FakeTitle);

			IElementHandler<IWindow, object> neutral = handler;

			Assert.Same(window, neutral.VirtualView);
			Assert.IsType<FakeNativeWindow>(neutral.PlatformView);
		}
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.VisualElement)]
	public partial class VisualElementTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task CanCreateHandler()
		{
			var image = new Image();
			await CreateHandlerAsync<ImageHandler>(image);
		}

		[Fact]
		public async Task SettingHandlerDoesNotThrow()
		{
			var image = new Image();
			var handler = await CreateHandlerAsync<ImageHandler>(image);
			image.Handler = handler;
		}

		[Fact]
		[Category(TestCategory.Element)]
		public async Task ConcurrentCustomBindablePropertyWritesComplete()
		{
			const int iterations = 20000;
			var target = new RaceLabel { Alpha = 1, Beta = 1 };
			using var overlap = new Barrier(2);
			int alphaEntered = 0;
			int betaEntered = 0;

			target.PropertyChanged += (_, args) =>
			{
				if ((args.PropertyName == nameof(RaceLabel.Alpha) && Interlocked.Exchange(ref alphaEntered, 1) == 0) ||
					(args.PropertyName == nameof(RaceLabel.Beta) && Interlocked.Exchange(ref betaEntered, 1) == 0))
				{
					Assert.True(overlap.SignalAndWait(TimeSpan.FromSeconds(5)), "Both property writes should overlap.");
				}
			};

			var alpha = Task.Factory.StartNew(() =>
			{
				for (int value = 2; value <= iterations; value++)
					target.Alpha = value;
			}, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

			var beta = Task.Factory.StartNew(() =>
			{
				for (int value = 2; value <= iterations; value++)
					target.Beta = value;
			}, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

			await Task.WhenAll(alpha, beta);
			Assert.Equal(iterations, target.Alpha);
			Assert.Equal(iterations, target.Beta);
		}

		[Fact]
		[Category(TestCategory.Element)]
		public async Task BackgroundVisualStateChangesRunOnUIThreadDuringAnimation()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<Label, LabelHandler>());
			});

			var target = new Label { Text = "Visual state and animation target" };
			var group = new VisualStateGroup { Name = "RaceStates" };
			group.States.Add(CreateState("Off", Colors.Blue));
			group.States.Add(CreateState("On", Colors.HotPink));
			VisualStateManager.SetVisualStateGroups(target, new VisualStateGroupList { group });

			await CreateHandlerAndAddToWindow(target, async () =>
			{
				Assert.NotNull(target.Handler);
				var firstAnimationTick = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				TaskCompletionSource stateApplied = null;
				Color expectedColor = null;
				int opacityChanges = 0;
				int offThreadColorChanges = 0;

				target.PropertyChanged += OnPropertyChanged;
				using var cancellation = new CancellationTokenSource();
				var animation = FadeLoopAsync(target, cancellation.Token);

				try
				{
					await firstAnimationTick.Task.WaitAsync(TimeSpan.FromSeconds(5));
					int initialOpacityChanges = opacityChanges;

					for (int index = 0; index < 20; index++)
					{
						string state = index % 2 == 0 ? "On" : "Off";
						expectedColor = index % 2 == 0 ? Colors.HotPink : Colors.Blue;
						stateApplied = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

						bool queued = await Task.Run(() =>
						{
							Assert.False(MainThread.IsMainThread);
							return VisualStateManager.GoToState(target, state);
						});

						Assert.True(queued);
						await stateApplied.Task.WaitAsync(TimeSpan.FromSeconds(5));
						Assert.Equal(expectedColor, target.TextColor);
						await Task.Delay(25);
					}

					Assert.True(opacityChanges > initialOpacityChanges, "The animation must advance during state transitions.");
					Assert.Equal(0, Volatile.Read(ref offThreadColorChanges));
				}
				finally
				{
					cancellation.Cancel();
					await animation;
					target.PropertyChanged -= OnPropertyChanged;
				}

				void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
				{
					if (args.PropertyName == nameof(VisualElement.Opacity))
					{
						opacityChanges++;
						firstAnimationTick.TrySetResult();
					}
					else if (args.PropertyName == nameof(Label.TextColor))
					{
						if (!MainThread.IsMainThread)
							Interlocked.Increment(ref offThreadColorChanges);
						if (Equals(target.TextColor, expectedColor))
							stateApplied?.TrySetResult();
					}
				}
			});
		}

		static VisualState CreateState(string name, Color color)
		{
			var state = new VisualState { Name = name };
			state.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = color });
			return state;
		}

		static async Task FadeLoopAsync(VisualElement target, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await target.FadeToAsync(0.2, 100, null, cancellationToken);
				if (!cancellationToken.IsCancellationRequested)
					await target.FadeToAsync(1, 100, null, cancellationToken);
			}
		}

		class RaceLabel : Label
		{
			public static readonly BindableProperty AlphaProperty =
				BindableProperty.Create(nameof(Alpha), typeof(int), typeof(RaceLabel), 0);

			public static readonly BindableProperty BetaProperty =
				BindableProperty.Create(nameof(Beta), typeof(int), typeof(RaceLabel), 0);

			public int Alpha
			{
				get => (int)GetValue(AlphaProperty);
				set => SetValue(AlphaProperty, value);
			}

			public int Beta
			{
				get => (int)GetValue(BetaProperty);
				set => SetValue(BetaProperty, value);
			}
		}

#if ANDROID || IOS || MACCATALYST
		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
		[Category(TestCategory.Lifecycle)]
#if IOS || MACCATALYST
		[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationRenderer)] // See RendererHandlerVariant.cs
#endif
		public class NewWindowCollection : ControlsHandlerTestBase
		{
			protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder builder)
			{

				return
					base
						.ConfigureBuilder(builder)
						.ConfigureMauiHandlers(handlers =>
						{
							RegisterNavigationPageHandler(handlers);
#if WINDOWS || ANDROID
							handlers.AddHandler<Toolbar, ToolbarHandler>();
#endif
						});
			}

			// Extracted so an iOS/MacCatalyst-only subclass can swap in NavigationRenderer,
			// letting every NewWindowCollection test run against both the NavigationPage
			// renderer and handler. See VisualElementNavigationHandlerTests.iOS.cs and
			// RendererHandlerVariant.cs.
			protected virtual void RegisterNavigationPageHandler(IMauiHandlersCollection handlers)
			{
#if IOS || MACCATALYST
				handlers.AddHandler<NavigationPage, NavigationRenderer>();
#else
				handlers.AddHandler<NavigationPage, NavigationViewHandler>();
#endif
			}

			[Fact]
			public async Task LoadedAndUnloadedFire()
			{
				var editor = new Editor();

				int unloaded = 0;
				int loaded = 0;
				editor.Loaded += (_, __) => loaded++;
				editor.Unloaded += (_, __) => unloaded++;

				await CreateHandlerAndAddToWindow<EditorHandler>(editor, (handler) =>
				{
					Assert.Equal(1, loaded);
					Assert.Equal(0, unloaded);
					return Task.CompletedTask;
				});

				Assert.Equal(1, loaded);
				Assert.Equal(1, unloaded);
			}

			[Fact]
			public async Task LoadedAndUnloadedFireWhenParentRemoved()
			{
				var editor = new Editor();
				var layout = new VerticalStackLayout()
				{
					editor
				};

				var parentLayout = new VerticalStackLayout()
				{
					layout
				};

				int unloaded = 0;
				int loaded = 0;
				editor.Loaded += (_, __) => loaded++;
				editor.Unloaded += (_, __) => unloaded++;

				await CreateHandlerAndAddToWindow<LayoutHandler>(parentLayout, async (handler) =>
				{
					parentLayout.Remove(layout);
					await OnUnloadedAsync(layout);
					await OnUnloadedAsync(editor);

					Assert.Equal(1, loaded);
					Assert.Equal(1, unloaded);

					parentLayout.Add(layout);
					await OnLoadedAsync(layout);
					await OnLoadedAsync(editor);

					Assert.Equal(2, loaded);
					Assert.Equal(1, unloaded);
				});

				await AssertEventually(() => loaded == 2 && unloaded == 2);
			}

			[Fact]
			public async Task NavigatedToFiresAfterLoaded()
			{
				var navPage = new NavigationPage(new ContentPage());
				var page = new ContentPage();

				int loaded = 0;
				bool loadedFired = false;

				page.Loaded += (_, __) => loaded++;
				page.NavigatedTo += (_, __) => loadedFired = (loaded == 1);

				await CreateHandlerAndAddToWindow<IElementHandler>(navPage, async (handler) =>
				{
					await navPage.PushAsync(page);
					Assert.True(loadedFired);
				});
			}

			[Fact]
			public async Task LoadedFiresOnPushedPage()
			{
				var navPage = new NavigationPage(new ContentPage());
				var page = new ContentPage();

				int unloaded = 0;
				int loaded = 0;
				page.Loaded += (_, __) => loaded++;
				page.Unloaded += (_, __) => unloaded++;

				await CreateHandlerAndAddToWindow<IElementHandler>(navPage, async (handler) =>
				{
					Assert.Equal(0, loaded);
					Assert.Equal(0, unloaded);

					await navPage.PushAsync(page);

					Assert.Equal(1, loaded);
					Assert.Equal(0, unloaded);

					await navPage.PopAsync();

					await AssertEventually(() => loaded == 1 && unloaded == 1);
				});
			}
		}
	}
}
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Xunit;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WSize = Windows.Foundation.Size;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FontImageSourceServiceTests
	{
		[Fact]
		public Task SystemFontGlyphRenders() =>
			InvokeOnMainThreadAsync(async () =>
			{
				var manager = new FontManager(new FontRegistrar(fontLoader: null));
				var service = new FontImageSourceService(manager);
				var source = new FontImageSourceStub
				{
					Glyph = "\uE711",
					Font = Font.OfSize("Segoe MDL2 Assets", 48),
					Color = Colors.Red
				};

				using var result = await service.GetImageSourceAsync(source);
				var canvas = Assert.IsType<CanvasImageSource>(result.Value);
				var image = new Image { Source = canvas, Width = 60, Height = 60 };
				using var services = new ServiceCollection().BuildServiceProvider();
				using var bitmap = await image.AssertContainsColor(Colors.Red, new ContextStub(services));
			});

		[Theory]
		[InlineData("Assets/Fonts/insubfolder.ttf#Dokdo")]
		[InlineData("ms-appx:///Assets/Fonts/insubfolder.ttf#Dokdo")]
		public Task FontFileSourceResolvesForCurrentPackaging(string fontSource) =>
			InvokeOnMainThreadAsync(() =>
			{
				var service = new FontImageSourceService(new FixedFontManager(fontSource));
				var source = new FontImageSourceStub { Font = Font.OfSize("Dokdo", 12) };

#if UNPACKAGED
				var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "insubfolder.ttf");
				Assert.True(File.Exists(path), $"File not found: {path}");
				var expected = "file:///" + path.Replace('\\', '/') + "#Dokdo";
#else
				var expected = fontSource;
#endif
				Assert.Equal(expected, service.GetFontSource(source));
			});

		[Theory]
		[InlineData("Assets/Fonts/missing-font.ttf#Missing")]
		[InlineData("ms-appx:///Assets/Fonts/missing-font.ttf#Missing")]
		[InlineData("Segoe MDL2 Assets")]
		public Task UnresolvedFontSourceIsPreserved(string fontSource) =>
			InvokeOnMainThreadAsync(() =>
			{
				var service = new FontImageSourceService(new FixedFontManager(fontSource));
				var source = new FontImageSourceStub { Font = Font.OfSize("Missing", 12) };

				Assert.Equal(fontSource, service.GetFontSource(source));
			});

		sealed class FixedFontManager : IFontManager
		{
			readonly FontFamily _family;

			public FixedFontManager(string source) => _family = new FontFamily(source);

			public FontFamily DefaultFontFamily => _family;
			public double DefaultFontSize => 12;
			public FontFamily GetFontFamily(Font font) => _family;
			public double GetFontSize(Font font, double defaultFontSize = 0) => font.Size;
		}

		[Theory]
		[InlineData("Ionicons", "\uf30c", 24, "#FF0000")]
		[InlineData("Dokdo", "X", 24, "#00FF00")]
		[InlineData("Dokdo", "g", 48, "#0000FF")]
		[InlineData("Dokdo", "A\u030A", 30, "#FF0000")]
		public async Task GetImageSourceAsyncRendersGlyph(string fontFamily, string glyph, double size, string colorHex)
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
					fonts.AddFont("ionicons.ttf", "Ionicons");
				})
				.Build();
			var service = new FontImageSourceService(app.Services.GetRequiredService<IFontManager>());
			var color = Color.FromArgb(colorHex);
			var source = new FontImageSourceStub
			{
				Font = Font.OfSize(fontFamily, size),
				Glyph = glyph,
				Color = color
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var pending = service.GetImageSourceAsync(source);
				Assert.False(pending.IsCompleted, "Font rendering must be deferred until after the current layout tick.");
				using var result = await pending;
				var image = Assert.IsType<CanvasImageSource>(result.Value);
				Assert.True(image.Size.Width > 2);
				Assert.True(image.Size.Height > 2);
				Assert.False(result.IsResolutionDependent);

				var platformImage = new WImage { Source = image, Width = 100, Height = 100 };
				using var bitmap = await platformImage.AssertContainsColor(color, new ContextStub(app.Services));
			});
		}

		[Fact]
		public async Task CanceledQueuedRequestDoesNotResolveFont()
		{
			var service = new FontImageSourceService(null);
			using var cancellation = new CancellationTokenSource();

			await InvokeOnMainThreadAsync(async () =>
			{
				var pending = service.GetImageSourceAsync(new FontImageSourceStub
				{
					Font = Font.OfSize("Dokdo", 24),
					Glyph = "X"
				}, cancellationToken: cancellation.Token);

				Assert.False(pending.IsCompleted);
				cancellation.Cancel();
				await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
			});
		}

		[Fact]
		public async Task EmptySourceReturnsNull()
		{
			var service = new FontImageSourceService(null);
			Assert.Null(await service.GetImageSourceAsync(new FontImageSourceStub { IsEmpty = true }));
		}

		[Fact]
		public async Task CanceledRequestDoesNotRender()
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddFont("dokdo_regular.ttf", "Dokdo"))
				.Build();
			var service = new FontImageSourceService(app.Services.GetRequiredService<IFontManager>());
			using var cancellation = new CancellationTokenSource();
			cancellation.Cancel();

			await InvokeOnMainThreadAsync(() => Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
				service.GetImageSourceAsync(new FontImageSourceStub
				{
					Font = Font.OfSize("Dokdo", 24),
					Glyph = "X"
				}, cancellationToken: cancellation.Token)));
		}

		// https://github.com/dotnet/maui/issues/39888
		[Fact]
		public async Task CustomFontDuringMeasureWithBitmapDecodingDoesNotCrash()
		{
			using var app = MauiApp.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddFont("dokdo_regular.ttf", "Dokdo"))
				.Build();
			var service = new FontImageSourceService(app.Services.GetRequiredService<IFontManager>());

			await InvokeOnMainThreadAsync(async () =>
			{
				var panel = new FontImagePanel(service) { Width = 400, Height = 200 };
				await panel.AttachAndRun(async () =>
				{
					void OnRendering(object sender, object args) => panel.InvalidateMeasure();

					CompositionTarget.Rendering += OnRendering;
					try
					{
						await Task.Delay(TimeSpan.FromSeconds(30));
						Assert.True(panel.GlyphsRendered > 100, $"Only {panel.GlyphsRendered} glyphs were rendered.");
						Assert.True(panel.BitmapImagesDecoded > 0, "No bitmap images were decoded.");
						Assert.Null(panel.BitmapError);
					}
					finally
					{
						CompositionTarget.Rendering -= OnRendering;
						await panel.FinishAsync();
					}
				}, new ContextStub(app.Services));
			});
		}

		sealed class FontImagePanel : Panel
		{
			readonly FontImageSourceService _service;
			readonly Task<IImageSourceServiceResult<ImageSource>>[] _pending = new Task<IImageSourceServiceResult<ImageSource>>[40];
			readonly Uri _bitmapUri;
			int _generation;
			bool _finished;

			public FontImagePanel(FontImageSourceService service)
			{
				_service = service;
				var bitmapPath = Path.Combine(AppContext.BaseDirectory, "dotnet_bot.scale-100.png");
				Assert.True(File.Exists(bitmapPath), $"Bitmap not found: {bitmapPath}");
				_bitmapUri = new Uri(bitmapPath);
				for (int i = 0; i < _pending.Length; i++)
					Children.Add(new WImage());
			}

			public int GlyphsRendered { get; private set; }
			public int BitmapImagesDecoded { get; private set; }
			public string BitmapError { get; private set; }

			protected override WSize MeasureOverride(WSize availableSize)
			{
				if (_finished)
					return new WSize(400, 200);

				int generation = ++_generation;
				for (int i = 0; i < Children.Count; i++)
				{
					var image = (WImage)Children[i];
					if (i % 4 == 0)
					{
						// A new decode size queues bitmap work during XAML's layout tick.
						var bitmap = new BitmapImage(_bitmapUri)
						{
							DecodePixelWidth = 8 + (generation * 7 + i) % 64
						};
						bitmap.ImageOpened += (sender, args) => BitmapImagesDecoded++;
						bitmap.ImageFailed += (sender, args) => BitmapError = args.ErrorMessage;
						image.Source = bitmap;
					}
					else if (_pending[i] == null || _pending[i].IsCompleted)
					{
						ApplyResult(i);
						_pending[i] = _service.GetImageSourceAsync(new FontImageSourceStub
						{
							Font = Font.OfSize("Dokdo", 24),
							Glyph = ((char)('A' + (generation + i) % 26)).ToString(),
							Color = Colors.DarkSlateBlue
						});
					}

					image.Measure(new WSize(32, 32));
				}
				return new WSize(400, 200);
			}

			protected override WSize ArrangeOverride(WSize finalSize)
			{
				for (int i = 0; i < Children.Count; i++)
					Children[i].Arrange(new global::Windows.Foundation.Rect(i % 10 * 34, i / 10 * 34, 32, 32));
				return finalSize;
			}

			public async Task FinishAsync()
			{
				_finished = true;
				for (int i = 0; i < _pending.Length; i++)
				{
					if (_pending[i] != null)
					{
						await _pending[i];
						ApplyResult(i);
					}
				}
			}

			void ApplyResult(int index)
			{
				if (_pending[index] == null)
					return;

				using var result = _pending[index].GetAwaiter().GetResult();
				((WImage)Children[index]).Source = Assert.IsType<CanvasImageSource>(result.Value);
				_pending[index] = null;
				GlyphsRendered++;
			}
		}
	}
}

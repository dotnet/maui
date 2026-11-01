using System;
using System.IO;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Fonts;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;
using Xunit;
using GtkLabel = global::Gtk.Label;

namespace Microsoft.Maui.Gtk.UnitTests
{
	public class InteropTests
	{
		[Fact]
		public void ColorsConvertToGdkAndCss()
		{
			var rgba = new Color(1f, 0.5f, 0f, 0.25f).ToGdkRgba();
			Assert.Equal(1f, rgba.Red);
			Assert.Equal(0.5f, rgba.Green);
			Assert.Equal(0f, rgba.Blue);
			Assert.Equal(0.25f, rgba.Alpha);

			Assert.Equal("rgba(255,128,0,0.25)", new Color(1f, 0.5f, 0f, 0.25f).ToCss());
			Assert.Equal(Colors.Red.Red, rgba.ToMauiColor().Red);
		}

		[Theory]
		[InlineData(double.PositiveInfinity, -1)]
		[InlineData(double.NaN, -1)]
		[InlineData(-5, -1)]
		[InlineData(0, 0)]
		[InlineData(10.2, 11)]
		[InlineData(10, 10)]
		public void ConstraintsConvertToForSize(double constraint, int expected) =>
			Assert.Equal(expected, GtkInterop.ToForSize(constraint));

		[Theory]
		[InlineData(double.NaN, 0)]
		[InlineData(-1, 0)]
		[InlineData(0, 0)]
		[InlineData(10.00001, 10)]
		[InlineData(10.4, 11)]
		public void LengthsRoundUpToWholePixels(double value, int expected) =>
			Assert.Equal(expected, GtkInterop.ToPixels(value));

		[Fact]
		public void KeyboardsMapToInputPurposes()
		{
			Assert.Equal(global::Gtk.InputPurpose.Number, GtkInputExtensions.ToInputPurpose(Keyboard.Numeric));
			Assert.Equal(global::Gtk.InputPurpose.Email, GtkInputExtensions.ToInputPurpose(Keyboard.Email));
			Assert.Equal(global::Gtk.InputPurpose.Phone, GtkInputExtensions.ToInputPurpose(Keyboard.Telephone));
			Assert.Equal(global::Gtk.InputPurpose.Url, GtkInputExtensions.ToInputPurpose(Keyboard.Url));
			Assert.Equal(global::Gtk.InputPurpose.FreeForm, GtkInputExtensions.ToInputPurpose(Keyboard.Default));
			Assert.Equal(global::Gtk.InputPurpose.FreeForm, GtkInputExtensions.ToInputPurpose(null));
		}

		[Theory]
		[InlineData(TextAlignment.Start, FlowDirection.LeftToRight, 0f)]
		[InlineData(TextAlignment.Center, FlowDirection.LeftToRight, 0.5f)]
		[InlineData(TextAlignment.End, FlowDirection.LeftToRight, 1f)]
		[InlineData(TextAlignment.Start, FlowDirection.RightToLeft, 1f)]
		[InlineData(TextAlignment.End, FlowDirection.RightToLeft, 0f)]
		public void TextAlignmentHonoursFlowDirection(TextAlignment alignment, FlowDirection flow, float expected) =>
			Assert.Equal(expected, GtkTextStyle.ToAlignment(alignment, flow));
	}

	public class StyleTests
	{
		[GtkFact]
		public void StyleBuildsOneRulePerSelectorAndClearsDeclarations() => GtkTestHost.Run(() =>
		{
			var widget = new GtkLabel("x");
			var style = GtkStyle.For(widget);
			Assert.Same(style, GtkStyle.For(widget));

			style.Set("color", "red");
			style.Set("font-size", "12px");
			style.Set("color", "blue", " placeholder");

			var css = style.ToCss();
			Assert.Contains($".{style.CssClass} {{ color: red; font-size: 12px; }}", css);
			Assert.Contains($".{style.CssClass} placeholder {{ color: blue; }}", css);
			Assert.True(widget.HasCssClass(style.CssClass));

			style.Set("color", null);
			Assert.DoesNotContain("color: red", style.ToCss());

			style.ClearPrefix("font-");
			style.Set("color", null, " placeholder");
			Assert.Equal(string.Empty, style.ToCss());
		});

		[GtkFact]
		public void EveryWidgetGetsItsOwnCssClass() => GtkTestHost.Run(() =>
		{
			var first = GtkStyle.For(new GtkLabel("a"));
			var second = GtkStyle.For(new GtkLabel("b"));
			Assert.NotEqual(first.CssClass, second.CssClass);
		});

		[GtkFact]
		public void StyleActuallyReachesTheRenderedWidget() => GtkTestHost.Run(() =>
		{
			var label = new GtkLabel("hello world wrapping text") { Wrap = true };
			var window = new global::Gtk.Window { Decorated = false, Child = label };
			window.SetDefaultSize(300, 300);
			window.Present();
			GtkTestHost.Pump(window);

			label.Measure(global::Gtk.Orientation.Horizontal, -1, out _, out int small, out _, out _);
			GtkStyle.For(label).Set("font-size", "40px");

			// The style is validated by the frame clock, so wait for it.
			int large = small;
			GtkTestHost.PumpUntil(() =>
			{
				label.Measure(global::Gtk.Orientation.Horizontal, -1, out _, out large, out _, out _);
				return large > small * 2;
			}, window, 3000);

			GtkTestHost.Retire(window);
			Assert.True(large > small * 2, $"a 40px font should be much wider ({small} -> {large})");
		});

		[GtkFact]
		public void PaintRendererDrawsSolidAndGradientPaints() => GtkTestHost.Run(() =>
		{
			Assert.True(GtkPaintRenderer.CanRender(new SolidPaint(Colors.Red)));
			Assert.False(GtkPaintRenderer.CanRender(null));
			Assert.False(GtkPaintRenderer.CanRender(new ImageSourcePaint()));

			var gradient = new LinearGradientPaint
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new[] { new PaintGradientStop(0, Colors.Red), new PaintGradientStop(1, Colors.Blue) },
			};
			Assert.True(GtkPaintRenderer.CanRender(gradient));

			var snapshot = new global::Gtk.Snapshot();
			var bounds = GtkInterop.ToGrapheneRect(0, 0, 100, 10);
			GtkPaintRenderer.Render(snapshot, gradient, bounds, 100, 10);
			Assert.NotNull(snapshot.ToNode());
		});
	}

	public class FontTests
	{
		[GtkFact]
		public void CssFontReflectsSizeWeightAndSlant() => GtkTestHost.Run(() =>
		{
			var manager = new GtkFontManager(null);
			var css = manager.GetCssFont(Font.OfSize("Some Family", 22, FontWeight.Bold, FontSlant.Italic));

			Assert.Contains("font-family: \"Some Family\"", css);
			Assert.Contains("font-size: 22px", css);
			Assert.Contains("font-weight: 700", css);
			Assert.Contains("font-style: italic", css);
		});

		[GtkFact]
		public void DefaultFontSizeComesFromTheGtkSettings() => GtkTestHost.Run(() =>
		{
			var manager = new GtkFontManager(null);
			Assert.True(manager.DefaultFontSize >= 8 && manager.DefaultFontSize <= 40, $"default size {manager.DefaultFontSize}");
			Assert.Contains($"font-size: {manager.DefaultFontSize:0.##}px", manager.GetCssFont(default));
		});

		[GtkFact]
		public void FontDescriptionIsAbsoluteSized() => GtkTestHost.Run(() =>
		{
			var manager = new GtkFontManager(null);
			var description = manager.GetFontDescription(Font.OfSize("Sans", 20, FontWeight.Bold, FontSlant.Oblique));

			Assert.Equal("Sans", description.Family);
			Assert.Equal(Pango.Weight.Bold, description.Weight);
			Assert.Equal(Pango.Style.Oblique, description.Style);
			Assert.True(description.SizeIsAbsolute);
			Assert.Equal(20 * 1024, description.Size);
		});

		[GtkFact]
		public void RegisteredAliasesResolveToTheFontFamilyOrTheAlias() => GtkTestHost.Run(() =>
		{
			var registrar = new FixedRegistrar("Alias", Path.Combine(Path.GetTempPath(), "missing-font-file.ttf"));
			var manager = new GtkFontManager(registrar);

			// The file cannot be queried (it does not exist), so the alias is used as the family.
			Assert.Equal("Alias", manager.GetFontFamily("Alias"));
			Assert.Equal("Other", manager.GetFontFamily("Other"));
			Assert.Null(manager.GetFontFamily(null));
			Assert.Null(manager.GetFontFamily("  "));
		});

		sealed class FixedRegistrar : IFontRegistrar
		{
			readonly string _alias;
			readonly string _path;

			public FixedRegistrar(string alias, string path)
			{
				_alias = alias;
				_path = path;
			}

			public void Register(string filename, string? alias, System.Reflection.Assembly assembly) { }

			public void Register(string filename, string? alias) { }

			public string? GetFont(string font) => font == _alias ? _path : null;
		}

		[GtkFact]
		public void EmbeddedFontLoaderExtractsTheFontNextToThePangoSearchPath() => GtkTestHost.Run(() =>
		{
			var name = $"maui-gtk-test-{Guid.NewGuid():N}.ttf";
			var loader = new GtkEmbeddedFontLoader();
			string? path = null;
			try
			{
				path = loader.LoadFont(new EmbeddedFont { FontName = name, ResourceStream = new MemoryStream(new byte[] { 1, 2, 3, 4 }) });

				Assert.NotNull(path);
				Assert.True(File.Exists(path));
				Assert.Equal(name, Path.GetFileName(path));
				Assert.Equal(new byte[] { 1, 2, 3, 4 }, File.ReadAllBytes(path!));

				// A second call finds the extracted file.
				Assert.Equal(path, loader.LoadFont(new EmbeddedFont { FontName = name, ResourceStream = null }));
			}
			finally
			{
				if (path is not null)
					File.Delete(path);
			}
		});

		[GtkFact]
		public void EmbeddedFontLoaderRejectsMissingNamesAndNeverEscapesItsDirectory() => GtkTestHost.Run(() =>
		{
			var loader = new GtkEmbeddedFontLoader();
			Assert.Null(loader.LoadFont(new EmbeddedFont { FontName = "", ResourceStream = new MemoryStream() }));
			Assert.Null(loader.LoadFont(new EmbeddedFont { FontName = "ok.ttf", ResourceStream = null }));

			var name = $"escape-{Guid.NewGuid():N}.ttf";
			var path = loader.LoadFont(new EmbeddedFont { FontName = Path.Combine("..", "..", name), ResourceStream = new MemoryStream(new byte[] { 9 }) });
			try
			{
				Assert.NotNull(path);
				Assert.Equal(name, Path.GetFileName(path));
				Assert.DoesNotContain("..", path);
			}
			finally
			{
				if (path is not null)
					File.Delete(path);
			}
		});
	}

	public class ImageSourceServiceTests
	{
		[GtkFact]
		public void FileServiceResolvesRelativeNamesAndExtensions() => GtkTestHost.Run(() =>
		{
			var path = TestImages.WritePng(3, 2, 1, 2, 3);
			try
			{
				var service = new GtkFileImageSourceService();
				var source = new FileSource(path);
				var result = GtkTestHost.Await(service.GetTextureAsync(source));

				Assert.NotNull(result);
				Assert.Equal(3, result!.Value.Width);
				Assert.Equal(2, result.Value.Height);
				Assert.False(result.IsResolutionDependent);

				// Extension-less names are probed next to the application.
				var withoutExtension = Path.Combine(AppContext.BaseDirectory, $"probe-{Guid.NewGuid():N}");
				File.Copy(path, withoutExtension + ".png");
				try
				{
					var probed = GtkTestHost.Await(service.GetTextureAsync(new FileSource(Path.GetFileName(withoutExtension))));
					Assert.NotNull(probed);
				}
				finally
				{
					File.Delete(withoutExtension + ".png");
				}
			}
			finally
			{
				File.Delete(path);
			}
		});

		[GtkFact]
		public void ServicesReturnNullForEmptyOrUnreadableSources() => GtkTestHost.Run(() =>
		{
			var files = new GtkFileImageSourceService();
			Assert.Null(GtkTestHost.Await(files.GetTextureAsync(new FileSource(string.Empty))));
			Assert.Null(GtkTestHost.Await(files.GetTextureAsync(new FileSource("nope-" + Guid.NewGuid().ToString("N") + ".png"))));

			var streams = new GtkStreamImageSourceService();
			Assert.Null(GtkTestHost.Await(streams.GetTextureAsync(new StreamSource(new byte[] { 1, 2, 3 }))));
			Assert.Null(GtkTestHost.Await(streams.GetTextureAsync(new StreamSource(Array.Empty<byte>()))));
		});

		[GtkFact]
		public void StreamServiceDecodesPngBytes() => GtkTestHost.Run(() =>
		{
			var service = new GtkStreamImageSourceService();
			var result = GtkTestHost.Await(service.GetTextureAsync(new StreamSource(TestImages.CreatePng(5, 7, 9, 9, 9))));

			Assert.NotNull(result);
			Assert.Equal(5, result!.Value.Width);
			Assert.Equal(7, result.Value.Height);
		});

		sealed class FileSource : IFileImageSource
		{
			public FileSource(string file) => File = file;

			public string File { get; }

			public bool IsEmpty => string.IsNullOrEmpty(File);
		}

		sealed class StreamSource : IStreamImageSource
		{
			readonly byte[] _data;

			public StreamSource(byte[] data) => _data = data;

			public bool IsEmpty => false;

			public System.Threading.Tasks.Task<Stream> GetStreamAsync(System.Threading.CancellationToken cancellationToken = default) =>
				System.Threading.Tasks.Task.FromResult<Stream>(new MemoryStream(_data));
		}
	}

	public class PanelTests
	{
		sealed class FixedLayout : ICrossPlatformLayout
		{
			public Size Measured = new(120, 80);
			public Rect Arranged;
			public int Measures;

			public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
			{
				Measures++;
				return Measured;
			}

			public Size CrossPlatformArrange(Rect bounds)
			{
				Arranged = bounds;
				return bounds.Size;
			}
		}

		[GtkFact]
		public void PanelReportsDesiredNaturalSizeAndZeroMinimum() => GtkTestHost.Run(() =>
		{
			var layout = new FixedLayout();
			var panel = new MauiGtkPanel { CrossPlatformLayout = layout };

			panel.Measure(global::Gtk.Orientation.Horizontal, -1, out int minimumWidth, out int naturalWidth, out _, out _);
			panel.Measure(global::Gtk.Orientation.Vertical, 120, out int minimumHeight, out int naturalHeight, out _, out _);

			Assert.Equal(0, minimumWidth);
			Assert.Equal(120, naturalWidth);
			Assert.Equal(0, minimumHeight);
			Assert.Equal(80, naturalHeight);
			Assert.Equal(global::Gtk.SizeRequestMode.HeightForWidth, panel.RequestMode);

			panel.MinimumWidthFollowsContent = true;
			panel.MinimumHeightFollowsContent = true;
			panel.QueueResize();
			panel.Measure(global::Gtk.Orientation.Horizontal, -1, out minimumWidth, out _, out _, out _);
			panel.Measure(global::Gtk.Orientation.Vertical, 120, out minimumHeight, out _, out _, out _);
			Assert.Equal(120, minimumWidth);
			Assert.Equal(80, minimumHeight);
		});

		[GtkFact]
		public void MauiMeasuredPanelsReportTheMauiMeasureInsteadOfRemeasuring() => GtkTestHost.Run(() =>
		{
			var layout = new FixedLayout();
			var panel = new MauiGtkPanel { CrossPlatformLayout = layout, IsMeasuredByMaui = true };

			// Before MAUI measured, GTK's measure falls back to the cross-platform measure.
			panel.Measure(global::Gtk.Orientation.Horizontal, -1, out _, out int fallback, out _, out _);
			Assert.Equal(120, fallback);
			int measuresAfterFallback = layout.Measures;

			panel.UpdateMeasuredSize(new Size(50, 30));
			panel.Measure(global::Gtk.Orientation.Horizontal, -1, out _, out int width, out _, out _);
			panel.Measure(global::Gtk.Orientation.Vertical, 50, out _, out int height, out _, out _);

			Assert.Equal(50, width);
			Assert.Equal(30, height);
			Assert.Equal(measuresAfterFallback, layout.Measures);
		});

		[GtkFact]
		public void PanelWithoutALayoutMeasuresZero() => GtkTestHost.Run(() =>
		{
			var panel = new MauiGtkPanel();
			panel.Measure(global::Gtk.Orientation.Horizontal, -1, out int minimum, out int natural, out _, out _);
			Assert.Equal(0, minimum);
			Assert.Equal(0, natural);
		});

		[GtkFact]
		public void PanelAllocationRunsTheCrossPlatformArrange() => GtkTestHost.Run(() =>
		{
			var layout = new FixedLayout();
			var panel = new MauiGtkPanel { CrossPlatformLayout = layout };
			var window = new global::Gtk.Window { Decorated = false, Child = panel };
			window.SetDefaultSize(300, 200);
			window.Present();
			GtkTestHost.Pump(window);
			GtkTestHost.Resize(window, 300, 200);

			Assert.Equal(new Rect(0, 0, 300, 200), layout.Arranged);
			GtkTestHost.Retire(window);
		});

		[GtkFact]
		public void RootPanelMeasuresZeroAndMeasuresTheContentBeforeArranging() => GtkTestHost.Run(() =>
		{
			var layout = new FixedLayout();
			var root = new MauiGtkRootPanel { CrossPlatformLayout = layout };
			root.Measure(global::Gtk.Orientation.Horizontal, -1, out int minimum, out int natural, out _, out _);
			Assert.Equal(0, minimum);
			Assert.Equal(0, natural);
			Assert.Equal(0, layout.Measures);

			int width = 0;
			root.SizeChanged += (w, _) => width = w;
			var window = new global::Gtk.Window { Decorated = false, Child = root };
			window.SetDefaultSize(250, 150);
			window.Present();
			GtkTestHost.Pump(window);
			GtkTestHost.Resize(window, 250, 150);

			Assert.True(layout.Measures > 0, "the root panel runs the cross-platform measure itself");
			Assert.Equal(250, width);
			Assert.Equal(new Rect(0, 0, 250, 150), layout.Arranged);
			GtkTestHost.Retire(window);
		});

		[GtkFact]
		public void ChildOperationsKeepTheSiblingOrder() => GtkTestHost.Run(() =>
		{
			var panel = new MauiGtkPanel();
			var a = new GtkLabel("a");
			var b = new GtkLabel("b");
			var c = new GtkLabel("c");

			panel.AddChild(a);
			panel.AddChild(c);
			panel.InsertChild(1, b);
			Assert.Equal(0, panel.IndexOfChild(a));
			Assert.Equal(1, panel.IndexOfChild(b));
			Assert.Equal(2, panel.IndexOfChild(c));

			// Re-inserting an existing child moves it.
			panel.InsertChild(0, c);
			Assert.Equal(0, panel.IndexOfChild(c));

			var replacement = new GtkLabel("r");
			panel.ReplaceChild(b, replacement);
			Assert.Equal(-1, panel.IndexOfChild(b));
			Assert.Equal(2, panel.IndexOfChild(replacement));
			Assert.Null(b.Parent);

			panel.RemoveChild(a);
			Assert.Null(a.Parent);

			panel.ClearChildren();
			Assert.Null(panel.FirstChild);

			Assert.Throws<ArgumentNullException>(() => panel.InsertChild(0, null!));
		});

		[GtkFact]
		public void InsertingAChildOfAnotherParentIsRejected() => GtkTestHost.Run(() =>
		{
			var first = new MauiGtkPanel();
			var second = new MauiGtkPanel();
			var child = new GtkLabel("x");
			first.AddChild(child);

			Assert.Throws<InvalidOperationException>(() => second.AddChild(child));
		});

		[GtkFact]
		public void WrapperPassesMeasureThroughAndSwapsItsChild() => GtkTestHost.Run(() =>
		{
			var wrapper = new GtkWrapperView();
			wrapper.Measure(global::Gtk.Orientation.Horizontal, -1, out int emptyMinimum, out int emptyNatural, out _, out _);
			Assert.Equal(0, emptyMinimum);
			Assert.Equal(0, emptyNatural);

			var label = new GtkLabel("some text");
			wrapper.Child = label;
			Assert.Same(wrapper, label.Parent);

			label.Measure(global::Gtk.Orientation.Horizontal, -1, out int labelMinimum, out int labelNatural, out _, out _);
			wrapper.Measure(global::Gtk.Orientation.Horizontal, -1, out int minimum, out int natural, out _, out _);
			Assert.Equal(labelMinimum, minimum);
			Assert.Equal(labelNatural, natural);

			var other = new GtkLabel("other");
			wrapper.ReplaceChild(label, other);
			Assert.Null(label.Parent);
			Assert.Same(wrapper, other.Parent);

			wrapper.Child = null;
			Assert.Null(other.Parent);
		});

		[GtkFact]
		public void PathConverterBuildsGskPathsFromMauiPaths() => GtkTestHost.Run(() =>
		{
			var path = new PathF();
			path.MoveTo(0, 0);
			path.LineTo(10, 0);
			path.QuadTo(15, 5, 10, 10);
			path.CurveTo(8, 12, 2, 12, 0, 10);
			path.Close();

			Assert.NotNull(GtkPathConverter.ToGskPath(path));

			var arc = new PathF();
			arc.AppendCircle(5, 5, 5);
			Assert.NotNull(GtkPathConverter.ToGskPath(arc));

			Assert.Throws<ArgumentNullException>(() => GtkPathConverter.ToGskPath(null!));
		});

		[GtkFact]
		public void ViewStateCopiesAndIsPerWidget() => GtkTestHost.Run(() =>
		{
			var first = new GtkLabel("a");
			var second = new GtkLabel("b");
			var state = GtkViewState.For(first);
			Assert.Same(state, GtkViewState.For(first));
			Assert.Null(GtkViewState.TryGet(second));

			state.Frame = new Rect(1, 2, 3, 4);
			state.HasFrame = true;
			state.TranslationX = 5;
			state.Rotation = 90;
			state.RotationX = 30;
			state.RotationY = 40;
			state.ScaleX = 2;
			Assert.True(state.HasTransform);

			var copy = GtkViewState.For(second);
			copy.CopyFrom(state);
			Assert.Equal(new Rect(1, 2, 3, 4), copy.Frame);
			Assert.True(copy.HasFrame);
			Assert.Equal(5, copy.TranslationX);
			Assert.Equal(90, copy.Rotation);
			Assert.Equal(30, copy.RotationX);
			Assert.Equal(40, copy.RotationY);
			Assert.Equal(2, copy.ScaleX);
		});

		[GtkFact]
		public void RuntimeVersionGuardReportsTheLoadedGtk() => GtkTestHost.Run(() =>
		{
			Assert.True(GtkRuntime.IsInitialized);
			Assert.True(GtkRuntime.IsGtkThread);
			Assert.True(GtkRuntime.AtLeast(4, GtkRuntime.MinimumMinorVersion), "the backend requires GTK 4.14 or newer");
			Assert.False(GtkRuntime.AtLeast(5, 0));

			bool ran = false;
			GtkRuntime.Post(() => ran = true);
			Assert.True(ran, "Post runs inline on the GTK thread");
		});
	}
}

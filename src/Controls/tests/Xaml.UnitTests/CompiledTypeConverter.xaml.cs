using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class CompiledTypeConverter : ContentPage
{
	public static readonly BindableProperty RectangleBPProperty =
		BindableProperty.Create(nameof(RectangleBP), typeof(Rect), typeof(CompiledTypeConverter), default(Rect));

	public Rect RectangleBP
	{
		get { return (Rect)GetValue(RectangleBPProperty); }
		set { SetValue(RectangleBPProperty, value); }
	}

	public Rect RectangleP { get; set; }

	public Point PointP { get; set; }

	public Brush BrushByName { get; set; }

	public Brush BrushByARGB { get; set; }

	public Brush BrushByRGB { get; set; }

	public ImageSource ImageByUrl { get; set; }

	public ImageSource ImageByName { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape EllipseShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape LineShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape LineShapeTwo { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape LineShapeFour { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape PolygonShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape PolylineShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape RectangleShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape RoundRectangleShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape PathShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
	public IList<string> List { get; set; }

	[System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))]
	public Type ButtonType { get; set; }

	public BindableProperty BindableProp { get; set; }


	public CompiledTypeConverter() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void CompiledTypeConverterAreInvoked(XamlInflator xamlInflator)
		{
			var p = new CompiledTypeConverter(xamlInflator);
			Assert.Equal(new Rect(0, 1, 2, 4), p.RectangleP);
			Assert.Equal(new Rect(4, 8, 16, 32), p.RectangleBP);
			Assert.Equal(new Point(1, 2), p.PointP);
			Assert.Equal(Brush.Red, p.BrushByName);
			Assert.Equal(new Color(1, 2, 3, 0), ((SolidColorBrush)p.BrushByARGB).Color);
			Assert.Equal(new Color(1, 2, 3), ((SolidColorBrush)p.BrushByRGB).Color);
			Assert.Equal("https://picsum.photos/200/300", ((UriImageSource)p.ImageByUrl).Uri.AbsoluteUri);
			Assert.Equal("foo.png", ((FileImageSource)p.ImageByName).File);
			Assert.Equal(Colors.Pink, p.BackgroundColor);
			Assert.IsType<Ellipse>(p.EllipseShape);
			Assert.IsType<Line>(p.LineShape);
			Assert.Equal(1, ((Line)p.LineShapeTwo).X1);
			Assert.Equal(2, ((Line)p.LineShapeTwo).Y1);
			Assert.Equal(1, ((Line)p.LineShapeFour).X1);
			Assert.Equal(2, ((Line)p.LineShapeFour).Y1);
			Assert.Equal(3, ((Line)p.LineShapeFour).X2);
			Assert.Equal(4, ((Line)p.LineShapeFour).Y2);
			Assert.Equal(3, ((Shapes.Polygon)p.PolygonShape).Points.Count);
			Assert.Equal(10, ((Shapes.Polyline)p.PolylineShape).Points.Count);
			Assert.IsType<Rectangle>(p.RectangleShape);
			Assert.Equal(new CornerRadius(1, 2, 3, 4), ((RoundRectangle)p.RoundRectangleShape).CornerRadius);
			Assert.Equal(3, ((PathGeometry)((Path)p.PathShape).Data).Figures.Count);
			Assert.Equal(LayoutOptions.EndAndExpand, p.label.GetValue(View.HorizontalOptionsProperty));
			var xConstraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetXConstraint(p.label);
			Assert.Equal(2, xConstraint.Compute(null));
			Assert.Equal(new Thickness(2, 3), p.label.Margin);
			Assert.Equal(2, p.List.Count);
			Assert.Equal("Bar", p.List[1]);
			Assert.Equal(typeof(Button), p.ButtonType);
			Assert.Equal(Label.TextProperty, p.BindableProp);
		}

		public static IEnumerable<object[]> ConverterTestData =>
			from inflator in new[] { XamlInflator.XamlC, XamlInflator.SourceGen }
			from converterType in new[]
			{
				typeof(BrushTypeConverter),
				typeof(ImageSourceConverter),
				typeof(StrokeShapeTypeConverter),
				typeof(Graphics.Converters.PointTypeConverter),
				typeof(Graphics.Converters.RectTypeConverter),
				typeof(TypeTypeConverter),
				typeof(BindablePropertyConverter),
				typeof(ListStringTypeConverter)
			}
			select new object[] { inflator, converterType };

		[Theory]
		[MemberData(nameof(ConverterTestData))]
		internal void ConvertersAreReplaced(XamlInflator inflator, Type converterType)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(CompiledTypeConverter), out var methodDef, out bool hasLoggedErrors);
				Assert.False(hasLoggedErrors);
				Assert.False(methodDef.Body.Instructions.Any(instr => HasConstructorForType(methodDef, instr, converterType)), $"This Xaml still generates a new {converterType}()");
			}

			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class CompiledTypeConverter : ContentPage
{
	public static readonly BindableProperty RectangleBPProperty =
		BindableProperty.Create("RectangleBP", typeof(Rect), typeof(CompiledTypeConverter), default(Rect));

	public Rect RectangleBP
	{
		get { return (Rect)GetValue(RectangleBPProperty); }
		set { SetValue(RectangleBPProperty, value); }
	}

	public Rect RectangleP { get; set; }

	public Point PointP { get; set; }

	public Brush BrushByName { get; set; }

	public Brush BrushByARGB { get; set; }

	public Brush BrushByRGB { get; set; }

	public ImageSource ImageByUrl { get; set; }

	public ImageSource ImageByName { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape EllipseShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape LineShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape LineShapeTwo { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape LineShapeFour { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape PolygonShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape PolylineShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape RectangleShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape RoundRectangleShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
	public IShape PathShape { get; set; }

	[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
	public IList<string> List { get; set; }

	[System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))]
	public Type ButtonType { get; set; }

	public BindableProperty BindableProp { get; set; }
}
""")
					.RunMauiSourceGenerator(typeof(CompiledTypeConverter));

				Assert.False(result.Diagnostics.Any());
				var boilerplate = result.GeneratedCodeBehind();
				var initComp = result.GeneratedInitializeComponent();
				if (converterType == typeof(Graphics.Converters.RectTypeConverter))
					Assert.True(result.GeneratedInitializeComponent().Contains("new global::Microsoft.Maui.Graphics.Rect(0, 1, 2, 4)", StringComparison.InvariantCulture));
				if (converterType == typeof(Graphics.Converters.PointTypeConverter))
					Assert.True(result.GeneratedInitializeComponent().Contains("new global::Microsoft.Maui.Graphics.Point(1, 2)", StringComparison.InvariantCulture));
				if (converterType == typeof(TypeTypeConverter))
					Assert.True(result.GeneratedInitializeComponent().Contains("typeof(global::Microsoft.Maui.Controls.Button)", StringComparison.InvariantCulture));
				if (converterType == typeof(BindablePropertyConverter))
					Assert.True(result.GeneratedInitializeComponent().Contains("global::Microsoft.Maui.Controls.Label.TextProperty", StringComparison.InvariantCulture));
				if (converterType == typeof(ListStringTypeConverter))
					Assert.True(result.GeneratedInitializeComponent().Contains("new global::System.Collections.Generic.List<string> { \"Foo\", \"Bar\" }", StringComparison.InvariantCulture));
				// TODO check all other converters here

				// TODO check that there are no ConvertFrom calls

				//check that there are no serviceProvider
				Assert.False(result.GeneratedInitializeComponent().Contains("new global::Microsoft.Maui.Controls.Xaml.Internals.XamlServiceProvider(", StringComparison.InvariantCulture));
			}
		}

		bool HasConstructorForType(MethodDefinition methodDef, Instruction instruction, Type converterType)
		{
			if (instruction.OpCode != OpCodes.Newobj)
				return false;
			if (!(instruction.Operand is MethodReference methodRef))
				return false;
			if (!Build.Tasks.TypeRefComparer.Default.Equals(methodRef.DeclaringType, methodDef.Module.ImportReference(converterType)))
				return false;
			return true;
		}
	}
}

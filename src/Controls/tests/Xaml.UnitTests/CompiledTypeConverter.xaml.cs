using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void CompiledTypeConverterAreInvoked([Values] XamlInflator xamlInflator)
		{
			var p = new CompiledTypeConverter(xamlInflator);
			Assert.AreEqual(new Rect(0, 1, 2, 4), p.RectangleP);
			Assert.AreEqual(new Rect(4, 8, 16, 32), p.RectangleBP);
			Assert.AreEqual(new Point(1, 2), p.PointP);
			Assert.AreEqual(Brush.Red, p.BrushByName);
			Assert.AreEqual(new Color(1, 2, 3, 0), ((SolidColorBrush)p.BrushByARGB).Color);
			Assert.AreEqual(new Color(1, 2, 3), ((SolidColorBrush)p.BrushByRGB).Color);
			Assert.AreEqual("https://picsum.photos/200/300", ((UriImageSource)p.ImageByUrl).Uri.AbsoluteUri);
			Assert.AreEqual("foo.png", ((FileImageSource)p.ImageByName).File);
			Assert.AreEqual(Colors.Pink, p.BackgroundColor);
			Assert.IsInstanceOf<Ellipse>(p.EllipseShape);
			Assert.IsInstanceOf<Line>(p.LineShape);
			Assert.AreEqual(1, ((Line)p.LineShapeTwo).X1);
			Assert.AreEqual(2, ((Line)p.LineShapeTwo).Y1);
			Assert.AreEqual(1, ((Line)p.LineShapeFour).X1);
			Assert.AreEqual(2, ((Line)p.LineShapeFour).Y1);
			Assert.AreEqual(3, ((Line)p.LineShapeFour).X2);
			Assert.AreEqual(4, ((Line)p.LineShapeFour).Y2);
			Assert.AreEqual(3, ((Shapes.Polygon)p.PolygonShape).Points.Count);
			Assert.AreEqual(10, ((Shapes.Polyline)p.PolylineShape).Points.Count);
			Assert.IsInstanceOf<Rectangle>(p.RectangleShape);
			Assert.AreEqual(new CornerRadius(1, 2, 3, 4), ((RoundRectangle)p.RoundRectangleShape).CornerRadius);
			Assert.AreEqual(3, ((PathGeometry)((Path)p.PathShape).Data).Figures.Count);
			Assert.AreEqual(LayoutOptions.EndAndExpand, p.label.GetValue(View.HorizontalOptionsProperty));
			var xConstraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetXConstraint(p.label);
			Assert.AreEqual(2, xConstraint.Compute(null));
			Assert.AreEqual(new Thickness(2, 3), p.label.Margin);
			Assert.AreEqual(2, p.List.Count);
			Assert.AreEqual("Bar", p.List[1]);
			Assert.AreEqual(typeof(Button), p.ButtonType);
			Assert.AreEqual(Label.TextProperty, p.BindableProp);
		}

		[Test]
		public void ConvertersAreReplaced(
				[Values] XamlInflator inflator,
				[Values(
					typeof(BrushTypeConverter),
					typeof(ImageSourceConverter),
					typeof(StrokeShapeTypeConverter),
					typeof(Graphics.Converters.PointTypeConverter),
					typeof(Graphics.Converters.RectTypeConverter),
					typeof(TypeTypeConverter),
					typeof(BindablePropertyConverter),
					typeof(ListStringTypeConverter)
					)] Type converterType)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(CompiledTypeConverter), out var methodDef, out bool hasLoggedErrors);
				Assert.That(!hasLoggedErrors);
				Assert.That(!methodDef.Body.Instructions.Any(instr => HasConstructorForType(methodDef, instr, converterType)), $"This Xaml still generates a new {converterType}()");
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
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
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

				Assert.IsFalse(result.Diagnostics.Any());
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

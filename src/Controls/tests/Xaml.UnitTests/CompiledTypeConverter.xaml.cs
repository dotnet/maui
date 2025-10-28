using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
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


		public CompiledTypeConverter() => InitializeComponent();

		public CompiledTypeConverter(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void CompiledTypeConverterAreInvoked(bool useCompiledXaml)
			{
				var p = new CompiledTypeConverter(useCompiledXaml);
				Assert.Equal(new Rect(0, 1, 2, 4), p.RectangleP);
				Assert.Equal(new Rect(4, 8, 16, 32), p.RectangleBP);
				Assert.Equal(new Point(1, 2), p.PointP);
				Assert.Equal(Brush.Red, p.BrushByName);
				Assert.Equal(new Color(1, 2, 3, 0), ((SolidColorBrush)p.BrushByARGB).Color);
				Assert.Equal(new Color(1, 2, 3), ((SolidColorBrush)p.BrushByRGB).Color);
				Assert.Equal("https://picsum.photos/200/300", ((UriImageSource)p.ImageByUrl).Uri.AbsoluteUri);
				Assert.Equal("foo.png", ((FileImageSource)p.ImageByName).File);
				Assert.Equal(Colors.Pink, p.BackgroundColor);
				Assert.IsInstanceOf<Ellipse>(p.EllipseShape);
				Assert.IsInstanceOf<Line>(p.LineShape);
				Assert.Equal(1, ((Line)p.LineShapeTwo).X1);
				Assert.Equal(2, ((Line)p.LineShapeTwo).Y1);
				Assert.Equal(1, ((Line)p.LineShapeFour).X1);
				Assert.Equal(2, ((Line)p.LineShapeFour).Y1);
				Assert.Equal(3, ((Line)p.LineShapeFour).X2);
				Assert.Equal(4, ((Line)p.LineShapeFour).Y2);
				Assert.Equal(3, ((Shapes.Polygon)p.PolygonShape).Points.Count);
				Assert.Equal(10, ((Shapes.Polyline)p.PolylineShape).Points.Count);
				Assert.IsInstanceOf<Rectangle>(p.RectangleShape);
				Assert.Equal(new CornerRadius(1, 2, 3, 4), ((RoundRectangle)p.RoundRectangleShape).CornerRadius);
				Assert.Equal(3, ((PathGeometry)((Path)p.PathShape).Data).Figures.Count);
				Assert.Equal(LayoutOptions.EndAndExpand, p.label.GetValue(View.HorizontalOptionsProperty));
				var xConstraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetXConstraint(p.label);
				Assert.Equal(2, xConstraint.Compute(null));
				Assert.Equal(new Thickness(2, 3), p.label.Margin);
				Assert.Equal(2, p.List.Count);
				Assert.Equal("Bar", p.List[1]);
			}

			[Theory]
	[InlineData(typeof(Microsoft.Maui.Controls.BrushTypeConverter))]
			[InlineData(typeof(Microsoft.Maui.Controls.ImageSourceConverter))]
			[InlineData(typeof(Microsoft.Maui.Controls.Shapes.StrokeShapeTypeConverter))]
			[InlineData(typeof(Microsoft.Maui.Graphics.Converters.PointTypeConverter))]
			[InlineData(typeof(Microsoft.Maui.Graphics.Converters.RectTypeConverter))]
			public void ConvertersAreReplaced(Type converterType)
			{
				MockCompiler.Compile(typeof(CompiledTypeConverter), out var methodDef, out var hasLoggedErrors);
				Assert.True(!hasLoggedErrors);
				Assert.True(!methodDef.Body.Instructions.Any(instr => HasConstructorForType(methodDef, instr, converterType)), $"This Xaml still generates a new {converterType}()");
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
}

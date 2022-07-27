using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

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

		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> List { get; set; }


		public CompiledTypeConverter() => InitializeComponent();

		public CompiledTypeConverter(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void CompiledTypeConverterAreInvoked(bool useCompiledXaml)
			{
				var p = new CompiledTypeConverter(useCompiledXaml);
				Assert.AreEqual(new Rect(0, 1, 2, 4), p.RectangleP);
				Assert.AreEqual(new Rect(4, 8, 16, 32), p.RectangleBP);
				Assert.AreEqual(new Point(1, 2), p.PointP);
				Assert.AreEqual(Brush.Red, p.BrushByName);
				Assert.AreEqual(new Color(1, 2, 3, 0), ((SolidColorBrush)p.BrushByARGB).Color);
				Assert.AreEqual(new Color(1, 2, 3), ((SolidColorBrush)p.BrushByRGB).Color);
				Assert.AreEqual("https://picsum.photos/200/300", ((UriImageSource)p.ImageByUrl).Uri.AbsoluteUri);
				Assert.AreEqual("foo.png", ((FileImageSource)p.ImageByName).File);
				Assert.AreEqual(Colors.Pink, p.BackgroundColor);
				Assert.AreEqual(LayoutOptions.EndAndExpand, p.label.GetValue(View.HorizontalOptionsProperty));
				var xConstraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetXConstraint(p.label);
				Assert.AreEqual(2, xConstraint.Compute(null));
				Assert.AreEqual(new Thickness(2, 3), p.label.Margin);
				Assert.AreEqual(2, p.List.Count);
				Assert.AreEqual("Bar", p.List[1]);
			}

			[Test]
			[TestCase(typeof(Microsoft.Maui.Controls.BrushTypeConverter))]
			[TestCase(typeof(Microsoft.Maui.Controls.ImageSourceConverter))]
			[TestCase(typeof(Microsoft.Maui.Graphics.Converters.PointTypeConverter))]
			[TestCase(typeof(Microsoft.Maui.Graphics.Converters.RectTypeConverter))]
			public void ConvertersAreReplaced(Type converterType)
			{
				MockCompiler.Compile(typeof(CompiledTypeConverter), out var methodDef);
				Assert.That(!methodDef.Body.Instructions.Any(instr => HasConstructorForType(methodDef, instr, converterType)), $"This Xaml still generates a new {converterType}()");
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

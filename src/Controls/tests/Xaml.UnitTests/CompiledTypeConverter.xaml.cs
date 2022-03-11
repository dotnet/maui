using System.Collections.Generic;
using Microsoft.Maui.Graphics;
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
				Assert.AreEqual(Colors.Pink, p.BackgroundColor);
				Assert.AreEqual(LayoutOptions.EndAndExpand, p.label.GetValue(View.HorizontalOptionsProperty));
				var xConstraint = Microsoft.Maui.Controls.Compatibility.RelativeLayout.GetXConstraint(p.label);
				Assert.AreEqual(2, xConstraint.Compute(null));
				Assert.AreEqual(new Thickness(2, 3), p.label.Margin);
				Assert.AreEqual(2, p.List.Count);
				Assert.AreEqual("Bar", p.List[1]);
			}
		}
	}
}

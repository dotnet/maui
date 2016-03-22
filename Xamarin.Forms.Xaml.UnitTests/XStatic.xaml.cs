using System;
using System.Collections.Generic;

using Xamarin.Forms;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class MockxStatic
	{
		public static string MockStaticProperty { get { return "Property"; } }
		public const string MockConstant = "Constant";
		public static string MockField = "Field";
		public string InstanceProperty { get { return "InstanceProperty"; } }
		public static readonly Color BackgroundColor = Color.Fuchsia;
	}

	public enum MockEnum
	{
		First,
		Second,
		Third,
	}

	public partial class XStatic : ContentPage
	{
		public XStatic ()
		{
			InitializeComponent ();
		}
		public XStatic (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase (false)]
			[TestCase (true)]
			public void StaticProperty (bool useCompiledXaml)
			{
				var layout = new XStatic (useCompiledXaml);
				Assert.AreEqual ("Property", layout.staticproperty.Text);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void MemberOptional (bool useCompiledXaml)
			{
				var layout = new XStatic (useCompiledXaml);
				Assert.AreEqual ("Property", layout.memberisoptional.Text);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void FieldColor (bool useCompiledXaml)
			{
				var layout = new XStatic (useCompiledXaml);
				Assert.AreEqual (Color.Fuchsia, layout.color.TextColor);
			}
		}
	}
}
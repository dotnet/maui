using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Unreported004 : ContentPage
	{
		public Unreported004()
		{
			InitializeComponent();
		}

		public Unreported004(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public static readonly BindableProperty SomePropertyProperty =
			BindableProperty.Create("SomeProperty", typeof(string),
			typeof(Unreported004), null);

		public static string GetSomeProperty(BindableObject bindable)
		{
			return bindable.GetValue(SomePropertyProperty) as string;
		}

		public static string GetSomeProperty(BindableObject bindable, object foo)
		{
			return null;
		}

		public static void SetSomeProperty(BindableObject bindable, string value)
		{
			bindable.SetValue(SomePropertyProperty, value);
		}


		[TestFixture]
		class Tests
		{
			[TestCase(true), TestCase(false)]
			public void MultipleGetMethodsAllowed(bool useCompiledXaml)
			{
				var page = new Unreported004(useCompiledXaml);
				Assert.NotNull(page.label);
				Assert.AreEqual("foo", GetSomeProperty(page.label));
			}
		}
	}
}


using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Bz47950Behavior : Behavior<View>
	{
		public static readonly BindableProperty ColorTestProperty =
			BindableProperty.CreateAttached("ColorTest", typeof(Color), typeof(View), default(Color));

		public static Color GetColorTest(BindableObject bindable) => (Color)bindable.GetValue(ColorTestProperty);
		public static void SetColorTest(BindableObject bindable, Color value) => bindable.SetValue(ColorTestProperty, value);
	}

	public partial class Bz47950 : ContentPage
	{
		public Bz47950()
		{
			InitializeComponent();
		}

		public Bz47950(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void BehaviorAndStaticResource(bool useCompiledXaml)
			{
				var page = new Bz47950(useCompiledXaml);
			}
		}
	}
}

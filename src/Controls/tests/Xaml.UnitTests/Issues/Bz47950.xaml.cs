using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void BehaviorAndStaticResource([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Bz47950(useCompiledXaml);
			}
		}
	}
}

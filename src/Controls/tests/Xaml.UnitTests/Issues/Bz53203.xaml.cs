using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;


namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public enum Bz53203Values
	{
		Unknown,
		Good,
		Better,
		Best
	}

	public partial class Bz53203 : ContentPage
	{
		public static int IntValue = 42;
		public static object ObjValue = new object();

		public static readonly BindableProperty ParameterProperty = BindableProperty.CreateAttached("Parameter",
			typeof(object), typeof(Bz53203), null);

		public static object GetParameter(BindableObject obj) =>
			obj.GetValue(ParameterProperty);

		public static void SetParameter(BindableObject obj, object value) =>
			obj.SetValue(ParameterProperty, value);

		public Bz53203()
		{
			InitializeComponent();
		}

		public Bz53203(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(true)]
			public void MarkupOnAttachedBPDoesNotThrowAtCompileTime()
			{
				MockCompiler.Compile(typeof(Bz53203));
			}

			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void MarkupOnAttachedBP(bool useCompiledXaml)
			{
				var page = new Bz53203(useCompiledXaml);
				var label = page.label0;
				Assert.Equal(42, Grid.GetRow(label));
				Assert.Equal(Bz53203Values.Better, GetParameter(label));
			}

		}
	}
}
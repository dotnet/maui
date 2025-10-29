// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh7559 : ContentPage
	{
		public Gh7559() => InitializeComponent();
		public Gh7559(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Gh7559));
				var layout = new Gh7559(useCompiledXaml);
				var value = Gh7559Generic<Gh7559Enum>.GetIcon(layout);
				Assert.Equal(Gh7559Enum.LetterA, value);
			}
		}
	}

	public abstract class Gh7559Generic<T>
	{
		public static readonly BindableProperty IconProperty = BindableProperty.Create("Icon", typeof(T), typeof(Gh7559Generic<T>), default(T));

		public static T GetIcon(BindableObject bindable)
		{
			return (T)bindable.GetValue(IconProperty);
		}

		public static void SetIcon(BindableObject bindable, T value)
		{
			bindable.SetValue(IconProperty, value);
		}
	}

	public enum Gh7559Enum
	{
		LetterX = 'X',
		LetterA = 'A',
	}

	public class Gh7559A : Gh7559Generic<Gh7559Enum>
	{ }
}

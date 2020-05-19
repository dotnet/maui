// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Maui;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public partial class Gh7559 : ContentPage
	{
		public Gh7559() => InitializeComponent();
		public Gh7559(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void GenericBPCompiles([Values(false, true)]bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Gh7559));
				var layout = new Gh7559(useCompiledXaml);
				var value = Gh7559Generic<Gh7559Enum>.GetIcon(layout);
				Assert.That(value, Is.EqualTo(Gh7559Enum.LetterA));
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

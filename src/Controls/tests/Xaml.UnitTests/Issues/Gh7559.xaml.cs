// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7559 : ContentPage
{
	public Gh7559() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void GenericBPCompiles([Values] XamlInflator inflator)
		{
			var layout = new Gh7559(inflator);
			var value = Gh7559Generic<Gh7559Enum>.GetIcon(layout);
			Assert.That(value, Is.EqualTo(Gh7559Enum.LetterA));
		}
	}
}

public abstract class Gh7559Generic<T>
{
	public static readonly BindableProperty IconProperty = BindableProperty.Create("Icon", typeof(T), typeof(Gh7559Generic<T>), default(T));

	public static T GetIcon(BindableObject bindable) => (T)bindable.GetValue(IconProperty);
	public static void SetIcon(BindableObject bindable, T value) => bindable.SetValue(IconProperty, value);
}

public enum Gh7559Enum
{
	LetterX = 'X',
	LetterA = 'A',
}

public class Gh7559A : Gh7559Generic<Gh7559Enum>
{
}

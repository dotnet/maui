// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7559 : ContentPage
{
	public Gh7559() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void GenericBPCompiles(XamlInflator inflator)
		{
			var layout = new Gh7559(inflator);
			var value = Gh7559Generic<Gh7559Enum>.GetIcon(layout);
			Assert.Equal(Gh7559Enum.LetterA, value);
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

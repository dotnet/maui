// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Reproduce MauiIcons pattern: enum for icon selection
public enum TestIcons
{
	None,
	Home,
	Settings,
	AccessAlarm
}

// Simulate MauiIcons BaseIcon class
public abstract class BaseTestIcon : BindableObject
{
	public static readonly BindableProperty IconColorProperty = 
		BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(BaseTestIcon), null);

	public Color IconColor
	{
		get => (Color)GetValue(IconColorProperty);
		set => SetValue(IconColorProperty, value);
	}
}

// Simulate MauiIcons generic BaseIconExtension<TEnum>
[ContentProperty(nameof(Icon))]
[AcceptEmptyServiceProvider]
public abstract class BaseTestIconExtension<TEnum> : BaseTestIcon, IMarkupExtension<BindingBase> where TEnum : Enum
{
	public static readonly BindableProperty IconExtensionProperty = 
		BindableProperty.Create(nameof(Icon), typeof(TEnum), typeof(BaseTestIconExtension<TEnum>), default(TEnum));

	public TEnum Icon
	{
		get => (TEnum)GetValue(IconExtensionProperty);
		set => SetValue(IconExtensionProperty, value);
	}

	public BindingBase ProvideValue(IServiceProvider serviceProvider)
	{
		// Return a binding that would produce a FontImageSource
		return new Binding 
		{ 
			Source = new FontImageSource 
			{ 
				Glyph = Icon?.ToString() ?? "",
				FontFamily = "TestFont",
				Size = 24
			},
			Path = "."
		};
	}

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) 
		=> ProvideValue(serviceProvider);
}

// Concrete implementation like MaterialOutlinedExtension
public class TestIconExtension : BaseTestIconExtension<TestIcons>
{
}

public partial class Maui32658 : ContentPage
{
	public Maui32658() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void MarkupExtensionWithGenericBaseClassAndEnumProperty(XamlInflator inflator)
		{
			// This test reproduces issue #32658
			// The bug was an IndexOutOfRangeException when parsing markup extensions
			// with a generic abstract base class pattern (like MauiIcons)
			var page = new Maui32658(inflator);
			
			// Just verifying the page loads without throwing is sufficient
			// The original bug would throw "Index was outside the bounds of the array"
			// during XAML parsing when using generic markup extensions
			Assert.NotNull(page);
			Assert.NotNull(page.imageButton);
		}
	}
}

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz48554View
{
	static List<string> all;

	public static List<string> All
	{
		get
		{
			if (all == null)
			{
				all = new List<string>();
				all.Add("6b+");
				all.Add("6c");
				all.Add("6c+");
				all.Add("7a");
				all.Add("7a+");
			}
			return all;
		}
		set
		{
			all = value;
		}
	}
}

public class Bz48554Slider : View
{
	public static readonly BindableProperty ValuesProperty =
		BindableProperty.Create(nameof(Values), typeof(List<string>), typeof(Bz48554Slider), null);

	public List<string> Values
	{
		get { return (List<string>)GetValue(ValuesProperty); }
		set { SetValue(ValuesProperty, value); }
	}
}

public partial class Bz48554 : ContentPage
{
	public Bz48554()
	{
		InitializeComponent();
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void XStaticWithXamlC(XamlInflator inflator)
		{
			Bz48554 page = null;
			Assert.Null(Record.Exception(() => page = new Bz48554(inflator)));
			Assert.NotNull(page.SliderGrades);
			Assert.Equal(5, page.SliderGrades.Values.Count);
		}
	}
}
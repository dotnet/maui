using System;
using System.Collections.Generic;
using System.Windows.Input;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
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

		public Bz48554(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void XStaticWithXamlC(bool useCompiledXaml)
			{
				Bz48554 page = null;
				Assert.DoesNotThrow(() => page = new Bz48554(useCompiledXaml));
				Assert.NotNull(page.SliderGrades);
				Assert.AreEqual(5, page.SliderGrades.Values.Count);
			}
		}
	}
}
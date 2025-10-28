using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public static class Gh2130Behavior
	{
		public static readonly BindableProperty AppearingProperty =
			BindableProperty.Create("Appearing", typeof(bool), typeof(Gh2130Behavior), default(bool));

		public static bool GetAppearing(BindableObject bindable)
		{
			return (bool)bindable.GetValue(AppearingProperty);
		}

		public static void SetAppearing(BindableObject bindable, bool value)
		{
			bindable.SetValue(AppearingProperty, value);
		}

	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh2130 : ContentPage
	{
		public Gh2130()
		{
			InitializeComponent();
		}

		public Gh2130(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(false), TestCase(true)]
			public void AttachedBPWithEventName(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Gh2130));
				new Gh2130(useCompiledXaml);
			}
		}
	}
}

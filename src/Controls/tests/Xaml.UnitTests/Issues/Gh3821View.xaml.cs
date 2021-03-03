using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh3821View : ContentView
	{
		public Gh3821View()
		{
			InitializeComponent();
		}

		public Gh3821View(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public static readonly BindableProperty TextProperty =
			BindableProperty.Create("Text", typeof(string), typeof(Gh3821View), default(string));

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
	}
}

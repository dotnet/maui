using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz29300DummyView : StackLayout
	{
		public static readonly BindableProperty NumOfRepeatProperty =
			BindableProperty.Create(nameof(NumOfRepeat), typeof(int), typeof(Bz29300DummyView), 1, BindingMode.OneWay, null, UpdateNumOfRepeats);

		public static readonly BindableProperty TextProperty =
			BindableProperty.Create(nameof(Text), typeof(string), typeof(Bz29300DummyView), string.Empty, BindingMode.OneWay, null, UpdateTexts);

		public int NumOfRepeat
		{
			get { return (int)GetValue(NumOfRepeatProperty); }
			set { SetValue(NumOfRepeatProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public Bz29300DummyView()
		{
		}

		static void UpdateTexts(BindableObject bindable, object oldValueObj, object newValueObj)
		{
			var oldValue = (string)oldValueObj;
			var newValue = (string)newValueObj;

			var instance = bindable as Bz29300DummyView;
			instance.Children.Clear();
			for (int i = 0; i < instance.NumOfRepeat; i++)
				instance.Children.Add(new Label() { Text = newValue });
		}

		static void UpdateNumOfRepeats(BindableObject bindable, object oldValueObj, object newValueObj)
		{
			var oldValue = (int)oldValueObj;
			var newValue = (int)newValueObj;

			var instance = bindable as Bz29300DummyView;
			if (oldValue == newValue)
				return;
			if (oldValue > newValue)
			{
				for (int i = newValue; i > oldValue; i--)
					instance.Children.RemoveAt(0);
			}
			else
			{
				for (int i = oldValue; i < newValue; i++)
					instance.Children.Add(new Label() { Text = instance.Text });
			}
		}
	}

	public partial class Bz29300 : ContentPage
	{
		public Bz29300()
		{
			InitializeComponent();
		}

		public Bz29300(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void AccessUserDefinedBindableProperties(bool useCompiledXaml)
			{
				var layout = new Bz29300(useCompiledXaml);
				Assert.Equal(4, layout.dummy.NumOfRepeat);
				Assert.Equal("Test", layout.dummy.Text);
			}
		}
	}
}
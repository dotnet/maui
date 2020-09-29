using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Bz29300DummyView : StackLayout
	{
		public static readonly BindableProperty NumOfRepeatProperty =
#pragma warning disable 618
			BindableProperty.Create<Bz29300DummyView, int>(p => p.NumOfRepeat, 1, BindingMode.OneWay, null, UpdateTexts);
#pragma warning restore 618

		public static readonly BindableProperty TextProperty =
#pragma warning disable 618
			BindableProperty.Create<Bz29300DummyView, string>(p => p.Text, string.Empty, BindingMode.OneWay, null, UpdateTexts);
#pragma warning restore 618

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

		static void UpdateTexts(BindableObject bindable, string oldValue, string newValue)
		{
			var instance = bindable as Bz29300DummyView;
			instance.Children.Clear();
			for (int i = 0; i < instance.NumOfRepeat; i++)
				instance.Children.Add(new Label() { Text = newValue });
		}

		static void UpdateTexts(BindableObject bindable, int oldValue, int newValue)
		{
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
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void AccessUserDefinedBindableProperties(bool useCompiledXaml)
			{
				var layout = new Bz29300(useCompiledXaml);
				Assert.AreEqual(4, layout.dummy.NumOfRepeat);
				Assert.AreEqual("Test", layout.dummy.Text);
			}
		}
	}
}
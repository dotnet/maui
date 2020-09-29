using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	//[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh3821 : ContentPage
	{
		public Gh3821()
		{
			InitializeComponent();
		}

		public Gh3821(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		string _text;
		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				OnPropertyChanged();
			}
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(true), TestCase(false)]
			public void NoConflictsInNamescopes(bool useCompiledXaml)
			{
				var layout = new Gh3821(useCompiledXaml) { Text = "root" };
				var view = ((Gh3821View)((StackLayout)layout.Content).Children[0]);
				var label0 = ((Label)((Gh3821View)((StackLayout)layout.Content).Children[0]).Content);
				Assert.That(label0.Text, Is.EqualTo("root"));
			}
		}
	}
}
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using NUnit.Framework;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class BindingsCompiler : ContentPage
	{
		public BindingsCompiler()
		{
			InitializeComponent();
		}

		public BindingsCompiler(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
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

			[TestCase(false)]
			[TestCase(true)]
			public void Test(bool useCompiledXaml)
			{
				var vm = new MockViewModel {
					Text = "Text0",
					Model = new MockViewModel {
						Text = "Text1"
					},
				};
				vm.Model [3] = "TextIndex";

				var layout = new BindingsCompiler(useCompiledXaml);
				layout.BindingContext = vm;
				//testing paths
				Assert.AreEqual("Text0", layout.label0.Text);
				Assert.AreEqual("Text0", layout.label1.Text);
				Assert.AreEqual("Text1", layout.label2.Text);
				Assert.AreEqual("TextIndex", layout.label3.Text);

				//testing selfPath
				layout.label4.BindingContext = "Self";
				Assert.AreEqual("Self", layout.label4.Text);

				//testing INPC
				vm.Text = "Text2";
				Assert.AreEqual("Text2", layout.label0.Text);

				//testing 2way
				Assert.AreEqual("Text2", layout.entry0.Text);
				((IElementController)layout.entry0).SetValueFromRenderer(Entry.TextProperty, "Text3");
				Assert.AreEqual("Text3", layout.entry0.Text);

				//testing invalid bindingcontext type
				layout.BindingContext = new object();
				Assert.AreEqual(null, layout.label0.Text);
			}
		}
	}

	class MockViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public MockViewModel(string text = null)
		{
			_text = text;
		}

		string _text;
		public string Text {
			get { return _text; }
			set {
				if (_text == value)
					return;

				_text = value;
				OnPropertyChanged();
			}
		}

		MockViewModel _model;
		public MockViewModel Model {
			get { return _model; }
			set {
				if (_model == value)
					return;
				_model = value;
				OnPropertyChanged();
			}
		}

		string [] values = new string [5];
		[IndexerName("Indexer")]
		public string this [int v] {
			get { return values [v]; }
			set {
				if (values [v] == value)
					return;

				values [v] = value;
				OnPropertyChanged("Indexer[" + v + "]");
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
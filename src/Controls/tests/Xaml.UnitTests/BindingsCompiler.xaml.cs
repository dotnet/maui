using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[TestCase(false)]
			[TestCase(true)]
			public void Test(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(BindingsCompiler));

				var vm = new MockViewModel
				{
					Text = "Text0",
					I = 42,
					Model = new MockViewModel
					{
						Text = "Text1"
					},
					StructModel = new MockStructViewModel
					{
						Model = new MockViewModel
						{
							Text = "Text9"
						}
					}
				};
				vm.Model[3] = "TextIndex";

				var layout = new BindingsCompiler(useCompiledXaml)
				{
					BindingContext = vm
				};
				layout.label6.BindingContext = new MockStructViewModel
				{
					Model = new MockViewModel
					{
						Text = "text6"
					}
				};

				//testing paths
				Assert.AreEqual("Text0", layout.label0.Text);
				Assert.AreEqual("Text0", layout.label1.Text);
				Assert.AreEqual("Text1", layout.label2.Text);
				Assert.AreEqual("TextIndex", layout.label3.Text);
				Assert.AreEqual("Text0", layout.label8.Text);

				//value types
				Assert.That(layout.label5.Text, Is.EqualTo("42"));
				Assert.That(layout.label6.Text, Is.EqualTo("text6"));
				Assert.AreEqual("Text9", layout.label9.Text);
				Assert.AreEqual("Text9", layout.label10.Text);
				layout.label9.Text = "Text from label9";
				Assert.AreEqual("Text from label9", vm.StructModel.Text);
				layout.label10.Text = "Text from label10";
				Assert.AreEqual("Text from label10", vm.StructModel.Model.Text);

				//testing selfPath
				layout.label4.BindingContext = "Self";
				Assert.AreEqual("Self", layout.label4.Text);
				layout.label7.BindingContext = 42;
				Assert.That(layout.label7.Text, Is.EqualTo("42"));

				//testing INPC
				GC.Collect();
				vm.Text = "Text2";
				Assert.AreEqual("Text2", layout.label0.Text);

				//https://github.com/dotnet/maui/issues/21181
				vm.Model[3] = "TextIndex2";
				Assert.AreEqual("TextIndex2", layout.label3.Text);

				//testing 2way
				Assert.AreEqual("Text2", layout.entry0.Text);
				((IElementController)layout.entry0).SetValueFromRenderer(Entry.TextProperty, "Text3");
				Assert.AreEqual("Text3", layout.entry0.Text);
				Assert.AreEqual("Text3", vm.Text);
				((IElementController)layout.entry1).SetValueFromRenderer(Entry.TextProperty, "Text4");
				Assert.AreEqual("Text4", layout.entry1.Text);
				Assert.AreEqual("Text4", vm.Model.Text);
				vm.Model = null;
				layout.entry1.BindingContext = null;

				//testing invalid bindingcontext type
				layout.BindingContext = new object();
				Assert.AreEqual(null, layout.label0.Text);
			}
		}
	}

	struct MockStructViewModel
	{
		public string Text
		{
			get { return Model.Text; }
			set { Model.Text = value; }
		}
		public int I { get; set; }
		public MockViewModel Model { get; set; }
	}

	class MockViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public MockViewModel(string text = null, int i = -1)
		{
			_text = text;
			_i = i;
		}

		string _text;
		public string Text
		{
			get { return _text; }
			set
			{
				if (_text == value)
					return;

				_text = value;
				OnPropertyChanged();
			}
		}

		int _i;
		public int I
		{
			get { return _i; }
			set
			{
				if (_i == value)
					return;
				_i = value;
				OnPropertyChanged();
			}
		}

		MockViewModel _model;
		public MockViewModel Model
		{
			get { return _model; }
			set
			{
				if (_model == value)
					return;
				_model = value;
				OnPropertyChanged();
			}
		}

		MockStructViewModel _structModel;
		public MockStructViewModel StructModel
		{
			get { return _structModel; }
			set
			{
				_structModel = value;
				OnPropertyChanged();
			}
		}

		string[] values = new string[5];
		[IndexerName("Indexer")]
		public string this[int v]
		{
			get { return values[v]; }
			set
			{
				if (values[v] == value)
					return;

				values[v] = value;
				OnPropertyChanged("Indexer[" + v + "]");
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
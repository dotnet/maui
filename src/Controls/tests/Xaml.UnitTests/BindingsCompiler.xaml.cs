using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

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
		public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			[InlineData(false)]
			[InlineData(true)]
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
					BindingContext = new GlobalViewModel(),
				};
				layout.stack.BindingContext = vm;
				layout.label6.BindingContext = new MockStructViewModel
				{
					Model = new MockViewModel
					{
						Text = "text6"
					}
				};

				//testing paths
				Assert.Equal("Text0", layout.label0.Text);
				Assert.Equal("Text0", layout.label1.Text);
				Assert.Equal("Text1", layout.label2.Text);
				Assert.Equal("TextIndex", layout.label3.Text);
				Assert.Equal("Text0", layout.label8.Text);

				//value types
				Assert.Equal("42", layout.label5.Text);
				Assert.Equal("text6", layout.label6.Text);
				Assert.Equal("Text9", layout.label9.Text);
				Assert.Equal("Text9", layout.label10.Text);
				layout.label9.Text = "Text from label9";
				Assert.Equal("Text from label9", vm.StructModel.Text);
				layout.label10.Text = "Text from label10";
				Assert.Equal("Text from label10", vm.StructModel.Model.Text);

				//testing selfPath
				layout.label4.BindingContext = "Self";
				Assert.Equal("Self", layout.label4.Text);
				layout.label7.BindingContext = 42;
				Assert.Equal("42", layout.label7.Text);

				//testing INPC
				GC.Collect();
				vm.Text = "Text2";
				Assert.Equal("Text2", layout.label0.Text);

				//https://github.com/dotnet/maui/issues/21181
				vm.Model[3] = "TextIndex2";
				Assert.Equal("TextIndex2", layout.label3.Text);

				//https://github.com/dotnet/maui/issues/23621
				vm.Model.SetIndexerValueAndCallOnPropertyChangedWithoutIndex(3, "TextIndex3");
				Assert.Equal("TextIndex3", layout.label3.Text);

				//testing 2way
				Assert.Equal("Text2", layout.entry0.Text);
				((IElementController)layout.entry0).SetValueFromRenderer(Entry.TextProperty, "Text3");
				Assert.Equal("Text3", layout.entry0.Text);
				Assert.Equal("Text3", vm.Text);
				((IElementController)layout.entry1).SetValueFromRenderer(Entry.TextProperty, "Text4");
				Assert.Equal("Text4", layout.entry1.Text);
				Assert.Equal("Text4", vm.Model.Text);
				vm.Model = null;
				layout.entry1.BindingContext = null;

				//testing standalone bindings
				if (useCompiledXaml)
				{
					var binding = layout.picker0.ItemDisplayBinding;
					Assert.IsType<TypedBinding<MockItemViewModel, string>>(binding);
				}

				//testing invalid bindingcontext type
				layout.stack.BindingContext = new object();
				Assert.Null(layout.label0.Text);

				//testing source
				Assert.Equal("Text for label12", layout.label12.Text);

				//testing binding with path that cannot be statically compiled (we don't support casts in the Path)
				Assert.Equal("Global Text", layout.label13.Text);
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

	class GlobalViewModel
	{
		public string GlobalText { get; set; } = "Global Text";
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

		MockItemViewModel[] _items;
		public MockItemViewModel[] Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void SetIndexerValueAndCallOnPropertyChangedWithoutIndex(int v, string value)
		{
			if (values[v] == value)
				return;

			values[v] = value;
			OnPropertyChanged("Indexer");
		}
	}

	class MockItemViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private string _title { get; set; }
		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				OnPropertyChanged();
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
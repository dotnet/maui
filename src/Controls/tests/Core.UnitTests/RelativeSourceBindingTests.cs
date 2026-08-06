using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class RelativeSourceBindingTests : BaseTestFixture
	{
		[Fact]
		public void RelativeSourceSelfBinding()
		{
			Label label = new Label
			{
				StyleId = "label1"
			};
			label.SetBinding(Label.TextProperty, new Binding(nameof(Label.StyleId))
			{
				Source = RelativeBindingSource.Self
			});
			Assert.Equal(label.Text, label.StyleId);
		}

		[Fact]
		public void RelativeSourceSelfBinding_TypedBinding()
		{
			Label label = new Label
			{
				StyleId = "label1"
			};
			label.SetBinding(Label.TextProperty, static (Label label) => label.StyleId, source: RelativeBindingSource.Self);
			Assert.Equal(label.Text, label.StyleId);

			label.StyleId = "label2";
			Assert.Equal("label2", label.Text);
		}

		[Fact]
		public void RelativeSourceBinding_FindAncestor()
		{
			var stack = new StackLayout
			{
				StyleId = "stack1"
			};
			var label = new Label();
			stack.Children.Add(label);

			label.SetBinding(Label.TextProperty, new Binding("StyleId", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(StackLayout), 1)));
			Assert.Equal("stack1", label.Text);

			stack.StyleId = "stack2";
			Assert.Equal("stack2", label.Text);
		}

		[Fact]
		public void RelativeSourceBinding_FindAncestor_TypedBinding()
		{
			var stack = new StackLayout
			{
				StyleId = "stack1"
			};
			var label = new Label();
			stack.Children.Add(label);

			label.SetBinding(Label.TextProperty, static (StackLayout stack) => stack.StyleId, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(StackLayout), 1));
			Assert.Equal("stack1", label.Text);

			stack.StyleId = "stack2";
			Assert.Equal("stack2", label.Text);
		}

		[Fact]
		public void RelativeSourceBinding_TemplatedParent()
		{
			Label label = null;
			var template = new ControlTemplate(() =>
			{
				label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("StyleId", source: RelativeBindingSource.TemplatedParent));
				return label;
			});

			var frame = new Frame
			{
				StyleId = "frame1",
				ControlTemplate = template
			};
			Assert.Equal("frame1", label?.Text);

			frame.StyleId = "frame2";
			Assert.Equal("frame2", label?.Text);
		}

		[Fact]
		public void RelativeSourceBinding_TemplatedParent_TypedBinding()
		{
			Label label = null;
			var template = new ControlTemplate(() =>
			{
				label = new Label();
				label.SetBinding(Label.TextProperty, static (Frame frame) => frame.StyleId, source: RelativeBindingSource.TemplatedParent);
				return label;
			});

			var frame = new Frame
			{
				StyleId = "frame1",
				ControlTemplate = template
			};
			Assert.Equal("frame1", label?.Text);

			frame.StyleId = "frame2";
			Assert.Equal("frame2", label?.Text);
		}

		[Fact]
		public void RelativeSourceAncestorTypeBinding()
		{
			Grid grid = new Grid();

			StackLayout stack0 = new StackLayout
			{
				BackgroundColor = Colors.Red
			};
			StackLayout stack1 = new StackLayout
			{
				BackgroundColor = Colors.Green
			};
			StackLayout stack2 = new StackLayout
			{
				BackgroundColor = Colors.Blue
			};

			Label label0 = new Label();
			Label label1 = new Label();
			Label label2 = new Label();
			var person0 = new PersonViewModel
			{
				Name = "Person 0",
			};
			var person1 = new PersonViewModel
			{
				Name = "Person 1",
			};
			var person2 = new PersonViewModel
			{
				Name = "Person 2",
			};

			stack2.Children.Add(stack1);
			stack1.Children.Add(stack0);
			stack0.Children.Add(grid);

			label0.SetBinding(Label.TextProperty, new Binding
			{
				Path = nameof(PersonViewModel.Name),
				Source = new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(PersonViewModel), 1)
			});
			label0.SetBinding(Label.TextColorProperty, new Binding
			{
				Path = nameof(StackLayout.BackgroundColor),
				Source = new RelativeBindingSource(
					RelativeBindingSourceMode.FindAncestor, typeof(StackLayout), 1)
			});
			Assert.Null(label0.Text);
			Assert.Equal(Label.TextColorProperty.DefaultValue, label0.TextColor);

			label1.SetBinding(Label.TextProperty, new Binding
			{
				Path = nameof(PersonViewModel.Name),
				Source = new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(PersonViewModel), 2)
			});
			label1.SetBinding(Label.TextColorProperty, new Binding
			{
				Path = nameof(StackLayout.BackgroundColor),
				Source = new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(StackLayout), 2)
			});
			Assert.Null(label1.Text);
			Assert.Equal(Label.TextColorProperty.DefaultValue, label1.TextColor);

			label2.SetBinding(Label.TextProperty, new Binding
			{
				Path = nameof(PersonViewModel.Name),
				Source = new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(PersonViewModel), 3)
			});
			label2.SetBinding(Label.TextColorProperty, new Binding
			{
				Path = nameof(StackLayout.BackgroundColor),
				Source = new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(StackLayout), 3)
			});
			Assert.Null(label2.Text);
			Assert.Equal(Label.TextColorProperty.DefaultValue, label2.TextColor);

			grid.Children.Add(label0);
			grid.Children.Add(label1);
			grid.Children.Add(label2);

			//// BindingContext changes

			// stack2
			//		stack1
			//			stack0
			//				grid
			//					label0 (min level 1)	= stack0/null
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Null(label0.Text);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			// stack2	(person2)
			//		stack1 (person2*)
			//			stack0 (person2*)
			//				grid (person2*)
			//					label0 (min level 1)	= stack0/person2
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			stack2.BindingContext = person2;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person2.Name);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			// stack2	(person2)
			//		stack1 (person1)
			//			stack0 (person1*)
			//				grid (person1*)
			//					label0 (min level 1)	= stack0/person1
			//					label1 (min level 2)	= stack1/person2
			//					label2 (min level 3)	= stack2/null
			stack1.BindingContext = person1;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person1.Name);
			Assert.Equal(label1.Text, person2.Name);
			Assert.Equal(Label.TextProperty.DefaultValue, label2.Text);

			// stack2	(person2)
			//		stack1 (person1)
			//			stack0 (person0)
			//				grid (person0*)
			//					label0 (min level 1)	= stack0/person0
			//					label1 (min level 2)	= stack1/person1
			//					label2 (min level 3)	= stack2/person2
			stack0.BindingContext = person0;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Equal(label1.Text, person1.Name);
			Assert.Equal(label2.Text, person2.Name);

			// stack2	
			//		stack1 (person1)
			//			stack0 (person0)
			//				grid (person0*)
			//					label0 (min level 1)	= stack0/person0
			//					label1 (min level 2)	= stack1/person1
			//					label2 (min level 3)	= stack2/null
			stack2.BindingContext = null;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Equal(label1.Text, person1.Name);
			Assert.Null(label2.Text);

			// stack2
			//		stack1
			//			stack0 (person0)
			//				grid (person0*)
			//					label0 (min level 1)	= stack0/person0
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			stack1.BindingContext = null;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			// stack2
			//		stack1
			//			stack0
			//					label0 (min level 1)	= stack0/null
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			stack0.BindingContext = null;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Null(label0.Text);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			stack0.BindingContext = person0;
			stack1.BindingContext = person1;
			stack2.BindingContext = person2;

			//// Parent Changes

			// stack2 (person2) 
			// stack1 (person1)
			//		stack0 (person0)
			//				label0 (min level 1)	= stack0/person0
			//				label1 (min level 2)	= stack1/person1
			//				label2 (min level 3)	= null/null
			stack2.Children.Clear();
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Equal(label1.Text, person1.Name);
			Assert.Null(label2.Text);

			// stack2 (person2) 
			// stack1 (person1) 
			// stack0 (person0)
			//			label0 (min level 1)	= stack0/person0
			//			label1 (min level 2)	= null/null
			//			label2 (min level 3)	= null/null
			stack1.Children.Clear();
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label2.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			// stack2 (person2) 
			// stack1 (person1) 
			// stack0 (person0)
			// grid
			//		label0 (min level 1)	= null/null
			//		label1 (min level 2)	= null/null
			//		label2 (min level 3)	= null/null
			stack0.Children.Clear();
			Assert.Equal(label0.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label1.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label2.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Null(label0.Text);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);
		}

		[Fact]
		public void RelativeSourceAncestorTypeBinding_TypedBinding()
		{
			Grid grid = new Grid();

			StackLayout stack0 = new StackLayout
			{
				BackgroundColor = Colors.Red
			};
			StackLayout stack1 = new StackLayout
			{
				BackgroundColor = Colors.Green
			};
			StackLayout stack2 = new StackLayout
			{
				BackgroundColor = Colors.Blue
			};

			Label label0 = new Label();
			Label label1 = new Label();
			Label label2 = new Label();
			var person0 = new PersonViewModel
			{
				Name = "Person 0",
			};
			var person1 = new PersonViewModel
			{
				Name = "Person 1",
			};
			var person2 = new PersonViewModel
			{
				Name = "Person 2",
			};

			stack2.Children.Add(stack1);
			stack1.Children.Add(stack0);
			stack0.Children.Add(grid);

			label0.SetBinding(Label.TextProperty, static (PersonViewModel vm) => vm.Name, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(PersonViewModel), 1));
			label0.SetBinding(Label.TextColorProperty, static (StackLayout stack) => stack.BackgroundColor, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(StackLayout), 1));
			Assert.Null(label0.Text);
			Assert.Equal(Label.TextColorProperty.DefaultValue, label0.TextColor);

			label1.SetBinding(Label.TextProperty, static (PersonViewModel vm) => vm.Name, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(PersonViewModel), 2));
			label1.SetBinding(Label.TextColorProperty, static (StackLayout stack) => stack.BackgroundColor, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(StackLayout), 2));
			Assert.Null(label1.Text);
			Assert.Equal(Label.TextColorProperty.DefaultValue, label1.TextColor);

			label2.SetBinding(Label.TextProperty, static (PersonViewModel vm) => vm.Name, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(PersonViewModel), 3));
			label2.SetBinding(Label.TextColorProperty, static (StackLayout vm) => vm.BackgroundColor, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(StackLayout), 3));
			Assert.Null(label2.Text);
			Assert.Equal(Label.TextColorProperty.DefaultValue, label2.TextColor);

			grid.Children.Add(label0);
			grid.Children.Add(label1);
			grid.Children.Add(label2);

			//// BindingContext changes

			// stack2
			//		stack1
			//			stack0
			//				grid
			//					label0 (min level 1)	= stack0/null
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Null(label0.Text);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			// stack2	(person2)
			//		stack1 (person2*)
			//			stack0 (person2*)
			//				grid (person2*)
			//					label0 (min level 1)	= stack0/person2
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			stack2.BindingContext = person2;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person2.Name);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			// stack2	(person2)
			//		stack1 (person1)
			//			stack0 (person1*)
			//				grid (person1*)
			//					label0 (min level 1)	= stack0/person1
			//					label1 (min level 2)	= stack1/person2
			//					label2 (min level 3)	= stack2/null
			stack1.BindingContext = person1;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person1.Name);
			Assert.Equal(label1.Text, person2.Name);
			Assert.Equal(Label.TextProperty.DefaultValue, label2.Text);

			// stack2	(person2)
			//		stack1 (person1)
			//			stack0 (person0)
			//				grid (person0*)
			//					label0 (min level 1)	= stack0/person0
			//					label1 (min level 2)	= stack1/person1
			//					label2 (min level 3)	= stack2/person2
			stack0.BindingContext = person0;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Equal(label1.Text, person1.Name);
			Assert.Equal(label2.Text, person2.Name);

			// stack2	
			//		stack1 (person1)
			//			stack0 (person0)
			//				grid (person0*)
			//					label0 (min level 1)	= stack0/person0
			//					label1 (min level 2)	= stack1/person1
			//					label2 (min level 3)	= stack2/null
			stack2.BindingContext = null;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Equal(label1.Text, person1.Name);
			Assert.Null(label2.Text);

			// stack2
			//		stack1
			//			stack0 (person0)
			//				grid (person0*)
			//					label0 (min level 1)	= stack0/person0
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			stack1.BindingContext = null;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			// stack2
			//		stack1
			//			stack0
			//					label0 (min level 1)	= stack0/null
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			stack0.BindingContext = null;
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, stack2.BackgroundColor);
			Assert.Null(label0.Text);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			stack0.BindingContext = person0;
			stack1.BindingContext = person1;
			stack2.BindingContext = person2;

			//// Parent Changes

			// stack2 (person2) 
			// stack1 (person1)
			//		stack0 (person0)
			//				label0 (min level 1)	= stack0/person0
			//				label1 (min level 2)	= stack1/person1
			//				label2 (min level 3)	= null/null
			stack2.Children.Clear();
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, stack1.BackgroundColor);
			Assert.Equal(label2.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Equal(label1.Text, person1.Name);
			Assert.Null(label2.Text);

			// stack2 (person2) 
			// stack1 (person1) 
			// stack0 (person0)
			//			label0 (min level 1)	= stack0/person0
			//			label1 (min level 2)	= null/null
			//			label2 (min level 3)	= null/null
			stack1.Children.Clear();
			Assert.Equal(label0.TextColor, stack0.BackgroundColor);
			Assert.Equal(label1.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label2.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label0.Text, person0.Name);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);

			// stack2 (person2) 
			// stack1 (person1) 
			// stack0 (person0)
			// grid
			//		label0 (min level 1)	= null/null
			//		label1 (min level 2)	= null/null
			//		label2 (min level 3)	= null/null
			stack0.Children.Clear();
			Assert.Equal(label0.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label1.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Equal(label2.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.Null(label0.Text);
			Assert.Null(label1.Text);
			Assert.Null(label2.Text);
		}

		internal class PersonViewModel
		{
			public string Name { get; set; }
		}
	}

	public class CustomControl : ContentView
	{
		StackLayout _layout = new StackLayout();
		public CustomControl()
		{
		}

		#region string CustomText bindable property
		public static BindableProperty CustomTextProperty = BindableProperty.Create(
			"CustomText",
			typeof(string),
			typeof(CustomControl),
			null);
		public string CustomText
		{
			get
			{
				return (string)GetValue(CustomTextProperty);
			}
			set
			{
				SetValue(CustomTextProperty, value);
			}
		}
		#endregion
	}

	public class MyCustomControlTemplate : StackLayout
	{
		Label _label = new Label();

		public MyCustomControlTemplate()
		{
			this.Children.Add(_label);

			_label.SetBinding(
				Label.TextProperty,
				new Binding(
					nameof(CustomControl.CustomText),
					source: RelativeBindingSource.TemplatedParent));
		}
	}
}

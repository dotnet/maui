using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class RelativeSourceBindingTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
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
			Assert.AreEqual(label.Text, label.StyleId);
		}

		[Test]
		public void RelativeSourceAncestorTypeBinding()
		{
			Grid grid = new Grid();

			StackLayout stack0 = new StackLayout
			{
				BackgroundColor = Color.Red
			};
			StackLayout stack1 = new StackLayout
			{
				BackgroundColor = Color.Green
			};
			StackLayout stack2 = new StackLayout
			{
				BackgroundColor = Color.Blue
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
			Assert.IsNull(label0.Text);
			Assert.AreEqual(Label.TextColorProperty.DefaultValue, label0.TextColor);

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
			Assert.IsNull(label1.Text);
			Assert.AreEqual(Label.TextColorProperty.DefaultValue, label1.TextColor);

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
			Assert.IsNull(label2.Text);
			Assert.AreEqual(Label.TextColorProperty.DefaultValue, label2.TextColor);

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
			Assert.AreEqual(label0.TextColor, stack0.BackgroundColor);
			Assert.AreEqual(label1.TextColor, stack1.BackgroundColor);
			Assert.AreEqual(label2.TextColor, stack2.BackgroundColor);
			Assert.IsNull(label0.Text);
			Assert.IsNull(label1.Text);
			Assert.IsNull(label2.Text);

			// stack2	(person2)
			//		stack1 (person2*)
			//			stack0 (person2*)
			//				grid (person2*)
			//					label0 (min level 1)	= stack0/person2
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			stack2.BindingContext = person2;
			Assert.AreEqual(label0.TextColor, stack0.BackgroundColor);
			Assert.AreEqual(label1.TextColor, stack1.BackgroundColor);
			Assert.AreEqual(label2.TextColor, stack2.BackgroundColor);
			Assert.AreEqual(label0.Text, person2.Name);
			Assert.IsNull(label1.Text);
			Assert.IsNull(label2.Text);

			// stack2	(person2)
			//		stack1 (person1)
			//			stack0 (person1*)
			//				grid (person1*)
			//					label0 (min level 1)	= stack0/person1
			//					label1 (min level 2)	= stack1/person2
			//					label2 (min level 3)	= stack2/null
			stack1.BindingContext = person1;
			Assert.AreEqual(label0.TextColor, stack0.BackgroundColor);
			Assert.AreEqual(label1.TextColor, stack1.BackgroundColor);
			Assert.AreEqual(label2.TextColor, stack2.BackgroundColor);
			Assert.AreEqual(label0.Text, person1.Name);
			Assert.AreEqual(label1.Text, person2.Name);
			Assert.AreEqual(Label.TextProperty.DefaultValue, label2.Text);

			// stack2	(person2)
			//		stack1 (person1)
			//			stack0 (person0)
			//				grid (person0*)
			//					label0 (min level 1)	= stack0/person0
			//					label1 (min level 2)	= stack1/person1
			//					label2 (min level 3)	= stack2/person2
			stack0.BindingContext = person0;
			Assert.AreEqual(label0.TextColor, stack0.BackgroundColor);
			Assert.AreEqual(label1.TextColor, stack1.BackgroundColor);
			Assert.AreEqual(label2.TextColor, stack2.BackgroundColor);
			Assert.AreEqual(label0.Text, person0.Name);
			Assert.AreEqual(label1.Text, person1.Name);
			Assert.AreEqual(label2.Text, person2.Name);

			// stack2	
			//		stack1 (person1)
			//			stack0 (person0)
			//				grid (person0*)
			//					label0 (min level 1)	= stack0/person0
			//					label1 (min level 2)	= stack1/person1
			//					label2 (min level 3)	= stack2/null
			stack2.BindingContext = null;
			Assert.AreEqual(label0.TextColor, stack0.BackgroundColor);
			Assert.AreEqual(label1.TextColor, stack1.BackgroundColor);
			Assert.AreEqual(label2.TextColor, stack2.BackgroundColor);
			Assert.AreEqual(label0.Text, person0.Name);
			Assert.AreEqual(label1.Text, person1.Name);
			Assert.IsNull(label2.Text);

			// stack2
			//		stack1
			//			stack0 (person0)
			//				grid (person0*)
			//					label0 (min level 1)	= stack0/person0
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			stack1.BindingContext = null;
			Assert.AreEqual(label0.TextColor, stack0.BackgroundColor);
			Assert.AreEqual(label1.TextColor, stack1.BackgroundColor);
			Assert.AreEqual(label2.TextColor, stack2.BackgroundColor);
			Assert.AreEqual(label0.Text, person0.Name);
			Assert.IsNull(label1.Text);
			Assert.IsNull(label2.Text);

			// stack2
			//		stack1
			//			stack0
			//					label0 (min level 1)	= stack0/null
			//					label1 (min level 2)	= stack1/null
			//					label2 (min level 3)	= stack2/null
			stack0.BindingContext = null;
			Assert.AreEqual(label0.TextColor, stack0.BackgroundColor);
			Assert.AreEqual(label1.TextColor, stack1.BackgroundColor);
			Assert.AreEqual(label2.TextColor, stack2.BackgroundColor);
			Assert.IsNull(label0.Text);
			Assert.IsNull(label1.Text);
			Assert.IsNull(label2.Text);

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
			Assert.AreEqual(label0.TextColor, stack0.BackgroundColor);
			Assert.AreEqual(label1.TextColor, stack1.BackgroundColor);
			Assert.AreEqual(label2.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.AreEqual(label0.Text, person0.Name);
			Assert.AreEqual(label1.Text, person1.Name);
			Assert.IsNull(label2.Text);

			// stack2 (person2) 
			// stack1 (person1) 
			// stack0 (person0)
			//			label0 (min level 1)	= stack0/person0
			//			label1 (min level 2)	= null/null
			//			label2 (min level 3)	= null/null
			stack1.Children.Clear();
			Assert.AreEqual(label0.TextColor, stack0.BackgroundColor);
			Assert.AreEqual(label1.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.AreEqual(label2.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.AreEqual(label0.Text, person0.Name);
			Assert.IsNull(label1.Text);
			Assert.IsNull(label2.Text);

			// stack2 (person2) 
			// stack1 (person1) 
			// stack0 (person0)
			// grid
			//		label0 (min level 1)	= null/null
			//		label1 (min level 2)	= null/null
			//		label2 (min level 3)	= null/null
			stack0.Children.Clear();
			Assert.AreEqual(label0.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.AreEqual(label1.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.AreEqual(label2.TextColor, StackLayout.BackgroundColorProperty.DefaultValue);
			Assert.IsNull(label0.Text);
			Assert.IsNull(label1.Text);
			Assert.IsNull(label2.Text);
		}

		class PersonViewModel
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
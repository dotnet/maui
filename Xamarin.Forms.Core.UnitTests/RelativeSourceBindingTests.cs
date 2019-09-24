using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
			string bindingContext0 = "bc0";
			string bindingContext1 = "bc1";
			string bindingContext2 = "bc2";

			StackLayout stack0 = new StackLayout
			{
				StyleId = "stack0",
				BindingContext = bindingContext0,
			};

			StackLayout stack1 = new StackLayout
			{
				StyleId = "stack1",				
			};

			StackLayout stack2 = new StackLayout
			{
				StyleId = "stack2",
				BindingContext = bindingContext2
			};

			Label label0 = new Label
			{
				StyleId = "label0",
			};
			Label label1 = new Label
			{
				StyleId = "label1",
			};
			Label label2 = new Label
			{
				StyleId = "label2",
			};

			stack0.Children.Add(stack1);
			stack0.Children.Add(label2);
			stack1.Children.Add(label0);
			stack1.Children.Add(label1);

			label0.SetBinding(Label.TextProperty, new Binding
			{
				Path = nameof(StackLayout.StyleId),
				Source = new RelativeBindingSource(
					mode: RelativeBindingSourceMode.FindAncestor,
					ancestorType: typeof(StackLayout))
			});
			label1.SetBinding(Label.TextProperty, new Binding
			{
				Path = nameof(StackLayout.StyleId),
				Source = new RelativeBindingSource(
					mode: RelativeBindingSourceMode.FindAncestor,
					ancestorType: typeof(StackLayout),
					ancestorLevel: 2)
			});
			label2.SetBinding(Label.TextProperty, new Binding
			{
				Path = nameof(StackLayout.StyleId),
				Source = new RelativeBindingSource(
					mode: RelativeBindingSourceMode.FindAncestor,
					ancestorType: typeof(StackLayout),
					ancestorLevel: 10)
			});

			Assert.AreEqual(label0.Text, stack1.StyleId);
			Assert.AreEqual(label1.Text, stack0.StyleId);
			Assert.IsNull(label2.Text);

			// Ensures RelativeBindingSource.AncestorType
			// works correctly after immediate ancestor changed.
			stack1.Children.Remove(label0);
			Assert.IsNull(label0.Text);
		
			stack0.Children.Add(label0);

			Assert.AreEqual(label0.Text, stack0.StyleId);
			Assert.AreEqual(label0.BindingContext, stack0.BindingContext);

			// And after distant ancestor changed
			stack0.Children.Remove(stack1);
			stack2.Children.Add(stack1);

			Assert.AreEqual(label1.Text, stack2.StyleId);
			Assert.AreEqual(label1.BindingContext, stack2.BindingContext);

			// And after parent binding context changed
			stack2.BindingContext = "foobar";
			Assert.AreEqual(label1.Text, stack2.StyleId);
			Assert.AreEqual(label1.BindingContext, "foobar");
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

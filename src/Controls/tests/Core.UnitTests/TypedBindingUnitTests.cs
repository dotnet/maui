using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class TypedBindingUnitTests : BindingBaseUnitTests
	{

		public TypedBindingUnitTests()
		{

			ApplicationExtensions.CreateAndSetMockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.ClearCurrent();
			}

			base.Dispose(disposing);
		}



		protected override BindingBase CreateBinding(BindingMode mode = BindingMode.Default, string stringFormat = null)
		{
			return new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{
				Mode = mode,
				StringFormat = stringFormat
			};
		}

		[Fact]
		public void InvalidCtor()
		{
			Assert.Throws<ArgumentNullException>(() => new TypedBinding<MockViewModel, string>((Func<MockViewModel, (string, bool)>)null, (mvm, s) => mvm.Text = s, null));
		}

		[Theory, Category("[Binding] Set Value")]
		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		public void ValueSetOnOneWayWithComplexPathBinding(
			bool setContextFirst,
			bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = value
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), null, propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model.Text, true),
				(cmvm, s) => cmvm.Model.Model.Text = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Text")
				})
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.Equal(value, viewmodel.Model.Model.Text);
			Assert.Equal(value, bindable.GetValue(property));
			Assert.Empty(MockApplication.MockLogger.Messages);
		}

		[Theory, Category("[Binding] Complex paths")]
		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		public void ValueSetOnOneWayToSourceWithComplexPathBinding(bool setContextFirst, bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = value
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), value, propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model.Text, true),
				(cmvm, s) => cmvm.Model.Model.Text = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Text")
				})
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.True(value == (string)bindable.GetValue(property),
				"Target property changed");
			Assert.True(value == viewmodel.Model.Model.Text,
				"BindingContext property did not change");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Complex paths")]
		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		public void ValueSetOnTwoWayWithComplexPathBinding(bool setContextFirst, bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = value
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model.Text, true),
				(cmvm, s) => cmvm.Model.Model.Text = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Text")
				})
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.True(value == viewmodel.Model.Model.Text,
				"BindingContext property changed");
			Assert.True(value == (string)bindable.GetValue(property),
				"Target property did not change");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithComplexPathOnOneWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = "Foo"
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model.Text, true),
				(cmvm, s) => cmvm.Model.Model.Text = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Text")
				})
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			viewmodel.Model.Model.Text = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == viewmodel.Model.Model.Text,
				"Source property changed when it shouldn't");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithComplexPathOnOneWayToSourceBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = "Foo"
					}
				}
			};
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model.Text, true),
				(cmvm, s) => cmvm.Model.Model.Text = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Text")
				})
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			string original = (string)bindable.GetValue(property);
			const string value = "value";
			viewmodel.Model.Model.Text = value;
			Assert.True(original == (string)bindable.GetValue(property),
				"Target updated from Source on OneWayToSource");

			bindable.SetValue(property, newvalue);
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == viewmodel.Model.Model.Text,
				"Source property changed when it shouldn't");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithComplexPathOnTwoWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = "Foo"
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model.Text, true),
				(cmvm, s) => cmvm.Model.Model.Text = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Text")
				})
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			viewmodel.Model.Model.Text = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Target property did not update change");
			Assert.True(newvalue == viewmodel.Model.Model.Text,
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue2 == viewmodel.Model.Model.Text,
				"Source property did not update with Target's change");
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}



		[Theory, Category("[Binding] Indexed paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithIndexedPathOnOneWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model[1], true),
				(cmvm, s) => cmvm.Model.Model[1] = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Indexer[1]")
				})
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			viewmodel.Model.Model[1] = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == viewmodel.Model.Model[1],
				"Source property changed when it shouldn't");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Indexed paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithIndexedPathOnOneWayToSourceBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model[1], true),
				(cmvm, s) => cmvm.Model.Model[1] = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Indexer[1]")
				})
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			string original = (string)bindable.GetValue(property);
			const string value = "value";
			viewmodel.Model.Model[1] = value;
			Assert.True(original == (string)bindable.GetValue(property),
				"Target updated from Source on OneWayToSource");

			bindable.SetValue(property, newvalue);
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == viewmodel.Model.Model[1],
				"Source property changed when it shouldn't");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Indexed paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithIndexedPathOnTwoWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model[1], true),
				(cmvm, s) => cmvm.Model.Model[1] = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Indexer[1]")
				})
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			viewmodel.Model.Model[1] = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Target property did not update change");
			Assert.True(newvalue == viewmodel.Model.Model[1],
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue2 == viewmodel.Model.Model[1],
				"Source property did not update with Target's change");
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Indexed paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithIndexedArrayPathOnTwoWayBinding(bool isDefault)
		{
			var viewmodel = new ComplexMockViewModel
			{
				Array = new string[2]
			};
			viewmodel.Array[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Array[1], true),
				(cmvm, s) => cmvm.Array[1] = s,
				null)
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, new Binding("Array[1]", bindingMode));

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue2 == viewmodel.Array[1],
				"Source property did not update with Target's change");
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Self paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithSelfPathOnOneWayBinding(bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<string, string>(
				cmvm => (cmvm, true),
				(cmvm, s) => cmvm = s, null)
			{ Mode = bindingMode };
			const string value = "foo";

			var bindable = new MockBindable();
			bindable.BindingContext = value;
			bindable.SetBinding(property, binding);

			const string newvalue = "value";
			bindable.SetValue(property, newvalue);
			Assert.True(value == (string)bindable.BindingContext,
				"Source was updated from Target on OneWay binding");

			bindable.BindingContext = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == (string)bindable.BindingContext,
				"Source property changed when it shouldn't");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Self paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueDoesNotUpdateWithSelfPathOnOneWayToSourceBinding(bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<string, string>(
				cmvm => (cmvm, true), (cmvm, s) => cmvm = s, null)
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);

			const string newvalue = "new value";

			string original = (string)bindable.GetValue(property);
			bindable.BindingContext = newvalue;
			Assert.True(original == (string)bindable.GetValue(property),
				"Target updated from Source on OneWayToSource with self path");

			const string newvalue2 = "new value 2";
			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed on OneWayToSource with self path");
			Assert.True(newvalue == (string)bindable.BindingContext,
				"Source property changed on OneWayToSource with self path");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Self paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithSelfPathOnTwoWayBinding(bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new TypedBinding<string, string>(
				cmvm => (cmvm, true), (cmvm, s) => cmvm = s, null)
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = "value";
			bindable.SetBinding(property, binding);

			const string newvalue = "New Value";
			bindable.BindingContext = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Target property did not update change");
			Assert.True(newvalue == (string)bindable.BindingContext,
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue == (string)bindable.BindingContext,
				"Self-path Source changed with Target's change");
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData(BindingMode.OneWay)]
		[InlineData(BindingMode.OneWayToSource)]
		[InlineData(BindingMode.TwoWay)]
		public void SourceAndTargetAreWeakComplexPath(BindingMode mode)
		{
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value");

			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => (cmvm.Model.Model[1], true),
				(cmvm, s) => cmvm.Model.Model[1] = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model.Model, "Indexer[1]")
				})
			{ Mode = mode };

			WeakReference weakViewModel = null, weakBindable = null;

			int i = 0;
			Action create = null;
			create = () =>
			{
				if (i++ < 1024)
				{
					create();
					return;
				}
				MockBindable bindable = new MockBindable();

				weakBindable = new WeakReference(bindable);

				ComplexMockViewModel viewmodel = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Model = new ComplexMockViewModel()
					}
				};

				weakViewModel = new WeakReference(viewmodel);

				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);

				bindable.BindingContext = null;
			};

			create();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWay)
				Assert.False(weakViewModel.IsAlive, "ViewModel wasn't collected");

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource)
				Assert.False(weakBindable.IsAlive, "Bindable wasn't collected");
		}

		class TestConverter<TSource, TTarget> : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				Assert.Equal(typeof(TTarget), targetType);
				return System.Convert.ChangeType(value, targetType, CultureInfo.CurrentUICulture);
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				Assert.Equal(typeof(TSource), targetType);
				return System.Convert.ChangeType(value, targetType, CultureInfo.CurrentUICulture);
			}
		}

		[Fact]
		public void ValueConverter()
		{
			var converter = new TestConverter<string, int>();

			var vm = new MockViewModel("1");
			var property = BindableProperty.Create("TargetInt", typeof(int), typeof(MockBindable), 0);
			var binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{ Converter = converter };

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.Equal(1, bindable.GetValue(property));

			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Fact]
		public void ValueConverterBack()
		{
			var converter = new TestConverter<string, int>();

			var vm = new MockViewModel();
			var property = BindableProperty.Create("TargetInt", typeof(int), typeof(MockBindable), 1, BindingMode.OneWayToSource);
			var binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{ Converter = converter };

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.Equal("1", vm.Text);

			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		class TestConverterParameter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return parameter;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return parameter;
			}
		}

		[Fact]
		public void ValueConverterParameter()
		{
			var converter = new TestConverterParameter();

			var vm = new MockViewModel();
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "Bar", BindingMode.OneWayToSource);
			var binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{ Converter = converter, ConverterParameter = "Foo" };

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.Equal("Foo", vm.Text);

			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		class TestConverterCulture : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return culture.ToString();
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return culture.ToString();
			}
		}

#if !WINDOWS_PHONE
		[Theory, InlineData("en-US"), InlineData("pt-PT")]
		public void ValueConverterCulture(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);

			var converter = new TestConverterCulture();
			var vm = new MockViewModel();
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "Bar", BindingMode.OneWayToSource);
			var binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{ Converter = converter };
			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.Equal(culture, vm.Text);
		}
#endif

		[Fact]
		public void SelfBindingConverter()
		{
			var converter = new TestConverter<int, string>();

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "0");
			var binding = new TypedBinding<int, int>(
				mvm => (mvm, true), (mvm, s) => mvm = s, null)
			{ Converter = converter };

			var bindable = new MockBindable();
			bindable.BindingContext = 1;
			bindable.SetBinding(property, binding);
			Assert.Equal("1", bindable.GetValue(property));

			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		internal class MultiplePropertyViewModel : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			int done;
			public int Done
			{
				get { return done; }
				set
				{
					done = value;
					OnPropertyChanged();
					OnPropertyChanged("Progress");
				}
			}

			int total = 100;
			public int Total
			{
				get { return total; }
				set
				{
					if (total == value)
						return;

					total = value;
					OnPropertyChanged();
					OnPropertyChanged("Progress");
				}
			}

			public float Progress
			{
				get { return (float)done / total; }
			}

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChangedEventHandler handler = PropertyChanged;
				if (handler != null)
					handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		internal class MultiplePropertyBindable
			: BindableObject
		{
			public static readonly BindableProperty ValueProperty = BindableProperty.Create("Value", typeof(float), typeof(MultiplePropertyBindable), 0f);

			public float Value
			{
				get { return (float)GetValue(ValueProperty); }
				set { SetValue(ValueProperty, value); }
			}

			public static readonly BindableProperty DoneProperty = BindableProperty.Create("Done", typeof(int), typeof(MultiplePropertyBindable), 0);

			public int Done
			{
				get { return (int)GetValue(DoneProperty); }
				set { SetValue(DoneProperty, value); }
			}
		}

		[Fact]
		public void MultiplePropertyUpdates()
		{
			var mpvm = new MultiplePropertyViewModel();

			var bindable = new MultiplePropertyBindable();
			var progressBinding = new TypedBinding<MultiplePropertyViewModel, float>(vm => (vm.Progress, true), null, new[] {
				new Tuple<Func<MultiplePropertyViewModel, object>, string> (vm=>vm, "Progress"),
			})
			{ Mode = BindingMode.OneWay };
			var doneBinding = new TypedBinding<MultiplePropertyViewModel, int>(vm => (vm.Done, true), (vm, d) => vm.Done = d, new[] {
				new Tuple<Func<MultiplePropertyViewModel, object>, string> (vm=>vm, "Done"),
			})
			{ Mode = BindingMode.OneWayToSource };

			bindable.SetBinding(MultiplePropertyBindable.ValueProperty, progressBinding);
			bindable.SetBinding(MultiplePropertyBindable.DoneProperty, doneBinding);
			bindable.BindingContext = mpvm;

			bindable.Done = 5;

			Assert.Equal(5, mpvm.Done);
			Assert.Equal(0.05f, mpvm.Progress);
			Assert.Equal(5, bindable.Done);
			Assert.Equal(0.05f, bindable.Value);

			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Fact, Category("[Binding] Complex paths")]
		[Description("When part of a complex path can not be evaluated during an update, bindables should return to their default value.")]
		public void NullInPathUsesDefaultValue()
		{
			var vm = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel()
			};

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "foo bar");

			var bindable = new MockBindable();
			var binding = new TypedBinding<ComplexMockViewModel, string>(cvm => (cvm.Model.Text, true), (cvm, t) => cvm.Model.Text = t, new[] {
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm, "Model"),
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm.Model, "Text")
			})
			{ Mode = BindingMode.OneWay };
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			vm.Model = null;

			Assert.Equal(property.DefaultValue, bindable.GetValue(property));
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Fact, Category("[Binding] Complex paths")]
		[Description("When part of a complex path can not be evaluated during an update, bindables should return to their default value.")]
		public void NullContextUsesDefaultValue()
		{
			var vm = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Text = "vm value"
				}
			};

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "foo bar");
			var binding = new TypedBinding<ComplexMockViewModel, string>(cvm => (cvm.Model.Text, true), (cvm, t) => cvm.Model.Text = t, new[] {
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm, "Model"),
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm.Model, "Text")
			})
			{ Mode = BindingMode.OneWay };
			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.Equal(bindable.GetValue(property), vm.Model.Text);

			bindable.BindingContext = null;

			Assert.Equal(property.DefaultValue, bindable.GetValue(property));
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Fact, Category("[Binding] Complex paths")]
		[Description("When part of a complex path can not be evaluated during an update, bindables should return to their default value, or TargetNullValue")]
		public void NullContextUsesFallbackValue()
		{
			var vm = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Text = "vm value"
				}
			};

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "foo bar");
			var binding = new TypedBinding<ComplexMockViewModel, string>(cvm => (cvm.Model.Text, true), (cvm, t) => cvm.Model.Text = t, new[] {
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm, "Model"),
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm.Model, "Text")
			})
			{ Mode = BindingMode.OneWay, FallbackValue = "fallback" };
			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.Equal(bindable.GetValue(property), vm.Model.Text);

			bindable.BindingContext = null;

			Assert.Equal("fallback", bindable.GetValue(property));
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Fact]
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/4103
		public void TestTargetNullValue()
		{
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), default(string));
			var binding = new TypedBinding<MockViewModel, string>(vm => (vm.Text, true), null, null) { TargetNullValue = "target null" };
			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = new MockViewModel("initial");
			Assert.Equal("initial", bindable.GetValue(property));

			bindable.BindingContext = new MockViewModel(null);
			Assert.Equal("target null", bindable.GetValue(property));

		}

		[Fact("OneWay bindings should not double apply on source updates.")]
		public void OneWayBindingsDontDoubleApplyOnSourceUpdates()
		{
			var vm = new ComplexMockViewModel();

			var bindable = new MockBindable();
			var binding = new TypedBinding<ComplexMockViewModel, int>(cmvm => (cmvm.QueryCount, true), null, null) { Mode = BindingMode.OneWay };
			bindable.SetBinding(MultiplePropertyBindable.DoneProperty, binding);
			bindable.BindingContext = vm;

			Assert.Equal(1, vm.count);

			bindable.BindingContext = null;

			Assert.True(1 == vm.count, "Source property was queried on an unset");

			bindable.BindingContext = vm;

			Assert.True(2 == vm.count, "Source property was queried multiple times on a reapply");
		}

		[Fact("When there are multiple bindings, an update in one should not cause the other to udpate.")]
		public void BindingsShouldNotTriggerOtherBindings()
		{
			var vm = new ComplexMockViewModel();

			var bindable = new MockBindable();
			var qcbinding = new TypedBinding<ComplexMockViewModel, int>(cmvm => (cmvm.QueryCount, true), null, null) { Mode = BindingMode.OneWay };
			var textBinding = new TypedBinding<ComplexMockViewModel, string>(cmvm => (cmvm.Text, true), null, null) { Mode = BindingMode.OneWay };
			bindable.SetBinding(MultiplePropertyBindable.DoneProperty, qcbinding);
			bindable.SetBinding(MockBindable.TextProperty, textBinding);
			bindable.BindingContext = vm;

			Assert.Equal(1, vm.count);

			vm.Text = "update";

			Assert.True(1 == vm.count, "Source property was queried due to a different binding update.");
		}

		internal class DerivedViewModel
			: MockViewModel
		{
			public override string Text
			{
				get { return base.Text + "2"; }
				set { base.Text = value; }
			}
		}

		[Fact("The most derived version of a property should always be called.")]
		public void MostDerviedPropertyOnContextSwitchOfSimilarType()
		{
			var vm = new MockViewModel { Text = "text" };

			var bindable = new MockBindable();
			bindable.BindingContext = vm;
			var binding = new TypedBinding<MockViewModel, string>(mvm => (mvm.Text, true), (mvm, s) => mvm.Text = s, new[] {
				new Tuple<Func<MockViewModel, object>, string>(mvm=>mvm, "Text")
			});
			bindable.SetBinding(MockBindable.TextProperty, binding);

			Assert.Equal(vm.Text, bindable.GetValue(MockBindable.TextProperty));

			bindable.BindingContext = vm = new DerivedViewModel { Text = "text" };

			Assert.Equal(vm.Text, bindable.GetValue(MockBindable.TextProperty));
		}

		[Fact("When binding with a multi-part path and part is null, no error should be thrown or logged")]
		public void ChainedPartNull()
		{
			var bindable = new MockBindable { BindingContext = new ComplexMockViewModel() };
			var binding = new TypedBinding<ComplexMockViewModel, string>(
			  cmvm => (cmvm.Model.Text, true),
			  (cmvm, s) => cmvm.Model.Text = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Text"),
				});

			bindable.SetBinding(MockBindable.TextProperty, binding);
			Assert.True(MockApplication.MockLogger.Messages.Count == 0, "An error was logged");
		}

		[Fact]
		public void SetBindingContextBeforeContextBindingAndInnerBindings()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			view.BindingContext = new Tuple<string, string>("Foo", "Bar");
			var bindingItem1 = new TypedBinding<Tuple<string, string>, string>(s => (s.Item1, true), null, null);
			var bindingSelf = new TypedBinding<string, string>(s => (s, true), null, null);
			label.SetBinding(BindableObject.BindingContextProperty, bindingItem1);
			label.SetBinding(Label.TextProperty, bindingSelf);

			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void SetBindingContextAndInnerBindingBeforeContextBinding()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			view.BindingContext = new Tuple<string, string>("Foo", "Bar");
			var bindingItem1 = new TypedBinding<Tuple<string, string>, string>(s => (s.Item1, true), null, null);
			var bindingSelf = new TypedBinding<string, string>(s => (s, true), null, null);
			label.SetBinding(Label.TextProperty, bindingSelf);
			label.SetBinding(BindableObject.BindingContextProperty, bindingItem1);

			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void SetBindingContextAfterContextBindingAndInnerBindings()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };
			var bindingItem1 = new TypedBinding<Tuple<string, string>, string>(s => (s.Item1, true), null, null);
			var bindingSelf = new TypedBinding<string, string>(s => (s, true), null, null);

			label.SetBinding(BindableObject.BindingContextProperty, bindingItem1);
			label.SetBinding(Label.TextProperty, bindingSelf);
			view.BindingContext = new Tuple<string, string>("Foo", "Bar");

			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void SetBindingContextAfterInnerBindingsAndContextBinding()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };
			var bindingItem1 = new TypedBinding<Tuple<string, string>, string>(s => (s.Item1, true), null, null);
			var bindingSelf = new TypedBinding<string, string>(s => (s, true), null, null);

			label.SetBinding(Label.TextProperty, bindingItem1);
			label.SetBinding(BindableObject.BindingContextProperty, bindingItem1);
			view.BindingContext = new Tuple<string, string>("Foo", "Bar");

			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void Convert()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			var slider = new Slider();
			var vm = new MockViewModel { Text = "0.5" };
			slider.BindingContext = vm;
			slider.SetBinding(Slider.ValueProperty, new TypedBinding<MockViewModel, string>(mvm => (mvm.Text, true), (mvm, s) => mvm.Text = s, null) { Mode = BindingMode.TwoWay });

			Assert.Equal(0.5, slider.Value);

			slider.Value = 0.9;

			Assert.Equal("0.9", vm.Text);
		}

#if !WINDOWS_PHONE
		[Theory]
		[InlineData("en-US", "0.5", 0.5, 0.9, "0.9")]
		[InlineData("pt-PT", "0,5", 0.5, 0.9, "0,9")]
		public void ConvertIsCultureAware(string culture, string sliderSetStringValue, double sliderExpectedDoubleValue, double sliderSetDoubleValue, string sliderExpectedStringValue)

		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

			var slider = new Slider();
			var vm = new MockViewModel { Text = sliderSetStringValue };
			slider.BindingContext = vm;
			slider.SetBinding(Slider.ValueProperty, new TypedBinding<MockViewModel, string>(mvm => (mvm.Text, true), (mvm, s) => mvm.Text = s, null) { Mode = BindingMode.TwoWay });

			Assert.Equal(slider.Value, sliderExpectedDoubleValue);

			slider.Value = sliderSetDoubleValue;

			Assert.Equal(vm.Text, sliderExpectedStringValue);
		}
#endif

		[Fact]
		public void FailToConvert()
		{
			var slider = new Slider();
			slider.BindingContext = new ComplexMockViewModel { Model = new ComplexMockViewModel() };

			slider.SetBinding(Slider.ValueProperty, new TypedBinding<ComplexMockViewModel, ComplexMockViewModel>(mvm => (mvm.Model, true), null, null));

			Assert.Equal(slider.Value, Slider.ValueProperty.DefaultValue);
			Assert.True(MockApplication.MockLogger.Messages.Count == 1, "No error logged");
		}

		class NullViewModel : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public string Foo
			{
				get;
				set;
			}

			public string Bar
			{
				get;
				set;
			}

			public void SignalAllPropertiesChanged(bool useNull)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((useNull) ? null : String.Empty));
			}
		}

		class MockBindable2 : MockBindable
		{
			public static readonly BindableProperty Text2Property = BindableProperty.Create("Text2", typeof(string), typeof(MockBindable2), "default", BindingMode.TwoWay);
			public string Text2
			{
				get { return (string)GetValue(Text2Property); }
				set { SetValue(Text2Property, value); }
			}
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory]
		public void NullPropertyUpdatesAllBindings(bool useStringEmpty)
		{
			var vm = new NullViewModel();
			var bindable = new MockBindable2();
			bindable.BindingContext = vm;
			bindable.SetBinding(MockBindable.TextProperty, new TypedBinding<NullViewModel, string>(nvm => (nvm.Foo, true), null, new[] {
				new Tuple<Func<NullViewModel, object>, string>(nvm=>nvm,"Foo")
			}));
			bindable.SetBinding(MockBindable2.Text2Property, new TypedBinding<NullViewModel, string>(nvm => (nvm.Bar, true), null, new[] {
				new Tuple<Func<NullViewModel, object>, string>(nvm=>nvm,"Bar")
			}));

			vm.Foo = "Foo";
			vm.Bar = "Bar";
			vm.SignalAllPropertiesChanged(useNull: !useStringEmpty);

			Assert.Equal("Foo", bindable.Text);
			Assert.Equal("Bar", bindable.Text2);
		}

		[Fact]
		public void BindingSourceOverContext()
		{
			var label = new Label();
			label.BindingContext = "bindingcontext";
			var bindingSelf = new TypedBinding<string, string>(s => (s, true), null, null);
			label.SetBinding(Label.TextProperty, bindingSelf);
			Assert.Equal("bindingcontext", label.Text);

			var bindingSelfSource = new TypedBinding<string, string>(s => (s, true), null, null) { Source = "bindingsource" };
			label.SetBinding(Label.TextProperty, bindingSelfSource);
			Assert.Equal("bindingsource", label.Text);
		}

		class TestViewModel : INotifyPropertyChanged
		{
			event PropertyChangedEventHandler PropertyChanged;
			event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
			{
				add { PropertyChanged += value; }
				remove { PropertyChanged -= value; }
			}

			string _foo;

			public string Foo
			{
				get => _foo;
				set
				{
					if (_foo != value)
					{
						_foo = value;
						OnPropertyChanged(nameof(Foo));
					}
				}
			}

			public int InvocationListSize()
			{
				if (PropertyChanged == null)
					return 0;
				return PropertyChanged.GetInvocationList().Length;
			}

			public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Fact]
		public async Task BindingUnsubscribesForDeadTarget()
		{
			var viewmodel = new TestViewModel();
			WeakReference bindingRef = null, buttonRef = null;

			int i = 0;
			Action create = null;
			create = () =>
			{
				if (i++ < 1024)
				{
					create();
					return;
				}

				var button = new Button();
				var binding = new TypedBinding<TestViewModel, string>(vm => (vm.Foo, true), (vm, s) => vm.Foo = s, new[] {
					new Tuple<Func<TestViewModel, object>, string>(vm=>vm,"Foo")
				});
				button.SetBinding(Button.TextProperty, binding);
				button.BindingContext = viewmodel;
				bindingRef = new WeakReference(binding);
				buttonRef = new WeakReference(button);
			};

			create();
			Assert.Equal(viewmodel.Foo = "Bar", ((Button)buttonRef.Target).Text);

			Assert.Equal(1, viewmodel.InvocationListSize());

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			viewmodel.OnPropertyChanged("Foo");

			Assert.Equal(0, viewmodel.InvocationListSize());

			Assert.False(bindingRef.IsAlive, "Binding should not be alive!");
			Assert.False(buttonRef.IsAlive, "Button should not be alive!");
		}

		[Fact]
		public async Task BindingDoesNotStayAliveForDeadTarget()
		{
			var viewModel = new TestViewModel();
			WeakReference bindingRef = null, buttonRef = null, proxyRef = null;

			int i = 0;
			Action create = null;
			create = () =>
			{
				if (i++ < 1024)
				{
					create();
					return;
				}

				var binding = new TypedBinding<TestViewModel, string>(vm => (vm.Foo, true), (vm, s) => vm.Foo = s, new[] {
					new Tuple<Func<TestViewModel, object>, string>(vm=>vm,"Foo")
				});
				var button = new Button();
				button.SetBinding(Button.TextProperty, binding);
				button.BindingContext = viewModel;

				bindingRef = new WeakReference(binding);
				buttonRef = new WeakReference(button);

				// Access private members:
				// WeakPropertyChangedProxy proxy = binding._handlers[0].Listener;
				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				var handlers = binding.GetType().GetField("_handlers", flags).GetValue(binding) as object[];
				Assert.NotNull(handlers);
				var handler = handlers[0];
				var proxy = handler.GetType().GetProperty("Listener").GetValue(handler);
				Assert.NotNull(proxy);
				proxyRef = new WeakReference(proxy);
			};

			create();
			Assert.Equal(viewModel.Foo = "Bar", ((Button)buttonRef.Target).Text);

			Assert.Equal(1, viewModel.InvocationListSize());

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(bindingRef.IsAlive, "Binding should not be alive!");
			Assert.False(buttonRef.IsAlive, "Button should not be alive!");

			// WeakPropertyChangedProxy won't go away until the second GC, PropertyChangedProxy unsubscribes in its finalizer
			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Assert.False(proxyRef.IsAlive, "WeakPropertyChangedProxy should not be alive!");
		}

		[Fact]
		public void BindingCreatesSingleSubscription()
		{
			TestViewModel viewmodel = new TestViewModel();
			var binding = new TypedBinding<TestViewModel, string>(vm => (vm.Foo, true), (vm, s) => vm.Foo = s, new[] {
					new Tuple<Func<TestViewModel, object>, string>(vm=>vm,"Foo")
				});

			var button = new Button();
			button.SetBinding(Button.TextProperty, binding);
			button.BindingContext = viewmodel;

			Assert.Equal(1, viewmodel.InvocationListSize());
		}

		public class IndexedViewModel : INotifyPropertyChanged
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();

			[IndexerName("Item")]
			public object this[string index]
			{
				get { return dict[index]; }
				set
				{
					dict[index] = value;
					OnPropertyChanged($"Item[{index}]");
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Fact]
		public void IndexedViewModelPropertyChanged()
		{
			var label = new Label();
			var viewModel = new IndexedViewModel();

			var binding = new TypedBinding<Tuple<IndexedViewModel, object>, object>(
				vm => (vm.Item1["Foo"], true),
				(vm, s) => vm.Item1["Foo"] = s,
				new[] {
					new Tuple<Func<Tuple<IndexedViewModel, object>, object>, string>(vm=>vm, "Item1"),
					new Tuple<Func<Tuple<IndexedViewModel, object>, object>, string>(vm=>vm.Item1, "Item[Foo]"),
				});

			label.BindingContext = new Tuple<IndexedViewModel, object>(viewModel, new object());
			label.SetBinding(Label.TextProperty, binding);
			Assert.Null(label.Text);

			viewModel["Foo"] = "Baz";

			Assert.Equal("Baz", label.Text);
		}

		[Fact]
		public void OneTimeBindingDoesntUpdateOnPropertyChanged()
		{
			var view = new VisualElement();
			var bp1t = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			var bp1w = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			var vm = new MockViewModel("foobar");
			view.BindingContext = vm;
			var b1t = CreateBinding(mode: BindingMode.OneTime);
			var b1w = CreateBinding(mode: BindingMode.OneWay);

			view.SetBinding(bp1t, b1t);
			view.SetBinding(bp1w, b1w);
			Assert.Equal("foobar", view.GetValue(bp1w));
			Assert.Equal("foobar", view.GetValue(bp1t));

			vm.Text = "qux";
			Assert.Equal("qux", view.GetValue(bp1w));
			Assert.Equal("foobar", view.GetValue(bp1t));
		}

		[Fact]
		public void OneTimeBindingUpdatesOnBindingContextChanged()
		{
			var view = new VisualElement();
			var bp1t = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			var bp1w = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			view.BindingContext = new MockViewModel("foobar");
			var b1t = CreateBinding(mode: BindingMode.OneTime);
			var b1w = CreateBinding(mode: BindingMode.OneWay);

			view.SetBinding(bp1t, b1t);
			view.SetBinding(bp1w, b1w);
			Assert.Equal("foobar", view.GetValue(bp1w));
			Assert.Equal("foobar", view.GetValue(bp1t));

			view.BindingContext = new MockViewModel("qux");
			Assert.Equal("qux", view.GetValue(bp1w));
			Assert.Equal("qux", view.GetValue(bp1t));
		}

		[Fact]
		public void OneTimeBindingDoesntUpdateNeedSettersOrHandlers()
		{
			var view = new VisualElement();
			var bp1t = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			var vm = new MockViewModel("foobar");
			view.BindingContext = vm;

			var b1t = new TypedBinding<MockViewModel, string>(v => (v.Text, true), null, null);

			view.SetBinding(bp1t, b1t);
			Assert.Equal("foobar", view.GetValue(bp1t));

			vm.Text = "qux";
			Assert.Equal("foobar", view.GetValue(bp1t));
		}

		[Fact(Skip = "SpeedTestApply")]
		public void SpeedTestApply()
		{

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var vm0 = new MockViewModel { Text = "Foo" };
			var vm1 = new MockViewModel { Text = "Bar" };
			var bindable = new MockBindable();

			var it = 100000;

			BindingBase binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				});

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var swtb = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
			{
				binding.Apply(i % 2 == 0 ? vm0 : vm1, bindable, property, false, SetterSpecificity.FromBinding);
				binding.Unapply();
			}
			swtb.Stop();
			Assert.Equal("Bar", bindable.GetValue(property));

			binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: null);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var swtbh = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
			{
				binding.Apply(i % 2 == 0 ? vm0 : vm1, bindable, property, false, SetterSpecificity.FromBinding);
				binding.Unapply();
			}
			swtbh.Stop();
			Assert.Equal("Bar", bindable.GetValue(property));

			binding = new Binding("Text");
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var swb = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
			{
				binding.Apply(i % 2 == 0 ? vm0 : vm1, bindable, property, false, SetterSpecificity.FromBinding);
				binding.Unapply();
			}
			swb.Stop();
			Assert.Equal("Bar", bindable.GetValue(property));

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var swsv = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
				bindable.SetValue(property, (i % 2 == 0 ? vm0 : vm1).Text);
			swsv.Stop();
			Assert.Equal("Bar", bindable.GetValue(property));

			throw new XunitException($"Applying {it} Typedbindings\t\t\t: {swtb.ElapsedMilliseconds}ms.\nApplying {it} Typedbindings (without INPC)\t: {swtbh.ElapsedMilliseconds}ms.\nApplying {it} Bindings\t\t\t: {swb.ElapsedMilliseconds}ms.\nSetting  {it} values\t\t\t\t: {swsv.ElapsedMilliseconds}ms.");
		}

		[Fact(Skip = "SpeedTestSetBC")]
		public void SpeedTestSetBC()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var vm0 = new MockViewModel { Text = "Foo" };
			var vm1 = new MockViewModel { Text = "Bar" };
			var bindable = new MockBindable();

			var it = 100000;

			BindingBase binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				});

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			bindable.SetBinding(property, binding);
			var swtb = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
				bindable.BindingContext = i % 2 == 0 ? vm0 : vm1;
			swtb.Stop();
			//Assert.Equal("Bar", bindable.GetValue(property));

			binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: null);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			bindable.SetBinding(property, binding);
			var swtbh = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
				bindable.BindingContext = i % 2 == 0 ? vm0 : vm1;
			swtbh.Stop();
			Assert.Equal("Bar", bindable.GetValue(property));

			binding = new Binding("Text");
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			bindable.SetBinding(property, binding);
			var swb = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
				bindable.BindingContext = i % 2 == 0 ? vm0 : vm1;
			swb.Stop();
			Assert.Equal("Bar", bindable.GetValue(property));

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			bindable.SetBinding(property, binding);
			var swsv = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
				bindable.SetValue(property, (i % 2 == 0 ? vm0 : vm1).Text);
			swsv.Stop();
			Assert.Equal("Bar", bindable.GetValue(property));

			throw new XunitException($"Setting BC for {it} Typedbindings\t\t\t: {swtb.ElapsedMilliseconds}ms.\nSetting BC for {it} Typedbindings (without INPC)\t: {swtbh.ElapsedMilliseconds}ms.\nSetting BC for {it} Bindings\t\t\t\t: {swb.ElapsedMilliseconds}ms.\nSetting  {it} values\t\t\t\t\t: {swsv.ElapsedMilliseconds}ms.");
		}

		class VM3650 : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public int Count { get; set; }

			string _title = "default";
			public string Title
			{
				get
				{
					Count++;
					return _title;
				}
				set
				{
					_title = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));
				}
			}
		}

		[Fact]
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/3650
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/3613
		public void TypedBindingsShouldNotHang()
		{
			var typedBinding = new TypedBinding<VM3650, string>(
				vm => (vm.Title, true),
				(vm, s) => vm.Title = s,
				new Tuple<Func<VM3650, object>, string>[] {
					new Tuple<Func<VM3650, object>, string>(vm=>vm, "Title")
				});
			var vm3650 = new VM3650();
			var label = new Label();
			label.SetBinding(Label.TextProperty, typedBinding);
			label.BindingContext = vm3650;

			Assert.Equal("default", label.Text);
			Assert.Equal(1, vm3650.Count);

			vm3650.Count = 0;
			vm3650.Title = "foo";
			Assert.Equal("foo", label.Text);
			Assert.Equal(1, vm3650.Count);

			vm3650.Count = 0;
			vm3650.Title = "bar";
			Assert.Equal("bar", label.Text);
			Assert.Equal(1, vm3650.Count);

			vm3650.Count = 0;
			vm3650.Title = "baz";
			Assert.Equal("baz", label.Text);
			Assert.Equal(1, vm3650.Count);

			vm3650.Count = 0;
			vm3650.Title = "qux";
			Assert.Equal("qux", label.Text);
			Assert.Equal(1, vm3650.Count);

		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;
using DescriptionAttribute = NUnit.Framework.DescriptionAttribute;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TypedBindingUnitTests : BindingBaseUnitTests
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			log = new Logger();

			Device.PlatformServices = new MockPlatformServices();
			Log.Listeners.Add(log);
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
			Log.Listeners.Remove(log);
		}

		protected override BindingBase CreateBinding(BindingMode mode = BindingMode.Default, string stringFormat = null)
		{
			return new TypedBinding<MockViewModel, string>(
				getter: mvm => mvm.Text,
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{
				Mode = mode,
				StringFormat = stringFormat
			};
		}

		[Test]
		public void InvalidCtor()
		{
			Assert.Throws<ArgumentNullException>(() => new TypedBinding<MockViewModel, string>((Func<MockViewModel, string>)null, (mvm, s) => mvm.Text = s, null), "Allowed null getter");
		}

		[Test, NUnit.Framework.Category("[Binding] Set Value")]
		public void ValueSetOnOneWayWithComplexPathBinding(
			[Values(true, false)] bool setContextFirst,
			[Values(true, false)] bool isDefault)
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
				cmvm => cmvm.Model.Model.Text,
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

			Assert.AreEqual(value, viewmodel.Model.Model.Text,
				"BindingContext property changed");
			Assert.AreEqual(value, bindable.GetValue(property),
				"Target property did not change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Complex paths")]
		public void ValueSetOnOneWayToSourceWithComplexPathBinding(
			[Values(true, false)] bool setContextFirst,
			[Values(true, false)] bool isDefault)
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
				cmvm => cmvm.Model.Model.Text,
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

			Assert.AreEqual(value, bindable.GetValue(property),
				"Target property changed");
			Assert.AreEqual(value, viewmodel.Model.Model.Text,
				"BindingContext property did not change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Complex paths")]
		public void ValueSetOnTwoWayWithComplexPathBinding(
			[Values(true, false)] bool setContextFirst,
			[Values(true, false)] bool isDefault)
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
				cmvm => cmvm.Model.Model.Text,
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

			Assert.AreEqual(value, viewmodel.Model.Model.Text,
				"BindingContext property changed");
			Assert.AreEqual(value, bindable.GetValue(property),
				"Target property did not change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Complex paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm.Model.Model.Text,
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
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual(newvalue, viewmodel.Model.Model.Text,
				"Source property changed when it shouldn't");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Complex paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm.Model.Model.Text,
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
			Assert.AreEqual(original, bindable.GetValue(property),
				"Target updated from Source on OneWayToSource");

			bindable.SetValue(property, newvalue);
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual(newvalue, viewmodel.Model.Model.Text,
				"Source property changed when it shouldn't");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Complex paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm.Model.Model.Text,
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
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Target property did not update change");
			Assert.AreEqual(newvalue, viewmodel.Model.Model.Text,
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.AreEqual(newvalue2, viewmodel.Model.Model.Text,
				"Source property did not update with Target's change");
			Assert.AreEqual(newvalue2, bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}



		[Category("[Binding] Indexed paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm.Model.Model[1],
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
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual(newvalue, viewmodel.Model.Model[1],
				"Source property changed when it shouldn't");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Indexed paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm.Model.Model[1],
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
			Assert.AreEqual(original, bindable.GetValue(property),
				"Target updated from Source on OneWayToSource");

			bindable.SetValue(property, newvalue);
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual(newvalue, viewmodel.Model.Model[1],
				"Source property changed when it shouldn't");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Indexed paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm.Model.Model[1],
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
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Target property did not update change");
			Assert.AreEqual(newvalue, viewmodel.Model.Model[1],
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.AreEqual(newvalue2, viewmodel.Model.Model[1],
				"Source property did not update with Target's change");
			Assert.AreEqual(newvalue2, bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Indexed paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm.Array[1],
				(cmvm, s) => cmvm.Array[1] = s,
				null)
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, new Binding("Array[1]", bindingMode));

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.AreEqual(newvalue2, viewmodel.Array[1],
				"Source property did not update with Target's change");
			Assert.AreEqual(newvalue2, bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Self paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm,
				(cmvm, s) => cmvm = s, null)
			{ Mode = bindingMode };
			const string value = "foo";

			var bindable = new MockBindable();
			bindable.BindingContext = value;
			bindable.SetBinding(property, binding);

			const string newvalue = "value";
			bindable.SetValue(property, newvalue);
			Assert.AreEqual(value, bindable.BindingContext,
				"Source was updated from Target on OneWay binding");

			bindable.BindingContext = newvalue;
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual(newvalue, bindable.BindingContext,
				"Source property changed when it shouldn't");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Self paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm, (cmvm, s) => cmvm = s, null)
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);

			const string newvalue = "new value";

			string original = (string)bindable.GetValue(property);
			bindable.BindingContext = newvalue;
			Assert.AreEqual(original, bindable.GetValue(property),
				"Target updated from Source on OneWayToSource with self path");

			const string newvalue2 = "new value 2";
			bindable.SetValue(property, newvalue2);
			Assert.AreEqual(newvalue2, bindable.GetValue(property),
				"Target property changed on OneWayToSource with self path");
			Assert.AreEqual(newvalue, bindable.BindingContext,
				"Source property changed on OneWayToSource with self path");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Self paths")]
		[TestCase(true)]
		[TestCase(false)]
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
				cmvm => cmvm, (cmvm, s) => cmvm = s, null)
			{ Mode = bindingMode };

			var bindable = new MockBindable();
			bindable.BindingContext = "value";
			bindable.SetBinding(property, binding);

			const string newvalue = "New Value";
			bindable.BindingContext = newvalue;
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Target property did not update change");
			Assert.AreEqual(newvalue, bindable.BindingContext,
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.AreEqual(newvalue, bindable.BindingContext,
				"Self-path Source changed with Target's change");
			Assert.AreEqual(newvalue2, bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Complex paths")]
		[TestCase(BindingMode.OneWay)]
		[TestCase(BindingMode.OneWayToSource)]
		[TestCase(BindingMode.TwoWay)]
		public void SourceAndTargetAreWeakComplexPath(BindingMode mode)
		{
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value");

			var binding = new TypedBinding<ComplexMockViewModel, string>(
				cmvm => cmvm.Model.Model[1],
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
				Assert.IsFalse(weakViewModel.IsAlive, "ViewModel wasn't collected");

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource)
				Assert.IsFalse(weakBindable.IsAlive, "Bindable wasn't collected");
		}

		class TestConverter<TSource, TTarget> : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				Assert.AreEqual(typeof(TTarget), targetType);
				return System.Convert.ChangeType(value, targetType, CultureInfo.CurrentUICulture);
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				Assert.AreEqual(typeof(TSource), targetType);
				return System.Convert.ChangeType(value, targetType, CultureInfo.CurrentUICulture);
			}
		}

		[Test]
		public void ValueConverter()
		{
			var converter = new TestConverter<string, int>();

			var vm = new MockViewModel("1");
			var property = BindableProperty.Create("TargetInt", typeof(int), typeof(MockBindable), 0);
			var binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => mvm.Text,
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{ Converter = converter };

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.AreEqual(1, bindable.GetValue(property));

			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test]
		public void ValueConverterBack()
		{
			var converter = new TestConverter<string, int>();

			var vm = new MockViewModel();
			var property = BindableProperty.Create("TargetInt", typeof(int), typeof(MockBindable), 1, BindingMode.OneWayToSource);
			var binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => mvm.Text,
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{ Converter = converter };

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.AreEqual("1", vm.Text);

			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
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

		[Test]
		public void ValueConverterParameter()
		{
			var converter = new TestConverterParameter();

			var vm = new MockViewModel();
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "Bar", BindingMode.OneWayToSource);
			var binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => mvm.Text,
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{ Converter = converter, ConverterParameter = "Foo" };

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.AreEqual("Foo", vm.Text);

			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
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
		[TestCase("en-US"), TestCase("pt-PT")]
		public void ValueConverterCulture(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);

			var converter = new TestConverterCulture();
			var vm = new MockViewModel();
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "Bar", BindingMode.OneWayToSource);
			var binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => mvm.Text,
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] {
					new Tuple<Func<MockViewModel, object>, string> (mvm=>mvm, "Text")
				})
			{ Converter = converter };
			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.AreEqual(culture, vm.Text);
		}
#endif

		[Test]
		public void SelfBindingConverter()
		{
			var converter = new TestConverter<int, string>();

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "0");
			var binding = new TypedBinding<int, int>(
				mvm => mvm, (mvm, s) => mvm = s, null)
			{ Converter = converter };

			var bindable = new MockBindable();
			bindable.BindingContext = 1;
			bindable.SetBinding(property, binding);
			Assert.AreEqual("1", bindable.GetValue(property));

			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
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

		[Test]
		public void MultiplePropertyUpdates()
		{
			var mpvm = new MultiplePropertyViewModel();

			var bindable = new MultiplePropertyBindable();
			var progressBinding = new TypedBinding<MultiplePropertyViewModel, float>(vm => vm.Progress, null, new[] {
				new Tuple<Func<MultiplePropertyViewModel, object>, string> (vm=>vm, "Progress"),
			})
			{ Mode = BindingMode.OneWay };
			var doneBinding = new TypedBinding<MultiplePropertyViewModel, int>(vm => vm.Done, (vm, d) => vm.Done = d, new[] {
				new Tuple<Func<MultiplePropertyViewModel, object>, string> (vm=>vm, "Done"),
			})
			{ Mode = BindingMode.OneWayToSource };

			bindable.SetBinding(MultiplePropertyBindable.ValueProperty, progressBinding);
			bindable.SetBinding(MultiplePropertyBindable.DoneProperty, doneBinding);
			bindable.BindingContext = mpvm;

			bindable.Done = 5;

			Assert.AreEqual(5, mpvm.Done);
			Assert.AreEqual(0.05f, mpvm.Progress);
			Assert.AreEqual(5, bindable.Done);
			Assert.AreEqual(0.05f, bindable.Value);

			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Complex paths")]
		[Description("When part of a complex path can not be evaluated during an update, bindables should return to their default value.")]
		public void NullInPathUsesDefaultValue()
		{
			var vm = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel()
			};

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "foo bar");

			var bindable = new MockBindable();
			var binding = new TypedBinding<ComplexMockViewModel, string>(cvm => cvm.Model.Text, (cvm, t) => cvm.Model.Text = t, new[] {
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm, "Model"),
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm.Model, "Text")
			})
			{ Mode = BindingMode.OneWay };
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			vm.Model = null;

			Assert.AreEqual(property.DefaultValue, bindable.GetValue(property));
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Complex paths")]
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
			var binding = new TypedBinding<ComplexMockViewModel, string>(cvm => cvm.Model.Text, (cvm, t) => cvm.Model.Text = t, new[] {
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm, "Model"),
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm.Model, "Text")
			})
			{ Mode = BindingMode.OneWay };
			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assume.That(bindable.GetValue(property), Is.EqualTo(vm.Model.Text));

			bindable.BindingContext = null;

			Assert.AreEqual(property.DefaultValue, bindable.GetValue(property));
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Complex paths")]
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
			var binding = new TypedBinding<ComplexMockViewModel, string>(cvm => cvm.Model.Text, (cvm, t) => cvm.Model.Text = t, new[] {
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm, "Model"),
				new Tuple<Func<ComplexMockViewModel, object>, string>(cvm=>cvm.Model, "Text")
			})
			{ Mode = BindingMode.OneWay, FallbackValue = "fallback" };
			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assume.That(bindable.GetValue(property), Is.EqualTo(vm.Model.Text));

			bindable.BindingContext = null;

			Assert.AreEqual("fallback", bindable.GetValue(property));
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test]
		//https://github.com/xamarin/Xamarin.Forms/issues/4103
		public void TestTargetNullValue()
		{
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), default(string));
			var binding = new TypedBinding<MockViewModel, string>(vm => vm.Text, null, null) { TargetNullValue = "target null" };
			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = new MockViewModel("initial");
			Assert.That(bindable.GetValue(property), Is.EqualTo("initial"));

			bindable.BindingContext = new MockViewModel(null);
			Assert.That(bindable.GetValue(property), Is.EqualTo("target null"));

		}

		[Test]
		[Description("OneWay bindings should not double apply on source updates.")]
		public void OneWayBindingsDontDoubleApplyOnSourceUpdates()
		{
			var vm = new ComplexMockViewModel();

			var bindable = new MockBindable();
			var binding = new TypedBinding<ComplexMockViewModel, int>(cmvm => cmvm.QueryCount, null, null) { Mode = BindingMode.OneWay };
			bindable.SetBinding(MultiplePropertyBindable.DoneProperty, binding);
			bindable.BindingContext = vm;

			Assert.AreEqual(1, vm.count);

			bindable.BindingContext = null;

			Assert.AreEqual(1, vm.count, "Source property was queried on an unset");

			bindable.BindingContext = vm;

			Assert.AreEqual(2, vm.count, "Source property was queried multiple times on a reapply");
		}

		[Test]
		[Description("When there are multiple bindings, an update in one should not cause the other to udpate.")]
		public void BindingsShouldNotTriggerOtherBindings()
		{
			var vm = new ComplexMockViewModel();

			var bindable = new MockBindable();
			var qcbinding = new TypedBinding<ComplexMockViewModel, int>(cmvm => cmvm.QueryCount, null, null) { Mode = BindingMode.OneWay };
			var textBinding = new TypedBinding<ComplexMockViewModel, string>(cmvm => cmvm.Text, null, null) { Mode = BindingMode.OneWay };
			bindable.SetBinding(MultiplePropertyBindable.DoneProperty, qcbinding);
			bindable.SetBinding(MockBindable.TextProperty, textBinding);
			bindable.BindingContext = vm;

			Assert.AreEqual(1, vm.count);

			vm.Text = "update";

			Assert.AreEqual(1, vm.count, "Source property was queried due to a different binding update.");
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

		[Test]
		[Description("The most derived version of a property should always be called.")]
		public void MostDerviedPropertyOnContextSwitchOfSimilarType()
		{
			var vm = new MockViewModel { Text = "text" };

			var bindable = new MockBindable();
			bindable.BindingContext = vm;
			var binding = new TypedBinding<MockViewModel, string>(mvm => mvm.Text, (mvm, s) => mvm.Text = s, new[] {
				new Tuple<Func<MockViewModel, object>, string>(mvm=>mvm, "Text")
			});
			bindable.SetBinding(MockBindable.TextProperty, binding);

			Assert.AreEqual(vm.Text, bindable.GetValue(MockBindable.TextProperty));

			bindable.BindingContext = vm = new DerivedViewModel { Text = "text" };

			Assert.AreEqual(vm.Text, bindable.GetValue(MockBindable.TextProperty));
		}

		[Test]
		[Description("When binding with a multi-part path and part is null, no error should be thrown or logged")]
		public void ChainedPartNull()
		{
			var bindable = new MockBindable { BindingContext = new ComplexMockViewModel() };
			var binding = new TypedBinding<ComplexMockViewModel, string>(
			  cmvm => cmvm.Model.Text,
			  (cmvm, s) => cmvm.Model.Text = s, new[] {
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm, "Model"),
					new Tuple<Func<ComplexMockViewModel, object>, string>(cmvm=>cmvm.Model, "Text"),
				});

			Assert.That(() => bindable.SetBinding(MockBindable.TextProperty, binding), Throws.Nothing);
			Assert.That(log.Messages.Count, Is.EqualTo(0), "An error was logged");
		}

		[Test]
		public void SetBindingContextBeforeContextBindingAndInnerBindings()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			view.BindingContext = new Tuple<string, string>("Foo", "Bar");
			var bindingItem1 = new TypedBinding<Tuple<string, string>, string>(s => s.Item1, null, null);
			var bindingSelf = new TypedBinding<string, string>(s => s, null, null);
			label.SetBinding(BindableObject.BindingContextProperty, bindingItem1);
			label.SetBinding(Label.TextProperty, bindingSelf);

			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void SetBindingContextAndInnerBindingBeforeContextBinding()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			view.BindingContext = new Tuple<string, string>("Foo", "Bar");
			var bindingItem1 = new TypedBinding<Tuple<string, string>, string>(s => s.Item1, null, null);
			var bindingSelf = new TypedBinding<string, string>(s => s, null, null);
			label.SetBinding(Label.TextProperty, bindingSelf);
			label.SetBinding(BindableObject.BindingContextProperty, bindingItem1);

			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void SetBindingContextAfterContextBindingAndInnerBindings()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };
			var bindingItem1 = new TypedBinding<Tuple<string, string>, string>(s => s.Item1, null, null);
			var bindingSelf = new TypedBinding<string, string>(s => s, null, null);

			label.SetBinding(BindableObject.BindingContextProperty, bindingItem1);
			label.SetBinding(Label.TextProperty, bindingSelf);
			view.BindingContext = new Tuple<string, string>("Foo", "Bar");

			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void SetBindingContextAfterInnerBindingsAndContextBinding()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };
			var bindingItem1 = new TypedBinding<Tuple<string, string>, string>(s => s.Item1, null, null);
			var bindingSelf = new TypedBinding<string, string>(s => s, null, null);

			label.SetBinding(Label.TextProperty, bindingItem1);
			label.SetBinding(BindableObject.BindingContextProperty, bindingItem1);
			view.BindingContext = new Tuple<string, string>("Foo", "Bar");

			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void Convert()
		{
			var slider = new Slider();
			var vm = new MockViewModel { Text = "0.5" };
			slider.BindingContext = vm;
			slider.SetBinding(Slider.ValueProperty, new TypedBinding<MockViewModel, string>(mvm => mvm.Text, (mvm, s) => mvm.Text = s, null) { Mode = BindingMode.TwoWay });

			Assert.That(slider.Value, Is.EqualTo(0.5));

			slider.Value = 0.9;

			Assert.That(vm.Text, Is.EqualTo("0.9"));
		}

#if !WINDOWS_PHONE
		[TestCase("en-US"), TestCase("pt-PT")]
		public void ConvertIsCultureInvariant(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

			var slider = new Slider();
			var vm = new MockViewModel { Text = "0.5" };
			slider.BindingContext = vm;
			slider.SetBinding(Slider.ValueProperty, new TypedBinding<MockViewModel, string>(mvm => mvm.Text, (mvm, s) => mvm.Text = s, null) { Mode = BindingMode.TwoWay });

			Assert.That(slider.Value, Is.EqualTo(0.5));

			slider.Value = 0.9;

			Assert.That(vm.Text, Is.EqualTo("0.9"));
		}
#endif

		[Test]
		public void FailToConvert()
		{
			var slider = new Slider();
			slider.BindingContext = new ComplexMockViewModel { Model = new ComplexMockViewModel() };

			Assert.That(() =>
			{
				slider.SetBinding(Slider.ValueProperty, new TypedBinding<ComplexMockViewModel, ComplexMockViewModel>(mvm => mvm.Model, null, null));
			}, Throws.Nothing);

			Assert.That(slider.Value, Is.EqualTo(Slider.ValueProperty.DefaultValue));
			Assert.That(log.Messages.Count, Is.EqualTo(1), "No error logged");
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

		[TestCase(true)]
		[TestCase(false)]
		public void NullPropertyUpdatesAllBindings(bool useStringEmpty)
		{
			var vm = new NullViewModel();
			var bindable = new MockBindable2();
			bindable.BindingContext = vm;
			bindable.SetBinding(MockBindable.TextProperty, new TypedBinding<NullViewModel, string>(nvm => nvm.Foo, null, new[] {
				new Tuple<Func<NullViewModel, object>, string>(nvm=>nvm,"Foo")
			}));
			bindable.SetBinding(MockBindable2.Text2Property, new TypedBinding<NullViewModel, string>(nvm => nvm.Bar, null, new[] {
				new Tuple<Func<NullViewModel, object>, string>(nvm=>nvm,"Bar")
			}));

			vm.Foo = "Foo";
			vm.Bar = "Bar";
			Assert.That(() => vm.SignalAllPropertiesChanged(useNull: !useStringEmpty), Throws.Nothing);

			Assert.That(bindable.Text, Is.EqualTo("Foo"));
			Assert.That(bindable.Text2, Is.EqualTo("Bar"));
		}

		[TestCase]
		public void BindingSourceOverContext()
		{
			var label = new Label();
			label.BindingContext = "bindingcontext";
			var bindingSelf = new TypedBinding<string, string>(s => s, null, null);
			label.SetBinding(Label.TextProperty, bindingSelf);
			Assert.AreEqual("bindingcontext", label.Text);

			var bindingSelfSource = new TypedBinding<string, string>(s => s, null, null) { Source = "bindingsource" };
			label.SetBinding(Label.TextProperty, bindingSelfSource);
			Assert.AreEqual("bindingsource", label.Text);
		}

		class TestViewModel : INotifyPropertyChanged
		{
			event PropertyChangedEventHandler PropertyChanged;
			event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
			{
				add { PropertyChanged += value; }
				remove { PropertyChanged -= value; }
			}

			public string Foo { get; set; }

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

		[Test]
		public void BindingUnsubscribesForDeadTarget()
		{
			var viewmodel = new TestViewModel();

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
				button.SetBinding(Button.TextProperty, new TypedBinding<TestViewModel, string>(vm => vm.Foo, (vm, s) => vm.Foo = s, new[] {
					new Tuple<Func<TestViewModel, object>, string>(vm=>vm,"Foo")
				}));
				button.BindingContext = viewmodel;
			};

			create();

			Assume.That(viewmodel.InvocationListSize(), Is.EqualTo(1));

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			viewmodel.OnPropertyChanged("Foo");

			Assert.AreEqual(0, viewmodel.InvocationListSize());
		}

		[Test]
		public void BindingDoesNotStayAliveForDeadTarget()
		{
			var viewModel = new TestViewModel();
			WeakReference bindingRef = null;

			int i = 0;
			Action create = null;
			create = () =>
			{
				if (i++ < 1024)
				{
					create();
					return;
				}

				var binding = new TypedBinding<TestViewModel, string>(vm => vm.Foo, (vm, s) => vm.Foo = s, new[] {
					new Tuple<Func<TestViewModel, object>, string>(vm=>vm,"Foo")
				});
				var button = new Button();
				button.SetBinding(Button.TextProperty, binding);
				button.BindingContext = viewModel;

				bindingRef = new WeakReference(binding);
				binding = null;
			};

			create();

			Assume.That(viewModel.InvocationListSize(), Is.EqualTo(1));

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.IsFalse(bindingRef.IsAlive, "Binding should not be alive!");
		}

		[Test]
		public void BindingCreatesSingleSubscription()
		{
			TestViewModel viewmodel = new TestViewModel();
			var binding = new TypedBinding<TestViewModel, string>(vm => vm.Foo, (vm, s) => vm.Foo = s, new[] {
					new Tuple<Func<TestViewModel, object>, string>(vm=>vm,"Foo")
				});

			var button = new Button();
			button.SetBinding(Button.TextProperty, binding);
			button.BindingContext = viewmodel;

			Assert.That(viewmodel.InvocationListSize(), Is.EqualTo(1));
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

		[Test]
		public void IndexedViewModelPropertyChanged()
		{
			var label = new Label();
			var viewModel = new IndexedViewModel();

			var binding = new TypedBinding<Tuple<IndexedViewModel, object>, object>(
				vm => vm.Item1["Foo"],
				(vm, s) => vm.Item1["Foo"] = s,
				new[] {
					new Tuple<Func<Tuple<IndexedViewModel, object>, object>, string>(vm=>vm, "Item1"),
					new Tuple<Func<Tuple<IndexedViewModel, object>, object>, string>(vm=>vm.Item1, "Item[Foo]"),
				});

			label.BindingContext = new Tuple<IndexedViewModel, object>(viewModel, new object());
			label.SetBinding(Label.TextProperty, binding);
			Assert.AreEqual(null, label.Text);

			viewModel["Foo"] = "Baz";

			Assert.AreEqual("Baz", label.Text);
		}

		[Test]
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
			Assert.That(view.GetValue(bp1w), Is.EqualTo("foobar"));
			Assert.That(view.GetValue(bp1t), Is.EqualTo("foobar"));

			vm.Text = "qux";
			Assert.That(view.GetValue(bp1w), Is.EqualTo("qux"));
			Assert.That(view.GetValue(bp1t), Is.EqualTo("foobar"));
		}

		[Test]
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
			Assert.That(view.GetValue(bp1w), Is.EqualTo("foobar"));
			Assert.That(view.GetValue(bp1t), Is.EqualTo("foobar"));

			view.BindingContext = new MockViewModel("qux");
			Assert.That(view.GetValue(bp1w), Is.EqualTo("qux"));
			Assert.That(view.GetValue(bp1t), Is.EqualTo("qux"));
		}

		[Test]
		public void OneTimeBindingDoesntUpdateNeedSettersOrHandlers()
		{
			var view = new VisualElement();
			var bp1t = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			var vm = new MockViewModel("foobar");
			view.BindingContext = vm;

			var b1t = new TypedBinding<MockViewModel, string>(v => v.Text, null, null);

			view.SetBinding(bp1t, b1t);
			Assert.That(view.GetValue(bp1t), Is.EqualTo("foobar"));

			vm.Text = "qux";
			Assert.That(view.GetValue(bp1t), Is.EqualTo("foobar"));
			Assert.Pass(); //doesn't throw
		}

		[Test]
		[Ignore("SpeedTestApply")]
		public void SpeedTestApply()
		{

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var vm0 = new MockViewModel { Text = "Foo" };
			var vm1 = new MockViewModel { Text = "Bar" };
			var bindable = new MockBindable();

			var it = 100000;

			BindingBase binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => mvm.Text,
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
				binding.Apply(i % 2 == 0 ? vm0 : vm1, bindable, property);
				binding.Unapply();
			}
			swtb.Stop();
			Assert.AreEqual("Bar", bindable.GetValue(property));

			binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => mvm.Text,
				setter: (mvm, s) => mvm.Text = s,
				handlers: null);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var swtbh = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
			{
				binding.Apply(i % 2 == 0 ? vm0 : vm1, bindable, property);
				binding.Unapply();
			}
			swtbh.Stop();
			Assert.AreEqual("Bar", bindable.GetValue(property));

			binding = new Binding("Text");
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var swb = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
			{
				binding.Apply(i % 2 == 0 ? vm0 : vm1, bindable, property);
				binding.Unapply();
			}
			swb.Stop();
			Assert.AreEqual("Bar", bindable.GetValue(property));

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var swsv = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
				bindable.SetValue(property, (i % 2 == 0 ? vm0 : vm1).Text);
			swsv.Stop();
			Assert.AreEqual("Bar", bindable.GetValue(property));

			Assert.Fail($"Applying {it} Typedbindings\t\t\t: {swtb.ElapsedMilliseconds}ms.\nApplying {it} Typedbindings (without INPC)\t: {swtbh.ElapsedMilliseconds}ms.\nApplying {it} Bindings\t\t\t: {swb.ElapsedMilliseconds}ms.\nSetting  {it} values\t\t\t\t: {swsv.ElapsedMilliseconds}ms.");
		}

		[Test]
		[Ignore("SpeedTestSetBC")]
		public void SpeedTestSetBC()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var vm0 = new MockViewModel { Text = "Foo" };
			var vm1 = new MockViewModel { Text = "Bar" };
			var bindable = new MockBindable();

			var it = 100000;

			BindingBase binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => mvm.Text,
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
			//Assert.AreEqual("Bar", bindable.GetValue(property));

			binding = new TypedBinding<MockViewModel, string>(
				getter: mvm => mvm.Text,
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
			Assert.AreEqual("Bar", bindable.GetValue(property));

			binding = new Binding("Text");
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			bindable.SetBinding(property, binding);
			var swb = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
				bindable.BindingContext = i % 2 == 0 ? vm0 : vm1;
			swb.Stop();
			Assert.AreEqual("Bar", bindable.GetValue(property));

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			bindable.SetBinding(property, binding);
			var swsv = Stopwatch.StartNew();
			for (var i = 0; i < it; i++)
				bindable.SetValue(property, (i % 2 == 0 ? vm0 : vm1).Text);
			swsv.Stop();
			Assert.AreEqual("Bar", bindable.GetValue(property));

			Assert.Fail($"Setting BC for {it} Typedbindings\t\t\t: {swtb.ElapsedMilliseconds}ms.\nSetting BC for {it} Typedbindings (without INPC)\t: {swtbh.ElapsedMilliseconds}ms.\nSetting BC for {it} Bindings\t\t\t\t: {swb.ElapsedMilliseconds}ms.\nSetting  {it} values\t\t\t\t\t: {swsv.ElapsedMilliseconds}ms.");
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

		[Test]
		//https://github.com/xamarin/Xamarin.Forms/issues/3650
		//https://github.com/xamarin/Xamarin.Forms/issues/3613
		public void TypedBindingsShouldNotHang()
		{
			var typedBinding = new TypedBinding<VM3650, string>(
				vm => vm.Title,
				(vm, s) => vm.Title = s,
				new Tuple<Func<VM3650, object>, string>[] {
					new Tuple<Func<VM3650, object>, string>(vm=>vm, "Title")
				});
			var vm3650 = new VM3650();
			var label = new Label();
			label.SetBinding(Label.TextProperty, typedBinding);
			label.BindingContext = vm3650;

			Assert.That(label.Text, Is.EqualTo("default"));
			Assert.That(vm3650.Count, Is.EqualTo(1));

			vm3650.Count = 0;
			vm3650.Title = "foo";
			Assert.That(label.Text, Is.EqualTo("foo"));
			Assert.That(vm3650.Count, Is.EqualTo(1));

			vm3650.Count = 0;
			vm3650.Title = "bar";
			Assert.That(label.Text, Is.EqualTo("bar"));
			Assert.That(vm3650.Count, Is.EqualTo(1));

			vm3650.Count = 0;
			vm3650.Title = "baz";
			Assert.That(label.Text, Is.EqualTo("baz"));
			Assert.That(vm3650.Count, Is.EqualTo(1));

			vm3650.Count = 0;
			vm3650.Title = "qux";
			Assert.That(label.Text, Is.EqualTo("qux"));
			Assert.That(vm3650.Count, Is.EqualTo(1));

		}
	}
}
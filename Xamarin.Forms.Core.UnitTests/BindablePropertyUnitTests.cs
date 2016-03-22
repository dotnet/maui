using System;
using System.Linq;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class BindablePropertyUnitTests : BaseTestFixture
	{
		[Test]
		public void Create()
		{
			const BindingMode mode = BindingMode.OneWayToSource;
			const string dvalue = "default";
			BindableProperty.CoerceValueDelegate<string> coerce = (bindable, value) => value;
			BindableProperty.ValidateValueDelegate<string> validate = (b, v) => true;
			BindableProperty.BindingPropertyChangedDelegate<string> changed = (b, ov, nv) => { };
			BindableProperty.BindingPropertyChangingDelegate<string> changing = (b, ov, nv) => { };

			var prop = BindableProperty.Create<Button, string> (b => b.Text, dvalue, mode, validate, changed, changing, coerce);
			Assert.AreEqual ("Text", prop.PropertyName);
			Assert.AreEqual (typeof (Button), prop.DeclaringType);
			Assert.AreEqual (typeof (string), prop.ReturnType);
			Assert.AreEqual (dvalue, prop.DefaultValue);
			Assert.AreEqual (mode, prop.DefaultBindingMode);
		}

		[Test]
		public void CreateWithDefaultMode ()
		{
			const BindingMode mode = BindingMode.Default;
			var prop = BindableProperty.Create<Button, string> (b => b.Text, null, defaultBindingMode: mode);
			Assert.AreEqual (BindingMode.OneWay, prop.DefaultBindingMode);
		}

		[Test]
		public void CreateCasted()
		{
			var prop = BindableProperty.Create<Cell, bool> (c => c.IsEnabled, true);

			Assert.AreEqual ("IsEnabled", prop.PropertyName);
			Assert.AreEqual (typeof (Cell), prop.DeclaringType);
			Assert.AreEqual (typeof (bool), prop.ReturnType);
		}

		[Test]
		public void CreateNonGeneric()
		{
			const BindingMode mode = BindingMode.OneWayToSource;
			const string dvalue = "default";
			BindableProperty.CoerceValueDelegate coerce = (bindable, value) => value;
			BindableProperty.ValidateValueDelegate validate = (b, v) => true;
			BindableProperty.BindingPropertyChangedDelegate changed = (b, ov, nv) => { };
			BindableProperty.BindingPropertyChangingDelegate changing = (b, ov, nv) => { };

			var prop = BindableProperty.Create ("Text", typeof(string), typeof(Button), dvalue, mode, validate, changed, changing, coerce);
			Assert.AreEqual ("Text", prop.PropertyName);
			Assert.AreEqual (typeof (Button), prop.DeclaringType);
			Assert.AreEqual (typeof (string), prop.ReturnType);
			Assert.AreEqual (dvalue, prop.DefaultValue);
			Assert.AreEqual (mode, prop.DefaultBindingMode);
		}

		class GenericView<T> : View
		{
			public string Text
			{
				get;
				set;
			}
		}

		[Test]
		public void CreateForGeneric()
		{
			const BindingMode mode = BindingMode.OneWayToSource;
			const string dvalue = "default";
			BindableProperty.CoerceValueDelegate coerce = (bindable, value) => value;
			BindableProperty.ValidateValueDelegate validate = (b, v) => true;
			BindableProperty.BindingPropertyChangedDelegate changed = (b, ov, nv) => { };
			BindableProperty.BindingPropertyChangingDelegate changing = (b, ov, nv) => { };

			var prop = BindableProperty.Create ("Text", typeof(string), typeof(GenericView<>), dvalue, mode, validate, changed, changing, coerce);
			Assert.AreEqual ("Text", prop.PropertyName);
			Assert.AreEqual (typeof (GenericView<>), prop.DeclaringType);
			Assert.AreEqual (typeof (string), prop.ReturnType);
			Assert.AreEqual (dvalue, prop.DefaultValue);
			Assert.AreEqual (mode, prop.DefaultBindingMode);
		}

		[Test]
		public void ChangingBeforeChanged ()
		{
			bool changingfired = false;
			bool changedfired = false;
			BindableProperty.BindingPropertyChangedDelegate<string> changed = (b, ov, nv) => { 
				Assert.True (changingfired);
				changedfired = true;
			};
			BindableProperty.BindingPropertyChangingDelegate<string> changing = (b, ov, nv) => { 
				Assert.False (changedfired);
				changingfired  = true;
			};

			var prop = BindableProperty.Create<Button, string> (b => b.Text, "Foo", 
				propertyChanging: changing, 
				propertyChanged: changed);

			Assert.False (changingfired);
			Assert.False (changedfired);

			(new View ()).SetValue (prop, "Bar");

			Assert.True (changingfired);
			Assert.True (changedfired);
		}

		[Test]
		public void NullableProperty ()
		{
			var prop = BindableProperty.Create ("foo", typeof(DateTime?), typeof(MockBindable), null);
			Assert.AreEqual (typeof(DateTime?), prop.ReturnType);

			var bindable = new MockBindable ();
			Assert.AreEqual (null, bindable.GetValue (prop));

			var now = DateTime.Now;
			bindable.SetValue (prop, now);
			Assert.AreEqual (now, bindable.GetValue (prop));

			bindable.SetValue (prop, null);
			Assert.AreEqual (null, bindable.GetValue (prop));
		}
	}
}
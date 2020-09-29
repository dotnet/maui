using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DynamicBindingContextTests
	{
		[Test]
		public void BindingTwoWayToDynamicModel()
		{
			var view = new MockBindable();
			var model = new DynamicModel
			{
				Properties =
				{
					{ "Title", "Foo" },
				}
			};

			view.SetBinding(MockBindable.TextProperty, "Title");
			view.BindingContext = model;

			Assert.AreEqual("Foo", view.Text);

			view.Text = "Bar";

			Assert.AreEqual("Bar", model.Properties["Title"]);
		}

		// This whole class and inner types is just a very simple 
		// dictionary-based dynamic model that proves that the 
		// approach works. It implements just the bare minimum of 
		// the base types to get our bindings to work properly and 
		// pass the tests.
		class DynamicModel : IReflectableType
		{
			public DynamicModel()
			{
				Properties = new Dictionary<string, object>();
			}

			public TypeInfo GetTypeInfo()
			{
				return new DynamicTypeInfo(Properties);
			}

			public IDictionary<string, object> Properties { get; private set; }

			class DynamicTypeInfo : TypeDelegator
			{
				IDictionary<string, object> properties;

				public DynamicTypeInfo(IDictionary<string, object> properties)
					: base(typeof(object))
				{
					this.properties = properties;
				}

				public override PropertyInfo GetDeclaredProperty(string name)
				{
					if (!properties.ContainsKey(name))
						return null;

					return new DynamicPropertyInfo(properties, name);
				}

				internal class DynamicPropertyInfo : PropertyInfo
				{
					IDictionary<string, object> properties;
					string name;

					public DynamicPropertyInfo(IDictionary<string, object> properties, string name)
					{
						this.properties = properties;
						this.name = name;
					}

					public override bool CanRead
					{
						get { return true; }
					}

					public override bool CanWrite
					{
						get { return true; }
					}

					public override MethodInfo GetGetMethod(bool nonPublic)
					{
						return new DynamicPropertyGetterInfo(this, properties);
					}

					public override MethodInfo GetSetMethod(bool nonPublic)
					{
						return new DynamicPropertySetterInfo(this, properties);
					}

					public override Type PropertyType
					{
						get { return properties[name].GetType(); }
					}

					public override string Name
					{
						get { return name; }
					}

					public override PropertyAttributes Attributes
					{
						get { return PropertyAttributes.None; }
					}

					public override MethodInfo[] GetAccessors(bool nonPublic)
					{
						return new[] { GetGetMethod(nonPublic), GetSetMethod(nonPublic) };
					}

					public override ParameterInfo[] GetIndexParameters()
					{
						return new ParameterInfo[0];
					}

					public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
					{
						throw new NotImplementedException();
					}

					public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
					{
						throw new NotImplementedException();
					}

					public override Type DeclaringType
					{
						get { throw new NotImplementedException(); }
					}

					public override object[] GetCustomAttributes(Type attributeType, bool inherit)
					{
						throw new NotImplementedException();
					}

					public override object[] GetCustomAttributes(bool inherit)
					{
						throw new NotImplementedException();
					}

					public override bool IsDefined(Type attributeType, bool inherit)
					{
						throw new NotImplementedException();
					}

					public override Type ReflectedType
					{
						get { throw new NotImplementedException(); }
					}
				}

				internal class DynamicPropertyGetterInfo : DynamicPropertyMethodInfo
				{
					public DynamicPropertyGetterInfo(PropertyInfo property, IDictionary<string, object> properties)
						: base(property, properties)
					{
					}

					public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
					{
						return Properties[Property.Name];
					}

					public override ParameterInfo[] GetParameters()
					{
						return new[] { new DynamicParameterInfo(Property, Property.PropertyType, "value") };
					}

					public override Type ReturnType
					{
						get { return Property.PropertyType; }
					}
				}

				internal class DynamicPropertySetterInfo : DynamicPropertyMethodInfo
				{
					public DynamicPropertySetterInfo(PropertyInfo property, IDictionary<string, object> properties)
						: base(property, properties)
					{
					}

					public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
					{
						Properties[Property.Name] = parameters[0];
						return null;
					}

					public override ParameterInfo[] GetParameters()
					{
						return new[] {
							new DynamicParameterInfo (Property, typeof(DynamicTypeInfo), "this"),
							new DynamicParameterInfo (Property, Property.PropertyType, "value")
						};
					}

					public override Type ReturnType
					{
						get { return typeof(void); }
					}
				}

				internal abstract class DynamicPropertyMethodInfo : MethodInfo
				{
					public DynamicPropertyMethodInfo(PropertyInfo property, IDictionary<string, object> properties)
					{
						Property = property;
						Properties = properties;
					}

					protected PropertyInfo Property { get; private set; }

					protected IDictionary<string, object> Properties { get; private set; }

					public override MethodInfo GetBaseDefinition()
					{
						return null;
					}

					public override ICustomAttributeProvider ReturnTypeCustomAttributes
					{
						get { return null; }
					}

					public override MethodAttributes Attributes
					{
						get { return MethodAttributes.Public; }
					}

					public override MethodImplAttributes GetMethodImplementationFlags()
					{
						return MethodImplAttributes.IL;
					}

					public override ParameterInfo[] GetParameters()
					{
						return new ParameterInfo[0];
					}

					public override RuntimeMethodHandle MethodHandle
					{
						get { throw new NotImplementedException(); }
					}

					public override Type DeclaringType
					{
						get { return typeof(DynamicModel); }
					}

					public override object[] GetCustomAttributes(Type attributeType, bool inherit)
					{
						return new object[0];
					}

					public override object[] GetCustomAttributes(bool inherit)
					{
						return new object[0];
					}

					public override bool IsDefined(Type attributeType, bool inherit)
					{
						return false;
					}

					public override string Name
					{
						get { return Property.Name; }
					}

					public override Type ReflectedType
					{
						get { return typeof(DynamicModel); }
					}
				}

				internal class DynamicParameterInfo : ParameterInfo
				{
					MemberInfo member;
					Type type;

					public DynamicParameterInfo(MemberInfo member, Type type, string name)
					{
						this.member = member;
						this.type = type;
					}

					public override MemberInfo Member
					{
						get { return member; }
					}

					public override Type ParameterType
					{
						get { return type; }
					}
				}
			}

		}
	}
}
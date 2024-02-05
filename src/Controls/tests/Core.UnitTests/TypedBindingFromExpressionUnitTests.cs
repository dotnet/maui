using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class TypedBindingFromExpressionUnitTests : BindingBaseUnitTests
	{

		public TypedBindingFromExpressionUnitTests()
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
			return TypedBindingFactory.Create(static (MockViewModel mvm) => mvm.Text, mode: mode, stringFormat: stringFormat);
		}

		[Theory, Category("[Binding] Expressions")]
		[InlineData(true)]
		[InlineData(false)]
		public void AllowsExpressionWithCast(bool useCStyleCasts)
		{
			var viewmodel = new ObjectValueViewModel
			{
				Value = new ObjectValueViewModel
				{
					Value = new object(),
				}
			};

			var property = BindableProperty.Create("Foo", typeof(object), typeof(MockBindable), null, BindingMode.TwoWay);
			var binding = useCStyleCasts
				? TypedBindingFactory.Create(static (ObjectValueViewModel vm) => ((ObjectValueViewModel)vm.Value).Value) 
				: TypedBindingFactory.Create(static (ObjectValueViewModel vm) => (vm.Value as ObjectValueViewModel).Value);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			Assert.Same(((ObjectValueViewModel)viewmodel.Value).Value, bindable.GetValue(property));

			var newValue = new ObjectValueViewModel { Value = new object() };
			bindable.SetValue(property, newValue);

			Assert.Same(newValue, (viewmodel.Value as ObjectValueViewModel).Value);
		}

		private record ObjectValueViewModel : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			private object _value;
			public object Value
			{
				get => _value;
				set
				{
					_value = value;
					OnPropertyChanged();
				}
			}

			protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}

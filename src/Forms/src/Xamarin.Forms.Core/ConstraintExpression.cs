using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	public class ConstraintExpression : IMarkupExtension<Constraint>
	{
		public ConstraintExpression()
		{
			Factor = 1.0;
		}

		public double Constant { get; set; }

		public string ElementName { get; set; }

		public double Factor { get; set; }

		public string Property { get; set; }

		public ConstraintType Type { get; set; }

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<Constraint>).ProvideValue(serviceProvider);
		}

		public Constraint ProvideValue(IServiceProvider serviceProvider)
		{
			MethodInfo minfo;
			switch (Type)
			{
				default:
				case ConstraintType.RelativeToParent:
					if (string.IsNullOrEmpty(Property))
						return null;
					minfo = typeof(View).GetProperties().First(pi => pi.Name == Property && pi.CanRead && pi.GetMethod.IsPublic).GetMethod;
					return Constraint.RelativeToParent(p => (double)minfo.Invoke(p, new object[] { }) * Factor + Constant);
				case ConstraintType.Constant:
					return Constraint.Constant(Constant);
				case ConstraintType.RelativeToView:
					if (string.IsNullOrEmpty(Property))
						return null;
					if (string.IsNullOrEmpty(ElementName))
						return null;
					minfo = typeof(View).GetProperties().First(pi => pi.Name == Property && pi.CanRead && pi.GetMethod.IsPublic).GetMethod;
					var referenceProvider = serviceProvider.GetService<IReferenceProvider>();

					View view;
					if (referenceProvider != null)
						view = (View)referenceProvider.FindByName(ElementName);
					else
					{ //legacy path
						var valueProvider = serviceProvider.GetService<IProvideValueTarget>();
						if (valueProvider == null || !(valueProvider.TargetObject is INameScope))
							return null;
						view = ((INameScope)valueProvider.TargetObject).FindByName<View>(ElementName);
					}
					return Constraint.RelativeToView(view, delegate (RelativeLayout p, View v)
					{ return (double)minfo.Invoke(v, new object[] { }) * Factor + Constant; });
			}
		}
	}
}
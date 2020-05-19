using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5680, "[Enhancement] Add method to force value coercion")]
	public partial class Issue5680 : TestContentPage
    {
		
		public Issue5680()
        {
            InitializeComponent();
        }

		protected override void Init()
		{

		}

		public static readonly BindableProperty AngleProperty = BindableProperty.Create("Angle", typeof(double), typeof(Issue5680), 0.0, coerceValue: CoerceAngle);
		public static readonly BindableProperty MaximumAngleProperty = BindableProperty.Create("MaximumAngle", typeof(double), typeof(Issue5680), 360.0, propertyChanged: ForceCoerceValue);

		public double Angle
		{
			get { return (double)GetValue(AngleProperty); }
			set { SetValue(AngleProperty, value); }
		}

		public double MaximumAngle
		{
			get { return (double)GetValue(MaximumAngleProperty); }
			set { SetValue(MaximumAngleProperty, value); }
		}

		static object CoerceAngle(BindableObject bindable, object value)
		{
			var homePage = bindable as Issue5680;
			double input = (double)value;

			if (input > homePage.MaximumAngle)
			{
				input = homePage.MaximumAngle;
			}
			return input;
		}

		static void ForceCoerceValue(BindableObject bindable, object oldValue, object newValue)
		{
			bindable.CoerceValue(AngleProperty);
		}
	}
}
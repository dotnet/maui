using System;

namespace Xamarin.Forms.StyleSheets
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
	sealed class StylePropertyAttribute : Attribute
	{
		public string CssPropertyName { get; }
		public string BindablePropertyName { get; }
		public Type TargetType { get; }
		public Type PropertyOwnerType { get; set; }
		public BindableProperty BindableProperty { get; set; }
		public bool Inherited { get; set; } = false;


		public StylePropertyAttribute(string cssPropertyName, Type targetType, string bindablePropertyName)
		{
			CssPropertyName = cssPropertyName;
			BindablePropertyName = bindablePropertyName;
			TargetType = targetType;
		}
	}
}
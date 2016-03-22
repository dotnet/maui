using System;
using System.Diagnostics;

namespace Xamarin.Forms.CustomAttributes
{
	[Conditional ("DEBUG")]
	[AttributeUsage (
		AttributeTargets.Class |
		AttributeTargets.Event |
		AttributeTargets.Property |
		AttributeTargets.Method |
		AttributeTargets.Delegate,
		AllowMultiple = true
		)]
	public class UiTestAttribute : Attribute
	{
		public UiTestAttribute (Type formsType)
		{
			Type = formsType;
			MemberName = "";
		}

		public UiTestAttribute (Type formsType, string memberName)
		{
			Type = formsType;
			MemberName = memberName;
		}

		public Type Type { get; }

		public string MemberName { get; }
	}
}

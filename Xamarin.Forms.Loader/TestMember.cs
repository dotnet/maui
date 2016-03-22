using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Loader
{
	internal sealed class TestMember : ILoaderElement
	{
		readonly IEnumerable<CategoryAttribute> categoryAttributes;

		readonly IEnumerable<UiTestAttribute> uiTestAttributes;

		public TestMember(TestType uiTestType, MemberInfo memberInfo)
		{
			DeclaringType = uiTestType;
			MemberInfo = memberInfo;

			// handle public overrides that inherit attributes
			uiTestAttributes = memberInfo.GetCustomAttributes<UiTestAttribute>();
			categoryAttributes = memberInfo.GetCustomAttributes<CategoryAttribute>();
		}

		internal FormsLoader FormsLoader
		{
			get { return DeclaringType.FormsLoader; }
		}

		public TestType DeclaringType { get; }

		public MemberInfo MemberInfo { get; }

		public IEnumerable<UiTestAttribute> UiTestAttributes()
		{
			return uiTestAttributes;
		}

		public IEnumerable<CategoryAttribute> CategoryAttributes()
		{
			return categoryAttributes;
		}
	}
}
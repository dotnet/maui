using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Xamarin.Forms.Loader
{
	internal sealed class TestType : ILoaderElement
	{
		readonly IEnumerable<CategoryAttribute> _categories;
		readonly IEnumerable<TestMember> _members;

		internal TestType(FormsLoader formsLoader, Type type)
		{
			FormsLoader = formsLoader;
			Type = type;

			_members =
				from o in Type.GetMethods()
				where !o.IsSetupOrTearDown() && !o.IsEqualityOverride() && !o.IsToStringOverride()
				select new TestMember(this, o);

			_categories = type.GetCustomAttributes<CategoryAttribute>();
		}

		public FormsLoader FormsLoader { get; }

		public Type Type { get; }

		public IEnumerable<TestMember> Members()
		{
			return _members;
		}

		public IEnumerable<CategoryAttribute> Categories()
		{
			return _categories;
		}
	}
}
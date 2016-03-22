using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms.Loader
{
	internal sealed class FormsType : ILoaderElement
	{
		readonly IEnumerable<FormsMember> formsMembers;

		internal FormsType(FormsLoader formsLoader, Type type)
		{
			FormsLoader = formsLoader;
			Type = type;

			const BindingFlags bindingFlags = BindingFlags.DeclaredOnly |
			                                  BindingFlags.Public |
			                                  BindingFlags.Instance |
			                                  BindingFlags.Static;

			const MemberTypes memberTypes = MemberTypes.Event |
			                                MemberTypes.Method |
			                                MemberTypes.Property;

			formsMembers =
				from o in Type.GetMember("*", memberTypes, bindingFlags)
				where
					o.IsPublic() &&
					!o.IsCompilerGenerated() &&
					!o.IsEqualityOverride() &&
					!o.IsToStringOverride() &&
					!LoaderExtensions.IsUnitTested(type, o)
				select new FormsMember(this, o);
		}

		public FormsLoader FormsLoader { get; }

		public Type Type { get; }

		public IEnumerable<FormsMember> Members()
		{
			return formsMembers;
		}
	}
}
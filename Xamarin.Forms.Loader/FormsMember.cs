using System.Reflection;

namespace Xamarin.Forms.Loader
{
	internal sealed class FormsMember : ILoaderElement
	{
		internal FormsMember(FormsType formsType, MemberInfo memberInfo)
		{
			FormsType = formsType;
			MemberInfo = memberInfo;
		}

		public FormsType FormsType { get; }

		public MemberInfo MemberInfo { get; }
	}
}
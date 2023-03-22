#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/DynamicResource.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.DynamicResource']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DynamicResource
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/DynamicResource.xml" path="//Member[@MemberName='Key']/Docs/*" />
		public string Key { get; private set; }
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/DynamicResource.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DynamicResource(string key) => Key = key;
	}
}
#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/QueryPropertyAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.QueryPropertyAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class QueryPropertyAttribute : Attribute
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/QueryPropertyAttribute.xml" path="//Member[@MemberName='Name']/Docs/*" />
		public string Name { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/QueryPropertyAttribute.xml" path="//Member[@MemberName='QueryId']/Docs/*" />
		public string QueryId { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/QueryPropertyAttribute.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public QueryPropertyAttribute(string name, string queryId)
		{
			Name = name;
			QueryId = queryId;
		}
	}
}

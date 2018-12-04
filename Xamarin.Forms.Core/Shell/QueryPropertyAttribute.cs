using System;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Class)]
	public class QueryPropertyAttribute : Attribute
	{
		public string Name { get;  }
		public string QueryId { get; }

		public QueryPropertyAttribute(string name, string queryId)
		{
			Name = name;
			QueryId = queryId;
		}
	}
}
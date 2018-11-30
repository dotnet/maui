using System;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Class)]
	public class QueryPropertyAttribute : Attribute
	{
		public string Name { get; private set; }

		public string QueryId { get; private set; }

		public QueryPropertyAttribute(string name, string queryId)
		{
			Name = name;
			QueryId = queryId;
		}
	}
}
#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	internal class ShellRouteParameters : Dictionary<string, object>
	{
		Dictionary<string, string> _queryStringParameters = new Dictionary<string, string>();

		public ShellRouteParameters()
		{
		}

		public ShellRouteParameters(ShellRouteParameters shellRouteParams) : base(shellRouteParams)
		{
			foreach (var queryParams in shellRouteParams._queryStringParameters)
			{
				_queryStringParameters[queryParams.Key] = queryParams.Value;
			}
		}

		internal ShellRouteParameters(ShellRouteParameters query, string prefix)
			: base(query.Count)
		{
			foreach (var q in query)
			{
				if (!q.Key.StartsWith(prefix, StringComparison.Ordinal))
					continue;
				var key = q.Key.Substring(prefix.Length);
				if (key.IndexOf(".", StringComparison.Ordinal) != -1)
					continue;
				this.Add(key, q.Value);

				if (query._queryStringParameters.ContainsKey(key))
					_queryStringParameters.Add(key, query._queryStringParameters[key]);
			}
		}

		internal ShellRouteParameters(IDictionary<string, object> shellRouteParams) : base(shellRouteParams)
		{
			if (shellRouteParams is ShellRouteParameters shellRoute)
			{
				foreach (var queryParams in shellRoute._queryStringParameters)
				{
					_queryStringParameters[queryParams.Key] = queryParams.Value;
				}
			}
		}

		internal void ResetToQueryParameters()
		{
			this.Clear();
			foreach (var queryParam in _queryStringParameters)
			{
				this[queryParam.Key] = queryParam.Value;
			}
		}

		internal void SetQueryStringParameters(string query)
		{
			_queryStringParameters = ParseQueryString(query);
			if (_queryStringParameters == null || _queryStringParameters.Count == 0)
				return;

			foreach (var item in _queryStringParameters)
			{
				if (!this.ContainsKey(item.Key))
					this[item.Key] = item.Value;
			}
		}

		static Dictionary<string, string> ParseQueryString(string query)
		{
			if (query.StartsWith("?", StringComparison.Ordinal))
				query = query.Substring(1);
			Dictionary<string, string> lookupDict = new(StringComparer.Ordinal);
			if (query == null)
				return lookupDict;
			foreach (var part in query.Split('&'))
			{
				var p = part.Split('=');
				if (p.Length != 2)
					continue;
				lookupDict[p[0]] = p[1];
			}

			return lookupDict;
		}
	}


	internal static class ShellParameterExtensions
	{
		public static void Deconstruct(this KeyValuePair<string, object> tuple, out string key, out object value)
		{
			key = tuple.Key;
			value = tuple.Value;
		}
	}
}
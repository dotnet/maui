#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	internal class ShellRouteParameters : Dictionary<string, object>
	{
		ShellNavigationQueryParameters _singleUseQueryParameters;

		public ShellRouteParameters()
		{
		}

		public ShellRouteParameters(ShellRouteParameters shellRouteParams) : base(shellRouteParams)
		{
			_singleUseQueryParameters = shellRouteParams._singleUseQueryParameters;
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
			}

			_singleUseQueryParameters = query._singleUseQueryParameters;
		}

		internal ShellRouteParameters(IDictionary<string, object> shellRouteParams) : base(shellRouteParams)
		{
		}

		internal ShellRouteParameters(ShellNavigationQueryParameters singleUseQueryParameters)
		{
			foreach (var item in singleUseQueryParameters)
				this.Add(item.Key, item.Value);

			_singleUseQueryParameters = singleUseQueryParameters;
		}

		internal void ResetToQueryParameters()
		{
			if (_singleUseQueryParameters is null)
				return;

			foreach (var item in _singleUseQueryParameters)
			{
				if (this.ContainsKey(item.Key))
				{
					this.Remove(item.Key);
				}
			}

			_singleUseQueryParameters = null;
		}

		internal void SetQueryStringParameters(string query)
		{
			var queryStringParameters = ParseQueryString(query);
			if (queryStringParameters == null || queryStringParameters.Count == 0)
				return;

			foreach (var item in queryStringParameters)
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
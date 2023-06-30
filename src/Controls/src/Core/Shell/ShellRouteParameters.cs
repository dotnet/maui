#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	internal class ShellRouteParameters : Dictionary<string, object>
	{
		KeyValuePair<string, object>? _singleUseQueryParameter;

		public ShellRouteParameters()
		{
		}

		public ShellRouteParameters(ShellRouteParameters shellRouteParams) : base(shellRouteParams)
		{
			_singleUseQueryParameter = shellRouteParams._singleUseQueryParameter;
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

			_singleUseQueryParameter = query._singleUseQueryParameter;
		}

		internal ShellRouteParameters(IDictionary<string, object> shellRouteParams) : base(shellRouteParams)
		{
		}

		internal ShellRouteParameters(KeyValuePair<string, object> singleUseQueryParameter)
		{
			this.Add(singleUseQueryParameter.Key, singleUseQueryParameter.Value);
			_singleUseQueryParameter = singleUseQueryParameter;
		}

		internal void ResetToQueryParameters()
		{
			if (_singleUseQueryParameter is null)
				return;

			if (this.ContainsKey(_singleUseQueryParameter.Value.Key))
			{
				this.Remove(_singleUseQueryParameter.Value.Key);
				_singleUseQueryParameter = null;
			}
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
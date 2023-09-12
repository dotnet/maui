#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	internal class ShellRouteParameters : Dictionary<string, object>
	{
		readonly ShellNavigationQueryParameters _shellNavigationQueryParameters =
			new ShellNavigationQueryParameters();

		public ShellRouteParameters()
		{
		}

		public ShellRouteParameters(ShellRouteParameters shellRouteParams) : base(shellRouteParams)
		{
			foreach (var item in shellRouteParams._shellNavigationQueryParameters)
				_shellNavigationQueryParameters[item.Key] = item.Value;
		}

		internal IDictionary<string, object> ToReadOnlyIfUsingShellNavigationQueryParameters()
		{
			if (_shellNavigationQueryParameters.Count > 0)
			{
				var returnValue = new ShellNavigationQueryParameters(_shellNavigationQueryParameters);

				foreach (var item in this)
				{
					if (!returnValue.ContainsKey(item.Key))
						returnValue.Add(item.Key, item.Value);
				}

				return returnValue.SetToReadOnly();
			}

			return this;
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

			foreach (var item in query._shellNavigationQueryParameters)
				_shellNavigationQueryParameters[item.Key] = item.Value;
		}

		internal ShellRouteParameters(IDictionary<string, object> shellRouteParams) : base(shellRouteParams)
		{
		}

		internal ShellRouteParameters(ShellNavigationQueryParameters shellNavigationQueryParameterss)
		{
			foreach (var item in shellNavigationQueryParameterss)
				this.Add(item.Key, item.Value);

			foreach (var item in shellNavigationQueryParameterss)
				_shellNavigationQueryParameters[item.Key] = item.Value;
		}

		internal void ResetToQueryParameters()
		{
			if (_shellNavigationQueryParameters.Count == 0)
				return;

			foreach (var item in _shellNavigationQueryParameters)
			{
				if (this.ContainsKey(item.Key))
				{
					this.Remove(item.Key);
				}
			}

			_shellNavigationQueryParameters.Clear();
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
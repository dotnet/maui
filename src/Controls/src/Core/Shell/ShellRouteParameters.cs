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
		readonly HashSet<string> _shellContentQueryParameterNames =
			new HashSet<string>(StringComparer.Ordinal);
		readonly HashSet<string> _shellContentQueryStringParameterNames =
			new HashSet<string>(StringComparer.Ordinal);

		public ShellRouteParameters()
		{
		}

		public ShellRouteParameters(ShellRouteParameters shellRouteParams) : base(shellRouteParams)
		{
			foreach (var item in shellRouteParams._shellNavigationQueryParameters)
				_shellNavigationQueryParameters[item.Key] = item.Value;

			foreach (var item in shellRouteParams._shellContentQueryParameterNames)
				_shellContentQueryParameterNames.Add(item);

			foreach (var item in shellRouteParams._shellContentQueryStringParameterNames)
				_shellContentQueryStringParameterNames.Add(item);
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

			foreach (var item in query._shellContentQueryParameterNames)
			{
				if (!item.StartsWith(prefix, StringComparison.Ordinal))
					continue;

				var key = item.Substring(prefix.Length);
				if (key.IndexOf(".", StringComparison.Ordinal) == -1)
					_shellContentQueryParameterNames.Add(key);
			}

			foreach (var item in query._shellContentQueryStringParameterNames)
			{
				if (!item.StartsWith(prefix, StringComparison.Ordinal))
					continue;

				var key = item.Substring(prefix.Length);
				if (key.IndexOf(".", StringComparison.Ordinal) == -1)
					_shellContentQueryStringParameterNames.Add(key);
			}

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
			var queryStringParameters = ParseQueryString(query.AsSpan());
			if (queryStringParameters == null || queryStringParameters.Count == 0)
				return;

			foreach (var item in queryStringParameters)
			{
				if (!this.ContainsKey(item.Key))
					this[item.Key] = item.Value;
			}
		}

		internal void SetShellContentQueryParameter(string name, object value)
		{
			this[name] = value;
			_shellNavigationQueryParameters.Remove(name);
			_shellContentQueryStringParameterNames.Remove(name);
			_shellContentQueryParameterNames.Add(name);
		}

		internal void SetShellContentQueryStringParameter(string name, object value)
		{
			this[name] = value;
			_shellNavigationQueryParameters.Remove(name);
			_shellContentQueryParameterNames.Remove(name);
			_shellContentQueryStringParameterNames.Add(name);
		}

		internal void RemoveShellContentQueryParameter(string name)
		{
			Remove(name);
			_shellNavigationQueryParameters.Remove(name);
			_shellContentQueryParameterNames.Remove(name);
			_shellContentQueryStringParameterNames.Remove(name);
		}

		internal bool IsShellContentQueryParameter(string name) =>
			_shellContentQueryParameterNames.Contains(name);

		internal bool IsShellContentQueryStringParameter(string name) =>
			_shellContentQueryStringParameterNames.Contains(name);

		internal bool IsShellContentParameter(string name) =>
			IsShellContentQueryParameter(name) || IsShellContentQueryStringParameter(name);

		static Dictionary<string, string> ParseQueryString(ReadOnlySpan<char> query)
		{
			if (query.Length > 0 && query[0] == '?')
				query = query.Slice(1);

			Dictionary<string, string> lookupDict = new(StringComparer.Ordinal);

			WebUtils.UnpackParameters(query, lookupDict);

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
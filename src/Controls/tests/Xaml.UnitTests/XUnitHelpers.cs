using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	/// <summary>
	/// Provides test data for all values of the XamlInflator enum.
	/// This replaces NUnit's [Values] attribute for XamlInflator parameters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ValuesAttribute : DataAttribute
	{
		private readonly object[] _data;

		public ValuesAttribute(params object[] data)
		{
			_data = data;
		}

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			var parameters = testMethod.GetParameters();
			if (parameters.Length != 1)
			{
				throw new ArgumentException($"Method {testMethod.Name} must have exactly one parameter to use [Values] attribute");
			}

			var parameterType = parameters[0].ParameterType;

			// If no data provided and parameter is XamlInflator enum, return all enum values
			if (_data == null || _data.Length == 0)
			{
				if (parameterType == typeof(Xaml.XamlInflator))
				{
					yield return new object[] { Xaml.XamlInflator.Runtime };
					yield return new object[] { Xaml.XamlInflator.XamlC };
					yield return new object[] { Xaml.XamlInflator.SourceGen };
				}
				else
				{
					throw new ArgumentException($"[Values] attribute requires explicit values for parameter type {parameterType.Name}");
				}
			}
			else
			{
				// Return provided data
				foreach (var value in _data)
				{
					yield return new object[] { value };
				}
			}
		}
	}
}

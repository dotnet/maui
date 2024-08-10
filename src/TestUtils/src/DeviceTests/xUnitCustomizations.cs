#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Maui
{
	public class InlineDataDiscoverer : Xunit.Sdk.InlineDataDiscoverer
	{
		public InlineDataDiscoverer(IMessageSink diagnosticMessageSink)
		{
		}
	}

	/// <summary>
	/// Provides a data source for a data theory, with the data coming from inline values.
	/// </summary>
	[DataDiscoverer("Microsoft.Maui.InlineDataDiscoverer", "Microsoft.Maui.TestUtils.DeviceTests")]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class InlineDataAttribute : DataAttribute
	{
		readonly object[] data;

		/// <summary>
		/// Initializes a new instance of the <see cref="InlineDataAttribute"/> class.
		/// </summary>
		/// <param name="data">The data values to pass to the theory.</param>
		public InlineDataAttribute(params object[] data)
		{
			this.data = data;
		}

		/// <inheritdoc/>
		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			// This is called by the WPA81 version as it does not have access to attribute ctor params
			return new[] { data };
		}
	}

	public class ClassDataDiscoverer : DataDiscoverer
	{
		public ClassDataDiscoverer(IMessageSink diagnosticMessageSink)
		{
		}
	}

	/// <summary>
	/// Provides a data source for a data theory, with the data coming from a class
	/// which must implement IEnumerable&lt;object[]&gt;.
	/// Caution: the property is completely enumerated by .ToList() before any test is run. Hence it should return independent object sets.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	[DataDiscoverer("Microsoft.Maui.ClassDataDiscoverer", "Microsoft.Maui.TestUtils.DeviceTests")]
	public class ClassDataAttribute : DataAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ClassDataAttribute"/> class.
		/// </summary>
		/// <param name="class">The class that provides the data.</param>
		public ClassDataAttribute(Type @class)
		{
			Class = @class;
		}

		/// <summary>
		/// Gets the type of the class that provides the data.
		/// </summary>
		public Type Class { get; private set; }

		/// <inheritdoc/>
		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			IEnumerable<object[]>? data = Activator.CreateInstance(Class) as IEnumerable<object[]>;
			if (data == null)
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"{0} must implement IEnumerable<object[]> to be used as ClassData for the test method named '{1}' on {2}",
						Class.FullName,
						testMethod.Name,
						testMethod.DeclaringType!.FullName
					)
				);

			return data;
		}
	}

	public class MemberDataDiscoverer : DataDiscoverer
	{
		public MemberDataDiscoverer(IMessageSink diagnosticMessageSink)
		{
		}
	}

	/// <summary>
	/// Provides a data source for a data theory, with the data coming from one of the following sources:
	/// 1. A static property
	/// 2. A static field
	/// 3. A static method (with parameters)
	/// The member must return something compatible with IEnumerable&lt;object[]&gt; with the test data.
	/// Caution: the property is completely enumerated by .ToList() before any test is run. Hence it should return independent object sets.
	/// </summary>
	[DataDiscoverer("Microsoft.Maui.MemberDataDiscoverer", "Microsoft.Maui.TestUtils.DeviceTests")]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class MemberDataAttribute : MemberDataAttributeBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemberDataAttribute"/> class.
		/// </summary>
		/// <param name="memberName">The name of the public static member on the test class that will provide the test data</param>
		/// <param name="parameters">The parameters for the member (only supported for methods; ignored for everything else)</param>
		public MemberDataAttribute(string memberName, params object[] parameters)
			: base(memberName, parameters) { }

		/// <inheritdoc/>
		protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
		{
			if (item == null)
				return null!;

			var array = item as object[];
			if (array == null)
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Property {0} on {1} yielded an item that is not an object[]",
						MemberName,
						MemberType ?? testMethod.DeclaringType
					)
				);

			return array;
		}
	}

	/// <summary>
	/// Custom trait discoverer which adds a Category trait for filtering, etc.
	/// </summary>
	public class CategoryDiscoverer : ITraitDiscoverer
	{
		public const string Category = "Category";

		public CategoryDiscoverer(IMessageSink diagnosticMessageSink)
		{
		}

		public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
		{
			var args = traitAttribute.GetConstructorArguments().FirstOrDefault();

			if (args is string[] categories)
			{
				foreach (var category in categories)
				{
					yield return new KeyValuePair<string, string>(Category, category.ToString());
				}
			}
		}
	}

	/// <summary>
	/// Custom fact discoverer which wraps the resulting test cases to provide custom names
	/// </summary>
	public class FactDiscoverer : Xunit.Sdk.FactDiscoverer
	{
		public FactDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
		{
		}

		public override IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions,
			ITestMethod testMethod, IAttributeInfo factAttribute)
		{
			var cases = base.Discover(discoveryOptions, testMethod, factAttribute);

			foreach (var testCase in cases)
			{
				yield return new DeviceTestCase(testCase);
			}
		}
	}

	/// <summary>
	/// Custom theory discoverer which wraps the resulting test cases to provide custom names
	/// </summary>
	public class TheoryDiscoverer : Xunit.Sdk.TheoryDiscoverer
	{
		public TheoryDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
		{
		}

		public override IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions,
			ITestMethod testMethod, IAttributeInfo factAttribute)
		{
			var testCases = base.Discover(discoveryOptions, testMethod, factAttribute);

			foreach (var testCase in testCases)
			{
				yield return new DeviceTestCase(testCase);
			}
		}
	}

	/// <summary>
	/// Conveninence attribute for setting a Category trait on a test or test class
	/// </summary>
	[TraitDiscoverer("Microsoft.Maui.CategoryDiscoverer", "Microsoft.Maui.TestUtils.DeviceTests")]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class CategoryAttribute : Attribute, ITraitAttribute
	{
		// Yes, it looks like the cateory parameter is not used. Don't worry, CategoryDiscoverer uses it. 
		public CategoryAttribute(params string[] categories) { }
	}

	/// <summary>
	/// Custom Fact attribute which defaults to using the test method name for the DisplayName property
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[XunitTestCaseDiscoverer("Microsoft.Maui.FactDiscoverer", "Microsoft.Maui.TestUtils.DeviceTests")]
	public class FactAttribute : Xunit.FactAttribute
	{
		public FactAttribute([CallerMemberName] string displayName = "")
		{
			base.DisplayName = displayName;
		}
	}

	/// <summary>
	/// Custom Theory attribute which defaults to using the test method name for the DisplayName property
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[XunitTestCaseDiscoverer("Microsoft.Maui.TheoryDiscoverer", "Microsoft.Maui.TestUtils.DeviceTests")]
	public class TheoryAttribute : Xunit.TheoryAttribute
	{
		public TheoryAttribute([CallerMemberName] string displayName = "")
		{
			base.DisplayName = displayName;
		}
	}

#pragma warning disable xUnit3000 // Test case classes must derive directly or indirectly from Xunit.LongLivedMarshalByRefObject
	// If we try to actually subclass Xunit.LongLivedMarshalByRefObject here, we get a version conflict between the XUnit and 
	// the runner. LLMBRO is only a concern if we're worried about app domains, and for the moment we aren't. If we have to 
	// worry about that later, we can work out the conflict issues.
	public class DeviceTestCase : IXunitTestCase
#pragma warning restore xUnit3000 // Test case classes must derive directly or indirectly from Xunit.LongLivedMarshalByRefObject
	{
		readonly IXunitTestCase _inner = null!;

		public DeviceTestCase() { }

		public DeviceTestCase(IXunitTestCase inner)
		{
			_inner = inner;
		}

		public Exception InitializationException => _inner.InitializationException;

		public IMethodInfo Method => _inner.Method;

		public int Timeout => _inner.Timeout;

		string? _categoryPrefix;

		string GetCategoryPrefix()
		{
			if (_categoryPrefix == null)
			{
				if (!Traits.ContainsKey(CategoryDiscoverer.Category))
				{
					_categoryPrefix = string.Empty;
				}
				else
				{
					_categoryPrefix = $"[{string.Join(", ", Traits[CategoryDiscoverer.Category])}] ";
				}
			}

			return _categoryPrefix;
		}

		string? _displayName;

		public string DisplayName
		{
			get
			{
				_displayName = _displayName ?? $"{GetCategoryPrefix()}{_inner.DisplayName}";
				return _displayName;
			}
		}

		public string SkipReason => _inner.SkipReason;

		public ISourceInformation SourceInformation { get => _inner.SourceInformation; set => _inner.SourceInformation = value; }

		public ITestMethod TestMethod => _inner.TestMethod;

		public object[] TestMethodArguments => _inner.TestMethodArguments;

		public Dictionary<string, List<string>> Traits => _inner.Traits;

		public string UniqueID => _inner.UniqueID;

		public void Deserialize(IXunitSerializationInfo info)
		{
			_inner.Deserialize(info);
		}

		public Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
		{
			return _inner.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);
		}

		public void Serialize(IXunitSerializationInfo info)
		{
			_inner.Serialize(info);
		}
	}
}
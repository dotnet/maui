using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Member))]
	[ProvideCompiled("Microsoft.Maui.Controls.Build.Tasks.StaticExtension")]
	[RequiresUnreferencedCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
	[RequireService([typeof(IXamlTypeResolver)])]
	public class StaticExtension : IMarkupExtension
	{
		public string Member { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver typeResolver))
				throw new ArgumentException("No IXamlTypeResolver in IServiceProvider");
			if (string.IsNullOrEmpty(Member) || Member.IndexOf(".", StringComparison.Ordinal) == -1)
				throw new XamlParseException("Syntax for x:Static is [Member=][prefix:]typeName.staticMemberName", serviceProvider);

			var dotIdx = Member.LastIndexOf('.');
			var typename = Member.Substring(0, dotIdx);
			var membername = Member.Substring(dotIdx + 1);

			var type = typeResolver.Resolve(typename, serviceProvider, expandToExtension: false);

			// Search for static property in the type hierarchy
			var currentType = type.GetTypeInfo();
			while (currentType != null)
			{
				var pinfo = currentType.DeclaredProperties.FirstOrDefault(pi => pi.Name == membername && pi.GetMethod?.IsStatic == true);
				if (pinfo != null)
					return pinfo.GetMethod.Invoke(null, Array.Empty<object>());

				var finfo = currentType.DeclaredFields.FirstOrDefault(fi => fi.Name == membername && fi.IsStatic);
				if (finfo != null)
					return finfo.GetValue(null);

				currentType = currentType.BaseType?.GetTypeInfo();
			}

			throw new XamlParseException($"No static member found for {Member}", serviceProvider);
		}
	}
}
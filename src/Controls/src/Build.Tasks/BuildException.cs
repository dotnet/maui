using System;
using System.Linq;
using System.Xml;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	[Serializable]
	class BuildException : Exception
	{
		public BuildExceptionCode Code { get; private set; }

		public IXmlLineInfo XmlInfo { get; private set; }

		public string[] MessageArgs { get; private set; }

		public override string HelpLink { get => Code.HelpLink; set => base.HelpLink = value; }

		public BuildException(BuildExceptionCode code, IXmlLineInfo xmlInfo, Exception innerException, params object[] args)
			: base(FormatMessage(code, xmlInfo, args), innerException)
		{
			Code = code;
			XmlInfo = xmlInfo;
			MessageArgs = args?.Select(a => a?.ToString()).ToArray();
		}

		protected BuildException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

		static string FormatMessage(BuildExceptionCode code, IXmlLineInfo xmlinfo, object[] args)
		{
			var message = string.Format(ErrorMessages.ResourceManager.GetString(code.ErrorMessageKey), args);
			var ecode = code.Code;
			var position = xmlinfo == null || !xmlinfo.HasLineInfo() ? "" : $"({xmlinfo.LineNumber},{xmlinfo.LinePosition})";

			return $"{position} : XamlC error {ecode} : {message}";
		}
	}

	class BuildExceptionCode
	{
		//Assemblies, Types, Members
		public static BuildExceptionCode TypeResolution = new BuildExceptionCode("XFC", 0000, nameof(TypeResolution), "");
		public static BuildExceptionCode PropertyResolution = new BuildExceptionCode("XFC", 0001, nameof(PropertyResolution), "");
		public static BuildExceptionCode MissingEventHandler = new BuildExceptionCode("XFC", 0002, nameof(MissingEventHandler), "");
		public static BuildExceptionCode PropertyMissing = new BuildExceptionCode("XFC", 0003, nameof(PropertyMissing), "");
		public static BuildExceptionCode ConstructorDefaultMissing = new BuildExceptionCode("XFC", 0004, nameof(ConstructorDefaultMissing), "");
		public static BuildExceptionCode ConstructorXArgsMissing = new BuildExceptionCode("XFC", 0005, nameof(ConstructorXArgsMissing), "");
		public static BuildExceptionCode MethodStaticMissing = new BuildExceptionCode("XFC", 0006, nameof(MethodStaticMissing), "");
		public static BuildExceptionCode EnumValueMissing = new BuildExceptionCode("XFC", 0007, nameof(EnumValueMissing), "");
		public static BuildExceptionCode AdderMissing = new BuildExceptionCode("XFC", 0008, nameof(AdderMissing), "");
		public static BuildExceptionCode MemberResolution = new BuildExceptionCode("XFC", 0009, nameof(MemberResolution), "");

		//BP,BO
		public static BuildExceptionCode BPName = new BuildExceptionCode("XFC", 0020, nameof(BPName), "");
		public static BuildExceptionCode BPMissingGetter = new BuildExceptionCode("XFC", 0021, nameof(BPMissingGetter), "");
		public static BuildExceptionCode BindingWithoutDataType = new BuildExceptionCode("XC", 0022, nameof(BindingWithoutDataType), "https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/compiled-bindings"); //warning
		public static BuildExceptionCode BindingWithNullDataType = new BuildExceptionCode("XC", 0023, nameof(BindingWithNullDataType), "https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/compiled-bindings"); //warning

		//Bindings, conversions
		public static BuildExceptionCode Conversion = new BuildExceptionCode("XFC", 0040, nameof(Conversion), "");
		public static BuildExceptionCode BindingIndexerNotClosed = new BuildExceptionCode("XFC", 0041, nameof(BindingIndexerNotClosed), "");
		public static BuildExceptionCode BindingIndexerEmpty = new BuildExceptionCode("XFC", 0042, nameof(BindingIndexerEmpty), "");
		public static BuildExceptionCode BindingIndexerTypeUnsupported = new BuildExceptionCode("XFC", 0043, nameof(BindingIndexerTypeUnsupported), "");
		public static BuildExceptionCode BindingIndexerParse = new BuildExceptionCode("XFC", 0044, nameof(BindingIndexerParse), "");
		public static BuildExceptionCode BindingPropertyNotFound = new BuildExceptionCode("XFC", 0045, nameof(BindingPropertyNotFound), "");

		//XAML issues
		public static BuildExceptionCode MarkupNotClosed = new BuildExceptionCode("XFC", 0060, nameof(MarkupNotClosed), "");
		public static BuildExceptionCode MarkupParsingFailed = new BuildExceptionCode("XFC", 0061, nameof(MarkupParsingFailed), "");
		public static BuildExceptionCode XmlnsUndeclared = new BuildExceptionCode("XFC", 0062, nameof(XmlnsUndeclared), "");
		public static BuildExceptionCode SByteEnums = new BuildExceptionCode("XFC", 0063, nameof(SByteEnums), "");
		public static BuildExceptionCode NamescopeDuplicate = new BuildExceptionCode("XFC", 0064, nameof(NamescopeDuplicate), "");
		public static BuildExceptionCode ContentPropertyAttributeMissing = new BuildExceptionCode("XFC", 0065, nameof(ContentPropertyAttributeMissing), "");
		public static BuildExceptionCode InvalidXaml = new BuildExceptionCode("XFC", 0066, nameof(InvalidXaml), "");


		//Extensions
		public static BuildExceptionCode XStaticSyntax = new BuildExceptionCode("XFC", 0100, nameof(XStaticSyntax), "");
		public static BuildExceptionCode XStaticResolution = new BuildExceptionCode("XFC", 0101, nameof(XStaticResolution), "");
		public static BuildExceptionCode XDataTypeSyntax = new BuildExceptionCode("XFC", 0102, nameof(XDataTypeSyntax), "");

		//Style, StyleSheets, Resources
		public static BuildExceptionCode StyleSheetSourceOrContent = new BuildExceptionCode("XFC", 0120, nameof(StyleSheetSourceOrContent), "");
		public static BuildExceptionCode StyleSheetNoSourceOrContent = new BuildExceptionCode("XFC", 0121, nameof(StyleSheetNoSourceOrContent), "");
		public static BuildExceptionCode StyleSheetStyleNotALiteral = new BuildExceptionCode("XFC", 0122, nameof(StyleSheetStyleNotALiteral), "");
		public static BuildExceptionCode StyleSheetSourceNotALiteral = new BuildExceptionCode("XFC", 0123, nameof(StyleSheetSourceNotALiteral), "");
		public static BuildExceptionCode ResourceMissing = new BuildExceptionCode("XFC", 0124, nameof(ResourceMissing), "");
		public static BuildExceptionCode ResourceDictDuplicateKey = new BuildExceptionCode("XFC", 0125, nameof(ResourceDictDuplicateKey), "");
		public static BuildExceptionCode ResourceDictMissingKey = new BuildExceptionCode("XFC", 0126, nameof(ResourceDictMissingKey), "");
		public static BuildExceptionCode XKeyNotLiteral = new BuildExceptionCode("XFC", 0127, nameof(XKeyNotLiteral), "");

		//CSC equivalents
		public static BuildExceptionCode ObsoleteProperty = new BuildExceptionCode("XC", 0618, nameof(ObsoleteProperty), ""); //warning

		public string Code { get; }
		public string CodePrefix { get; }
		public int CodeCode { get; }

		public string ErrorMessageKey { get; private set; }
		public string HelpLink { get; private set; }

		BuildExceptionCode()
		{
		}

		BuildExceptionCode(string codePrefix, int codeCode, string errorMessageKey, string helpLink)
		{
			Code = $"{codePrefix}{codeCode:0000}";
			CodePrefix = codePrefix;
			CodeCode = codeCode;
			ErrorMessageKey = errorMessageKey;
			HelpLink = helpLink;
		}
	}
}

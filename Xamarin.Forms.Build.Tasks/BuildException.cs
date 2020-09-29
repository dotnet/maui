using System;
using System.Linq;
using System.Xml;

namespace Xamarin.Forms.Build.Tasks
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
		public static BuildExceptionCode TypeResolution = new BuildExceptionCode("XFC0000", nameof(TypeResolution), "");
		public static BuildExceptionCode PropertyResolution = new BuildExceptionCode("XFC0001", nameof(PropertyResolution), "");
		public static BuildExceptionCode MissingEventHandler = new BuildExceptionCode("XFC0002", nameof(MissingEventHandler), "");
		public static BuildExceptionCode PropertyMissing = new BuildExceptionCode("XFC0003", nameof(PropertyMissing), "");
		public static BuildExceptionCode ConstructorDefaultMissing = new BuildExceptionCode("XFC0004", nameof(ConstructorDefaultMissing), "");
		public static BuildExceptionCode ConstructorXArgsMissing = new BuildExceptionCode("XFC0005", nameof(ConstructorXArgsMissing), "");
		public static BuildExceptionCode MethodStaticMissing = new BuildExceptionCode("XFC0006", nameof(MethodStaticMissing), "");
		public static BuildExceptionCode EnumValueMissing = new BuildExceptionCode("XFC0007", nameof(EnumValueMissing), "");
		public static BuildExceptionCode AdderMissing = new BuildExceptionCode("XFC0008", nameof(AdderMissing), "");
		public static BuildExceptionCode MemberResolution = new BuildExceptionCode("XFC0009", nameof(MemberResolution), "");


		//BP,BO
		public static BuildExceptionCode BPName = new BuildExceptionCode("XFC0020", nameof(BPName), "");
		public static BuildExceptionCode BPMissingGetter = new BuildExceptionCode("XFC0021", nameof(BPMissingGetter), "");

		//Bindings, conversions
		public static BuildExceptionCode Conversion = new BuildExceptionCode("XFC0040", nameof(Conversion), "");
		public static BuildExceptionCode BindingIndexerNotClosed = new BuildExceptionCode("XFC0041", nameof(BindingIndexerNotClosed), "");
		public static BuildExceptionCode BindingIndexerEmpty = new BuildExceptionCode("XFC0042", nameof(BindingIndexerEmpty), "");
		public static BuildExceptionCode BindingIndexerTypeUnsupported = new BuildExceptionCode("XFC0043", nameof(BindingIndexerTypeUnsupported), "");
		public static BuildExceptionCode BindingIndexerParse = new BuildExceptionCode("XFC0044", nameof(BindingIndexerParse), "");
		public static BuildExceptionCode BindingPropertyNotFound = new BuildExceptionCode("XFC0045", nameof(BindingPropertyNotFound), "");

		//XAML issues
		public static BuildExceptionCode MarkupNotClosed = new BuildExceptionCode("XFC0060", nameof(MarkupNotClosed), "");
		public static BuildExceptionCode MarkupParsingFailed = new BuildExceptionCode("XFC0061", nameof(MarkupParsingFailed), "");
		public static BuildExceptionCode XmlnsUndeclared = new BuildExceptionCode("XFC0062", nameof(XmlnsUndeclared), "");
		public static BuildExceptionCode SByteEnums = new BuildExceptionCode("XFC0063", nameof(SByteEnums), "");
		public static BuildExceptionCode NamescopeDuplicate = new BuildExceptionCode("XFC0064", nameof(NamescopeDuplicate), "");
		public static BuildExceptionCode ContentPropertyAttributeMissing = new BuildExceptionCode("XFC0065", nameof(ContentPropertyAttributeMissing), "");
		public static BuildExceptionCode InvalidXaml = new BuildExceptionCode("XFC0066", nameof(InvalidXaml), "");


		//Extensions
		public static BuildExceptionCode XStaticSyntax = new BuildExceptionCode("XFC0100", nameof(XStaticSyntax), "");
		public static BuildExceptionCode XStaticResolution = new BuildExceptionCode("XFC0101", nameof(XStaticResolution), "");
		public static BuildExceptionCode XDataTypeSyntax = new BuildExceptionCode("XFC0102", nameof(XDataTypeSyntax), "");

		//Style, StyleSheets, Resources
		public static BuildExceptionCode StyleSheetSourceOrContent = new BuildExceptionCode("XFC0120", nameof(StyleSheetSourceOrContent), "");
		public static BuildExceptionCode StyleSheetNoSourceOrContent = new BuildExceptionCode("XFC0121", nameof(StyleSheetNoSourceOrContent), "");
		public static BuildExceptionCode StyleSheetStyleNotALiteral = new BuildExceptionCode("XFC0122", nameof(StyleSheetStyleNotALiteral), "");
		public static BuildExceptionCode StyleSheetSourceNotALiteral = new BuildExceptionCode("XFC0123", nameof(StyleSheetSourceNotALiteral), "");
		public static BuildExceptionCode ResourceMissing = new BuildExceptionCode("XFC0124", nameof(ResourceMissing), "");
		public static BuildExceptionCode ResourceDictDuplicateKey = new BuildExceptionCode("XFC0125", nameof(ResourceDictDuplicateKey), "");
		public static BuildExceptionCode ResourceDictMissingKey = new BuildExceptionCode("XFC0126", nameof(ResourceDictMissingKey), "");

		public string Code { get; private set; }
		public string ErrorMessageKey { get; private set; }
		public string HelpLink { get; private set; }

		BuildExceptionCode()
		{
		}

		BuildExceptionCode(string code, string errorMessage, string helpLink)
		{
			Code = code;
			ErrorMessageKey = errorMessage;
			HelpLink = helpLink;
		}
	}
}
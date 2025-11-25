; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
MAUIG1001 | XamlParsing | Error | XamlParsingFailed
MAUIG1002 | XamlParsing | Error | AmbiguousType
MAUIG1003 | XamlParsing | Error | ExpressionNotClosed
MAUIG1010 | XamlParsing | Error | ConversionFailed
MAUIX2000 | XamlInflation | Error | TypeResolutionFailed
MAUIX2001 | XamlInflation | Error | Descriptors
MAUIX2002 | XamlInflation | Error | Descriptors
MAUIX2003 | XamlInflation | Error | Descriptors
MAUIX2004 | XamlInflation | Error | Descriptors
MAUIX2005 | XamlInflation | Warning | Descriptors
MAUIX2006 | XamlInflation | Error | PropertyResolution
MAUIX2007 | XamlInflation | Error | MissingEventHandler
MAUIX2008 | XamlInflation | Error | AdderMissing
MAUIX2009 | XamlInflation | Error | ConstructorDefaultMissing
MAUIX2022 | Performance | Warning | BindingWithoutDataType
MAUIX2041 | XamlParsing | Error | BindingIndexerNotClosed
MAUIX2042 | XamlParsing | Error | BindingIndexerEmpty
MAUIX2062 | XamlParsing | Error | XmlnsUndeclared
MAUIX2064 | XamlInflation | Error | NamescopeDuplicate
MAUIX2065 | XamlParsing | Error | ContentPropertyAttributeMissing
MAUIX2100 | XamlParsing | Error | XStaticSyntax
MAUIX2101 | XamlInflation | Error | XStaticResolution
MAUIX2102 | XamlParsing | Error | XDataTypeSyntax
MAUIX2126 | XamlInflation | Error | ResourceDictMissingKey

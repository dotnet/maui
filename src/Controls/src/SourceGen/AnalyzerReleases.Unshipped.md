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
MAUIX2006 | XamlParsing | Warning | PropertyElementWithAttribute
MAUIX2014 | XamlInflation | Error | MissingEventHandler
MAUIG2024 | XamlInflation | Warning | BindingWithXDataTypeFromOuterScope
MAUIG2041 | XamlInflation | Error | BindingIndexerNotClosed
MAUIG2042 | XamlInflation | Error | BindingIndexerEmpty
MAUIG2043 | XamlInflation | Error | BindingIndexerTypeUnsupported
MAUIG2045 | XamlInflation | Warning | BindingPropertyNotFound
MAUIG2064 | XamlInflation | Error | NamescopeDuplicate
MAUIX2007 | XamlParsing | Warning | AmbiguousExpressionOrMarkup
MAUIX2008 | XamlParsing | Error | AmbiguousMemberExpression
MAUIX2009 | XamlParsing | Error | MemberNotFound
MAUIX2010 | XamlParsing | Info | ExpressionNotSettable
MAUIX2011 | XamlParsing | Warning | AmbiguousMemberWithStaticType
MAUIX2012 | XamlParsing | Error | CSharpExpressionsRequirePreviewFeatures
MAUIX2013 | XamlParsing | Error | AsyncLambdaNotSupported

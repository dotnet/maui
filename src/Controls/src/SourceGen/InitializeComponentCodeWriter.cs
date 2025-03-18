using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

static class InitializeComponentCodeWriter
{
	static readonly string NewLine = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\n" : "\n";

    public static string GeneratedCodeAttribute => $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{typeof(InitializeComponentCodeWriter).Assembly.FullName}\", \"{typeof(InitializeComponentCodeWriter).Assembly.GetName().Version}\")]";
	const string AutoGeneratedHeaderText = @"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a .NET MAUI source generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
";

    public static string GenerateInitializeComponent(XamlProjectItemForIC xamlItem, Compilation compilation, SourceProductionContext sourceProductionContext, AssemblyCaches xmlnsCache, IDictionary<XmlType, ITypeSymbol> typeCache)
    {
        using (var codeWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") {NewLine = NewLine})
        {
            PrePost newblock() =>
                PrePost.NewBlock(codeWriter);
            
            codeWriter.WriteLine(AutoGeneratedHeaderText);
            var warningDisable = xamlItem.Root!.DisableWarnings != null ? string.Join(", ", xamlItem.Root!.DisableWarnings) : null;
            if (warningDisable != null && warningDisable.Length > 0)
            {
                codeWriter.WriteLine($"#pragma warning disable {warningDisable}");
                codeWriter.WriteLine();
            }
            var root = xamlItem.Root!;
            
            if (root.Properties.TryGetValue(XmlName.xClass, out var classNode))
            {
                string accessModifier = "public";
				if ((classNode as ValueNode)?.Value is not string rootClass)
					goto exit;

				if (root.Properties.TryGetValue(XmlName.xClassModifier, out var classModifierNode))
                {
                    var classModifier = (classModifierNode as ValueNode)?.Value as string;
                    accessModifier = classModifier?.ToLowerInvariant().Replace("notpublic", "internal") ?? "public"; // notpublic is WPF for internal
                }

                //TODO support x:TypeArguments

                XmlnsHelper.ParseXmlns(rootClass, out var rootTypeName, out var rootClrNamespace, out _, out _);
                var rootType = compilation.GetTypeByMetadataName($"{rootClrNamespace}.{rootTypeName}");
                //roottype null might mean we need to add the type to the Compilation, we'll figure that out later
                if (rootType == null)
                    goto exit;

        		(var genSwitch, var xamlInflators, var set) = rootType.GetXamlProcessing();

                //this test must go as soon as 'set' goes away
                if (!set)
                    goto exit;

                if (   (xamlInflators & XamlInflator.SourceGen)!= XamlInflator.SourceGen
                    && xamlInflators != XamlInflator.Default
                    && xamlItem.ProjectItem.ForceSourceGen == false)
                    goto exit;

                codeWriter.WriteLine($"namespace {rootClrNamespace};");
                codeWriter.WriteLine();
                codeWriter.WriteLine(GeneratedCodeAttribute);
                codeWriter.WriteLine($"{accessModifier} partial class {rootTypeName}");
                using (newblock()) {
                    codeWriter.WriteLine($"private partial void InitializeComponentSourceGen()");
                    xamlItem.Root!.XmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, out var baseType);
                    var sgcontext = new SourceGenContext(codeWriter, compilation, sourceProductionContext, xmlnsCache, typeCache, rootType!, baseType) {FilePath = xamlItem.ProjectItem.RelativePath};
                    using(newblock()) {
                        Visit(root, sgcontext);
                    }
                    
                    foreach (var writer in sgcontext.AddtitionalWriters)
                    {
                        codeWriter.Write(writer.ToString());
                        codeWriter.WriteLine();
                    }
                }
                
            } else { //No x:Class attribute
                //TODO

            }
exit:
            codeWriter.Flush();
            return codeWriter.InnerWriter.ToString();   
        }
    }

    static void Visit(RootNode rootnode, SourceGenContext visitorContext, bool useDesignProperties = false)
    {
        rootnode.Accept(new XamlNodeVisitor((node, parent) => node.Parent = parent), null); //set parents for {StaticResource}
        rootnode.Accept(new ExpandMarkupsVisitor(visitorContext), null);
        rootnode.Accept(new PruneIgnoredNodesVisitor(useDesignProperties), null);
        if (useDesignProperties)
            rootnode.Accept(new RemoveDuplicateDesignNodes(), null);
        rootnode.Accept(new CreateValuesVisitor(visitorContext), null);
        rootnode.Accept(new SetNamescopesAndRegisterNamesVisitor(visitorContext), null); //set namescopes for {x:Reference} and FindByName
        rootnode.Accept(new SetFieldsForXNamesVisitor(visitorContext), null);
        // rootnode.Accept(new SimplifyTypeExtensionVisitor(), null);
        // rootnode.Accept(new FillResourceDictionariesVisitor(visitorContext), null);
        rootnode.Accept(new SetResourcesVisitor(visitorContext), null);
        rootnode.Accept(new SetPropertiesVisitor(visitorContext, true), null);
    }
}

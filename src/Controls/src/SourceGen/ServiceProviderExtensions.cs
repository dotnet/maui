using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;

using Microsoft.Maui.Controls.Xaml;

using Microsoft.CodeAnalysis;

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;


namespace Microsoft.Maui.Controls.SourceGen;

static class ServiceProviderExtensions
{
    public static LocalVariable GetOrCreateServiceProvider(this INode node, IndentedTextWriter writer, SourceGenContext context, ImmutableArray<ITypeSymbol>? requiredServices)
    {
        IFieldSymbol? bpFieldSymbol = null;
        IPropertySymbol? propertySymbol = null;
        if (node.TryGetPropertyName(node.Parent, out var propertyName))
        {
            var localName = propertyName.LocalName;
            if (context.Variables.TryGetValue(node.Parent, out var parentVar))
            {
                bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out bool attached, context, null);
                propertySymbol = parentVar.Type.GetAllProperties(localName).FirstOrDefault();
            }
        }

        //TODO based on the node, the service provider, or some of the services, could be reused
        var serviceProviderSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.Internals.XamlServiceProvider")!;
        var serviceProviderVariableName = NamingHelpers.CreateUniqueVariableName(context, "XamlServiceProvider");

        writer.WriteLine($"var {serviceProviderVariableName} = new {serviceProviderSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}(this);");
  
        node.AddServices(writer, serviceProviderVariableName, requiredServices, context, bpFieldSymbol, propertySymbol);

        return context.ServiceProviders[node] = new LocalVariable(serviceProviderSymbol, serviceProviderVariableName);
    }

    static void AddServices (this INode node, IndentedTextWriter writer, string serviceProviderVariableName, ImmutableArray<ITypeSymbol>? requiredServices, SourceGenContext context, IFieldSymbol? bpFieldSymbol, IPropertySymbol? propertySymbol)
    {     
        var createAllServices = requiredServices is null;
        var objectAndParents = node.ObjectAndParents(context).ToArray();
        if (   (   createAllServices
                || requiredServices!.Value.Contains(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IProvideParentValues")!, SymbolEqualityComparer.Default)
                || requiredServices!.Value.Contains(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IReferenceProvider")!, SymbolEqualityComparer.Default))
            && objectAndParents.Length > 0)
        {
            var simpleValueTargetProvider = NamingHelpers.CreateUniqueVariableName(context, "ValueTargetProvider");
            writer.WriteLine($"var {simpleValueTargetProvider} = new global::Microsoft.Maui.Controls.Xaml.Internals.SimpleValueTargetProvider(");
            writer.Indent++;
            writer.WriteLine($"new object[] {{{String.Join(", ", node.ObjectAndParents(context).Select(v=>v.Name))}}},");
            var bpinfo = bpFieldSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType)) ?? String.Empty;
            var pinfo = $"typeof({propertySymbol?.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}).GetProperty(\"{propertySymbol?.Name}\")" ?? string.Empty;
            writer.WriteLine($"{(bpFieldSymbol != null ? bpFieldSymbol : propertySymbol != null ? pinfo : "null")},");
            writer.WriteLine($"null,");
            writer.WriteLine($"false);");
            writer.Indent--;
            writer.WriteLine($"{serviceProviderVariableName}.Add(typeof(global::Microsoft.Maui.Controls.Xaml.IReferenceProvider), {simpleValueTargetProvider});");
            writer.WriteLine($"{serviceProviderVariableName}.Add(typeof(global::Microsoft.Maui.Controls.Xaml.IProvideValueTarget), {simpleValueTargetProvider});");
        }

        if (createAllServices
            || requiredServices!.Value.Contains(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IXamlTypeResolver")!, SymbolEqualityComparer.Default))      
        {
            var nsResolver = NamingHelpers.CreateUniqueVariableName(context, "NSResolver");
            writer.WriteLine($"var {nsResolver} = new global::Microsoft.Maui.Controls.Xaml.Internals.XmlNamespaceResolver();");
            
            foreach (var kvp in node.NamespaceResolver!.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml))
                writer.WriteLine($"{nsResolver}.Add(\"{kvp.Key}\", \"{kvp.Value}\");");
            
            writer.WriteLine($"{serviceProviderVariableName}.Add(typeof(global::Microsoft.Maui.Controls.Xaml.IXamlTypeResolver), new global::Microsoft.Maui.Controls.Xaml.Internals.XamlTypeResolver({nsResolver}, typeof({context.RootType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}).Assembly));");
        }
    }

    static IEnumerable<LocalVariable> ObjectAndParents(this INode node, SourceGenContext context)
    {
        var currentCtx = context;
        while (currentCtx != null)
        {
            if (currentCtx.Variables.TryGetValue(node, out var variable))
                yield return variable;
            var n = node.Parent;
            while (n is not null)
            {
                if (n is IElementNode en )
                {
                    if (currentCtx.Variables.TryGetValue(en, out var parentVariable))
                        yield return parentVariable;
                    n = n.Parent;        
                } else
                    break;
            }
            currentCtx = currentCtx.ParentContext;
        }

    }
    public static (bool acceptEmptyServiceProvider, ImmutableArray<ITypeSymbol>? requiredServices) GetServiceProviderAttributes(this ITypeSymbol typeConverter, SourceGenContext context)
    {
        var acceptEmptyServiceProviderAttribute = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.AcceptEmptyServiceProviderAttribute")!;
        var acceptEmptyServiceProvider = typeConverter.GetAttributes(acceptEmptyServiceProviderAttribute).Any();;
        var requireServiceAttribute = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.RequireServiceAttribute")!;
        var requiredServices = typeConverter.GetAttributes(requireServiceAttribute).FirstOrDefault()?.ConstructorArguments[0].Values.Where(ca => ca.Value is ITypeSymbol).Select(ca => (ca.Value as ITypeSymbol)!).ToImmutableArray() ?? null;
        return (acceptEmptyServiceProvider, requiredServices);
    }

}
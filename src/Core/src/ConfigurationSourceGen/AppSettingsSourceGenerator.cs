using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Core.ConfigurationSourceGen;

[Generator]
public class AppSettingsSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get additional files (like appsettings.json) and combine them
        var appSettingsProvider = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileName(file.Path).Equals("appsettings.json", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, cancellationToken) => 
            {
                var content = file.GetText(cancellationToken)?.ToString();
                return content;
            })
            .Where(static content => !string.IsNullOrEmpty(content))
            .Collect(); // Collect all appsettings.json files into a single value

        // Generate source when we have any appsettings.json files
        context.RegisterSourceOutput(appSettingsProvider, static (context, contents) =>
        {
            try
            {
                if (contents.IsEmpty)
                    return;

                // Combine all appsettings files into one configuration
                var allConfigurationEntries = new Dictionary<string, string>();
                
                foreach (var content in contents)
                {
                    if (!string.IsNullOrEmpty(content))
                    {
                        var entries = ParseJsonToConfigurationEntries(content!);
                        foreach (var entry in entries)
                        {
                            allConfigurationEntries[entry.Key] = entry.Value; // Later files override earlier ones
                        }
                    }
                }

                // Generate the extension method source code
                var sourceCode = GenerateExtensionMethod(allConfigurationEntries);

                // Add the generated source to the compilation
                context.AddSource("LocalAppSettingsExtensions.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                // Add diagnostic for any errors
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "MAS001",
                        "AppSettings Source Generator Error",
                        "Error generating app settings extension: {0}",
                        "MauiAppSettings",
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true),
                    Location.None,
                    ex.Message));
            }
        });
    }

    private static Dictionary<string, string> ParseJsonToConfigurationEntries(string jsonContent)
    {
        var entries = new Dictionary<string, string>();
        
        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            FlattenJsonElement(document.RootElement, entries, string.Empty);
        }
        catch (JsonException)
        {
            // If JSON parsing fails, return empty dictionary
        }
        
        return entries;
    }

    private static void FlattenJsonElement(JsonElement element, Dictionary<string, string> entries, string prefix)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";
                    FlattenJsonElement(property.Value, entries, key);
                }
                break;
            
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var key = $"{prefix}:{index}";
                    FlattenJsonElement(item, entries, key);
                    index++;
                }
                break;
            
            case JsonValueKind.String:
                entries[prefix] = element.GetString() ?? string.Empty;
                break;
            
            case JsonValueKind.Number:
                entries[prefix] = element.GetRawText();
                break;
            
            case JsonValueKind.True:
            case JsonValueKind.False:
                entries[prefix] = element.GetBoolean().ToString().ToLowerInvariant();
                break;
            
            case JsonValueKind.Null:
                entries[prefix] = string.Empty;
                break;
        }
    }

    private static string GenerateExtensionMethod(Dictionary<string, string> configurationEntries)
    {
        var sourceBuilder = new StringBuilder();
        
        sourceBuilder.AppendLine("//------------------------------------------------------------------------------");
        sourceBuilder.AppendLine("// <auto-generated>");
        sourceBuilder.AppendLine("//     This code was generated by the MAUI AppSettings source generator.");
        sourceBuilder.AppendLine("//");
        sourceBuilder.AppendLine("//     Changes to this file may cause incorrect behavior and will be lost if");
        sourceBuilder.AppendLine("//     the code is regenerated.");
        sourceBuilder.AppendLine("// </auto-generated>");
        sourceBuilder.AppendLine("//------------------------------------------------------------------------------");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("using System.Collections.Generic;");
        sourceBuilder.AppendLine("using Microsoft.Extensions.Configuration;");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("namespace Microsoft.Extensions.Configuration");
        sourceBuilder.AppendLine("{");
        sourceBuilder.AppendLine("    /// <summary>");
        sourceBuilder.AppendLine("    /// Extension methods for IConfigurationBuilder to add local app settings.");
        sourceBuilder.AppendLine("    /// </summary>");
        sourceBuilder.AppendLine("    public static class LocalAppSettingsExtensions");
        sourceBuilder.AppendLine("    {");
        sourceBuilder.AppendLine("        /// <summary>");
        sourceBuilder.AppendLine("        /// Adds configuration values from the local appsettings.json file as an in-memory collection.");
        sourceBuilder.AppendLine("        /// </summary>");
        sourceBuilder.AppendLine("        /// <param name=\"configurationBuilder\">The configuration builder.</param>");
        sourceBuilder.AppendLine("        /// <returns>The configuration builder.</returns>");
        sourceBuilder.AppendLine("        public static IConfigurationBuilder AddLocalAppSettings(this IConfigurationBuilder configurationBuilder)");
        sourceBuilder.AppendLine("        {");
        sourceBuilder.AppendLine("            var configurationData = new Dictionary<string, string>");
        sourceBuilder.AppendLine("            {");
        
        foreach (var entry in configurationEntries)
        {
            var escapedKey = EscapeStringLiteral(entry.Key);
            var escapedValue = EscapeStringLiteral(entry.Value);
            sourceBuilder.AppendLine($"                {{ \"{escapedKey}\", \"{escapedValue}\" }},");
        }
        
        sourceBuilder.AppendLine("            };");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("            return configurationBuilder.AddInMemoryCollection(configurationData);");
        sourceBuilder.AppendLine("        }");
        sourceBuilder.AppendLine("    }");
        sourceBuilder.AppendLine("}");
        
        return sourceBuilder.ToString();
    }

    private static string EscapeStringLiteral(string input)
    {
        return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
    }
}
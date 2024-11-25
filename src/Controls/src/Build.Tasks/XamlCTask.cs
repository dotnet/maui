using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static Microsoft.Build.Framework.MessageImportance;
using static Mono.Cecil.Cil.OpCodes;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	/// <summary>
	/// Provides extension methods for the <see cref="TaskLoggingHelper"/> class to assist with logging warnings and errors.
	/// </summary>
	static class LoggingHelperExtensions
	{
		class LoggingHelperContext
		{
			public int WarningLevel { get; set; } = 4; //unused so far
			public bool TreatWarningsAsErrors { get; set; } = false;
			public IList<int> WarningsAsErrors { get; set; }
			public IList<int> WarningsNotAsErrors { get; set; }
			public IList<int> NoWarn { get; set; }
			public string PathPrefix { get; set; }
		}

		static LoggingHelperContext Context { get; set; }
		internal static List<BuildException> LoggedErrors { get; set; }

		public static void SetContext(
			this TaskLoggingHelper loggingHelper,
			int warningLevel,
			bool treatWarningsAsErrors,
			string noWarn,
			string warningsAsErrors,
			string warningsNotAsErrors,
			string pathPrefix)
		{
			if (Context == null)
				Context = new LoggingHelperContext();
			Context.WarningLevel = warningLevel;
			Context.TreatWarningsAsErrors = treatWarningsAsErrors;
			Context.PathPrefix = pathPrefix;

			Context.NoWarn = noWarn?.Split([';', ','], StringSplitOptions.RemoveEmptyEntries).Select(s =>
			{
				if (int.TryParse(s, out var i))
					return i;
				if (s.StartsWith("XC"))
				{
					var code = s.Substring(2);
					if (int.TryParse(code, out i))
						return i;
				}
				return -1;
			}).Where(i => i != -1).ToList();

			Context.WarningsAsErrors = warningsAsErrors?.Split([';', ','], StringSplitOptions.RemoveEmptyEntries).Select(s =>
			{
				if (int.TryParse(s, out var i))
					return i;
				if (s.StartsWith("XC"))
				{
					var code = s.Substring(2);
					if (int.TryParse(code, out i))
						return i;
				}
				return -1;
			}).Where(i => i != -1).ToList();

			Context.WarningsNotAsErrors = warningsNotAsErrors?.Split([';', ','], StringSplitOptions.RemoveEmptyEntries).Select(s =>
			{
				if (int.TryParse(s, out var i))
					return i;
				if (s.StartsWith("XC"))
				{
					var code = s.Substring(2);
					if (int.TryParse(code, out i))
						return i;
				}
				return -1;
			}).Where(i => i != -1).ToList();
		}

		public static void LogWarningOrError(this TaskLoggingHelper loggingHelper, BuildExceptionCode code, string xamlFilePath, int lineNumber, int linePosition, int endLineNumber, int endLinePosition, params object[] messageArgs)
		{
			if (Context == null)
				Context = new LoggingHelperContext();
			if (Context.NoWarn != null && Context.NoWarn.Contains(code.CodeCode))
				return;
			xamlFilePath = loggingHelper.GetXamlFilePath(xamlFilePath);
			if ((Context.TreatWarningsAsErrors && (Context.WarningsNotAsErrors == null || !Context.WarningsNotAsErrors.Contains(code.CodeCode)))
				|| (Context.WarningsAsErrors != null && Context.WarningsAsErrors.Contains(code.CodeCode)))
			{
				loggingHelper.LogError("XamlC", $"{code.CodePrefix}{code.CodeCode:0000}", code.HelpLink, xamlFilePath, lineNumber, linePosition, endLineNumber, endLinePosition, ErrorMessages.ResourceManager.GetString(code.ErrorMessageKey), messageArgs);
				LoggedErrors ??= new();
				LoggedErrors.Add(new BuildException(code, new XmlLineInfo(lineNumber, linePosition), innerException: null, messageArgs));
			}
			else
			{
				loggingHelper.LogWarning("XamlC", $"{code.CodePrefix}{code.CodeCode:0000}", code.HelpLink, xamlFilePath, lineNumber, linePosition, endLineNumber, endLinePosition, ErrorMessages.ResourceManager.GetString(code.ErrorMessageKey), messageArgs);
			}
		}

		public static string GetXamlFilePath(this TaskLoggingHelper loggingHelper, string xamlFilePath)
		{
			Context ??= new LoggingHelperContext();

			if (Context.PathPrefix is string prefix)
			{
				xamlFilePath = IOPath.Combine(prefix, xamlFilePath);
			}

			return xamlFilePath;
		}
	}

	public class XamlCTask : XamlTask
	{
		readonly XamlCache cache = new();
		bool hasCompiledXamlResources;
		public bool KeepXamlResources { get; set; }
		public bool OptimizeIL { get; set; } = true;
		public bool DefaultCompile { get; set; }
		public bool ForceCompile { get; set; }
		public bool CompileBindingsWithSource { get; set; }
		public string TargetFramework { get; set; }

		public int WarningLevel { get; set; } = 4; //unused so far
		public bool TreatWarningsAsErrors { get; set; } = false;
		public string WarningsAsErrors { get; set; }
		public string WarningsNotAsErrors { get; set; }
		public string NoWarn { get; set; }

		public bool GenerateFullPaths { get; set; }
		public string FullPathPrefix { get; set; }

		public IAssemblyResolver DefaultAssemblyResolver { get; set; }

		internal string Type { get; set; }
		internal MethodDefinition InitCompForType { get; private set; }

		/// <summary>
		/// Enable to optimize for shorter build time
		/// e.g. OptimizeIL unused, Debug symbols not loaded, no assemblies written
		/// </summary>
		public bool ValidateOnly { get; set; }

		internal bool GenerateFullILInValidateOnlyMode { get; set; }

		public override bool Execute(out IList<Exception> thrownExceptions)
		{
			thrownExceptions = null;
			LoggingHelper.SetContext(WarningLevel, TreatWarningsAsErrors, NoWarn, WarningsAsErrors, WarningsNotAsErrors, GenerateFullPaths ? FullPathPrefix : null);
			LoggingHelper.LogMessage(Normal, $"{new string(' ', 0)}Compiling Xaml, assembly: {Assembly}");
			var skipassembly = !DefaultCompile;
			bool success = true;

			if (!File.Exists(Assembly))
			{
				LoggingHelper.LogMessage(Normal, $"{new string(' ', 2)}Assembly file not found. Skipping XamlC.");
				return true;
			}

			if (GenerateFullPaths && string.IsNullOrEmpty(FullPathPrefix))
			{
				LoggingHelper.LogMessage(Low, "  GenerateFullPaths is enabled but FullPathPrefix is missing or empty.");
			}

			using (var fallbackResolver = DefaultAssemblyResolver == null ? new XamlCAssemblyResolver() : null)
			{
				var resolver = DefaultAssemblyResolver ?? fallbackResolver;
				if (resolver is XamlCAssemblyResolver xamlCResolver)
				{
					if (ReferencePath != null)
					{
						var paths = ReferencePath.Select(p => IOPath.GetDirectoryName(p.Replace("//", "/"))).Distinct();
						foreach (var searchpath in paths)
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}Adding searchpath {searchpath}");
							xamlCResolver.AddSearchDirectory(searchpath);
						}
					}
				}
				else
					LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}Ignoring dependency and reference paths due to an unsupported resolver");

				var debug = DebugSymbols || (!string.IsNullOrEmpty(DebugType) && DebugType.ToLowerInvariant() != "none");

				var readerParameters = new ReaderParameters
				{
					AssemblyResolver = resolver,
					ReadWrite = !ValidateOnly,
					ReadSymbols = debug && !ValidateOnly, // We don't need symbols for ValidateOnly, since we won't be writing
				};

				using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(IOPath.GetFullPath(Assembly), readerParameters))
				{
					CustomAttribute xamlcAttr;
					if (assemblyDefinition.HasCustomAttributes &&
						(xamlcAttr =
							assemblyDefinition.CustomAttributes.FirstOrDefault(
								ca => ca.AttributeType.FullName == "Microsoft.Maui.Controls.Xaml.XamlCompilationAttribute")) != null)
					{
						var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
						if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
							skipassembly = true;
						if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
							skipassembly = false;
					}

					foreach (var module in assemblyDefinition.Modules)
					{
						var skipmodule = skipassembly;
						if (module.HasCustomAttributes &&
							(xamlcAttr =
								module.CustomAttributes.FirstOrDefault(
									ca => ca.AttributeType.FullName == "Microsoft.Maui.Controls.Xaml.XamlCompilationAttribute")) != null)
						{
							var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
							if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
								skipmodule = true;
							if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
								skipmodule = false;
						}

						LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}Module: {module.Name}");
						var resourcesToPrune = new List<EmbeddedResource>();
						foreach (var resource in module.Resources.OfType<EmbeddedResource>())
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 4)}Resource: {resource.Name}");
							string classname;
							if (!resource.IsXaml(cache, module, out classname))
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}skipped.");
								continue;
							}
							TypeDefinition typeDef = module.GetType(classname);
							if (typeDef == null)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}no type found... skipped.");
								continue;
							}
							var skiptype = skipmodule;
							if (typeDef.HasCustomAttributes &&
								(xamlcAttr =
									typeDef.CustomAttributes.FirstOrDefault(
										ca => ca.AttributeType.FullName == "Microsoft.Maui.Controls.Xaml.XamlCompilationAttribute")) != null)
							{
								var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
								if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
									skiptype = true;
								if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
									skiptype = false;
							}

							if (Type != null)
								skiptype = !(Type == classname);

							if (skiptype && !ForceCompile)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}has XamlCompilationAttribute set to Skip and not Compile... skipped.");
								continue;
							}

							var initComp = typeDef.Methods.FirstOrDefault(md => md.Name == "InitializeComponent");
							if (initComp == null)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}no InitializeComponent found... skipped.");
								continue;
							}

							CustomAttribute xamlFilePathAttr;
							var xamlFilePath = typeDef.HasCustomAttributes && (xamlFilePathAttr = typeDef.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == "Microsoft.Maui.Controls.Xaml.XamlFilePathAttribute")) != null ?
													  (string)xamlFilePathAttr.ConstructorArguments[0].Value :
													  resource.Name;


							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Parsing Xaml");
							var rootnode = ParseXaml(resource.GetResourceStream(), typeDef);
							if (rootnode == null)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}failed.");
								continue;
							}
							LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");

							hasCompiledXamlResources = true;

							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Replacing {0}.InitializeComponent ()");
							Exception e;
							if (!TryCoreCompile(initComp, rootnode, xamlFilePath, LoggingHelper, out e))
							{
								success = false;
								LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}failed.");
								(thrownExceptions = thrownExceptions ?? new List<Exception>()).Add(e);
								xamlFilePath = LoggingHelper.GetXamlFilePath(xamlFilePath);
								if (e is BuildException be)
									LoggingHelper.LogError("XamlC", be.Code.Code, be.HelpLink, xamlFilePath, be.XmlInfo?.LineNumber ?? 0, be.XmlInfo?.LinePosition ?? 0, 0, 0, ErrorMessages.ResourceManager.GetString(be.Code.ErrorMessageKey), be.MessageArgs);
								else if (e is XamlParseException xpe) //shouldn't happen anymore
									LoggingHelper.LogError("XamlC", null, xpe.HelpLink, xamlFilePath, xpe.XmlInfo.LineNumber, xpe.XmlInfo.LinePosition, 0, 0, xpe.Message);
								else if (e is XmlException xe)
									LoggingHelper.LogError("XamlC", null, xe.HelpLink, xamlFilePath, xe.LineNumber, xe.LinePosition, 0, 0, xe.Message);
								else
									LoggingHelper.LogError("XamlC", null, e.HelpLink, xamlFilePath, 0, 0, 0, 0, e.Message);
								LoggingHelper.LogMessage(Low, e.StackTrace);
								continue;
							}
							else
							{
								if (LoggingHelperExtensions.LoggedErrors is List<BuildException> errors)
								{
									foreach (var error in errors)
									{
										success = false;
										(thrownExceptions = thrownExceptions ?? new List<Exception>()).Add(error);
									}

									LoggingHelperExtensions.LoggedErrors = null;
								}
							}

							if (initComp.HasCustomAttributes)
							{
								var suppressMessageAttribute = initComp.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute");
								if (suppressMessageAttribute != null)
								{
									LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Removing UnconditionalSuppressMessageAttribute from InitializeComponent()");
									initComp.CustomAttributes.Remove(suppressMessageAttribute);
								}
							}
							if (Type != null)
								InitCompForType = initComp;

							LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");

							if (ValidateOnly)
								continue;

							if (OptimizeIL)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Optimizing IL");
								initComp.Body.Optimize();
								LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");
							}
							resourcesToPrune.Add(resource);
						}
						if (hasCompiledXamlResources)
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 4)}Changing the module MVID");
							module.Mvid = Guid.NewGuid();
							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}done.");
						}
						if (!KeepXamlResources)
						{
							if (resourcesToPrune.Any())
								LoggingHelper.LogMessage(Low, $"{new string(' ', 4)}Removing compiled xaml resources");
							foreach (var resource in resourcesToPrune)
							{
								LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Removing {resource.Name}");
								module.Resources.Remove(resource);
								LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");
							}
						}
					}
					if (ValidateOnly)
					{
						LoggingHelper.LogMessage(Low, $"{new string(' ', 0)}ValidateOnly=True. Skipping writing assembly.");
						return success;
					}
					if (!hasCompiledXamlResources)
					{
						LoggingHelper.LogMessage(Low, $"{new string(' ', 0)}No compiled resources. Skipping writing assembly.");
						return success;
					}

					LoggingHelper.LogMessage(Low, $"{new string(' ', 0)}Writing the assembly");
					try
					{
						assemblyDefinition.Write(new WriterParameters
						{
							WriteSymbols = debug,
						});
						LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}done.");
					}
					catch (Exception e)
					{
						LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}failed.");
						LoggingHelper.LogErrorFromException(e);
						(thrownExceptions = thrownExceptions ?? new List<Exception>()).Add(e);
						LoggingHelper.LogMessage(Low, e.StackTrace);
						success = false;
					}
				}
			}
			return success;
		}

		bool TryCoreCompile(MethodDefinition initComp, ILRootNode rootnode, string xamlFilePath, TaskLoggingHelper loggingHelper, out Exception exception)
		{
			try
			{
				var body = new MethodBody(initComp);
				var module = body.Method.Module;
				body.InitLocals = true;
				var il = body.GetILProcessor();
				var resourcePath = GetPathForType(cache, module, initComp.DeclaringType);

				il.Emit(Nop);

				var visitorContext = new ILContext(il, body, module, cache)
				{
					XamlFilePath = xamlFilePath,
					LoggingHelper = loggingHelper,
					ValidateOnly = ValidateOnly && !GenerateFullILInValidateOnlyMode,
					CompileBindingsWithSource = CompileBindingsWithSource,
				};


				rootnode.Accept(new XamlNodeVisitor((node, parent) => node.Parent = parent), null);
				rootnode.Accept(new ExpandMarkupsVisitor(visitorContext), null);
				rootnode.Accept(new PruneIgnoredNodesVisitor(), null);
				rootnode.Accept(new SimplifyOnPlatformVisitor(TargetFramework), null);
				rootnode.Accept(new CreateObjectVisitor(visitorContext), null);
				rootnode.Accept(new SetNamescopesAndRegisterNamesVisitor(visitorContext), null);
				rootnode.Accept(new SetFieldVisitor(visitorContext), null);
				rootnode.Accept(new SimplifyTypeExtensionVisitor(), null);
				rootnode.Accept(new SetResourcesVisitor(visitorContext), null);
				rootnode.Accept(new SetPropertiesVisitor(visitorContext, true), null);

				il.Emit(Ret);
				initComp.Body = body;
				exception = null;
				return true;
			}
			catch (Exception e)
			{
				exception = e;
				return false;
			}
		}

		internal static string GetPathForType(XamlCache cache, ModuleDefinition module, TypeReference type)
		{
			foreach (var ca in type.Module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (!TypeRefComparer.Default.Equals(ca.ConstructorArguments[2].Value as TypeReference, type))
					continue;
				return ca.ConstructorArguments[1].Value as string;
			}
			return null;
		}

		internal static string GetResourceIdForPath(XamlCache cache, ModuleDefinition module, string path)
		{
			foreach (var ca in module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (ca.ConstructorArguments[1].Value as string != path)
					continue;
				return ca.ConstructorArguments[0].Value as string;
			}
			return null;
		}
	}
}

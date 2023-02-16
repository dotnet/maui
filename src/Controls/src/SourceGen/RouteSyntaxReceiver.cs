using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Maui.Controls;

using static Microsoft.Maui.Controls.SourceGen.Helper;

namespace VijayAnand.MauiAutomation
{
	public class RouteSyntaxReceiver : ISyntaxReceiver
	{
		const string RouteAttribute = "Microsoft.Maui.Controls.RouteAttribute";
		const string MauiServiceAttribute = "Microsoft.Maui.Controls.MauiServiceAttribute";

		private readonly List<Service> _services = new();
		private readonly List<RoutedPage> _routedPages = new();
		private readonly List<ClassDeclarationSyntax> _shellPages = new();

		internal IEnumerable<Service> Services => _services;

		internal IEnumerable<RoutedPage> RoutedPages => _routedPages;

		internal IEnumerable<ClassDeclarationSyntax> ShellPages => _shellPages;

		internal ClassDeclarationSyntax? MauiApp { get; private set; }

		internal ClassDeclarationSyntax? MauiStartup { get; private set; }

		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is ClassDeclarationSyntax cds)
			{
				if (cds.BaseList is not null)
				{
					foreach (var baseType in cds.BaseList.Types)
					{
						if (baseType.Type is IdentifierNameSyntax identifierName)
						{
							if (identifierName.Identifier.ValueText == "Application")
							{
								MauiApp = cds;
								break;
							}
							else if (identifierName.Identifier.ValueText == "Shell")
							{
								_shellPages.Add(cds);
								break;
							}
						}
					}
				}

				if (cds.Identifier.ValueText == "MauiProgram")
				{
					foreach (var member in cds.Members)
					{
						if (member is MethodDeclarationSyntax methodDeclaration)
						{
							if (methodDeclaration.Identifier.ValueText == "CreateMauiApp")
							{
								MauiStartup = cds;
							}
						}
					}
				}
				else if (cds.AttributeLists.Count > 0)
				{
					SeparatedSyntaxList<AttributeArgumentSyntax>? arguments;

					// .Where(a => a.Name.NormalizeWhitespace().ToFullString().Replace("Attribute", string.Empty) == "Route");
					// .Where(a => a.Name.NormalizeWhitespace().ToFullString().Replace("Attribute", string.Empty) == "Service");

					var routeAttributes = cds.AttributeLists.SelectMany(al => al.Attributes)
						.Where(a => a.Name.NormalizeWhitespace().ToFullString().Replace("Attribute", string.Empty) == "Route");

					var routeAttribute = routeAttributes.FirstOrDefault();

					var serviceAttributes = cds.AttributeLists.SelectMany(al => al.Attributes)
						.Where(a => a.Name.NormalizeWhitespace().ToFullString().Replace("Attribute", string.Empty) == "MauiService");

					var serviceAttribute = serviceAttributes.FirstOrDefault();

					ServiceScope scope;

					if (routeAttribute is not null)
					{
						string? viewModelType = null;
						var implicitViewModel = false;
						var route = string.Empty;
						var routes = new List<string>();

						var routeArgument = true;
						scope = ServiceScope.Singleton;
						arguments = routeAttribute.ArgumentList?.Arguments;

						if (arguments is not null)
						{
							foreach (var argument in arguments)
							{
								if (argument.NameColon is not null)
								{
									if (argument.NameColon.Name.Identifier.Text == "route")
									{
										route = GetRoute(cds, argument.Expression);
									}
									else if (argument.NameColon.Name.Identifier.Text == "scope")
									{
										scope = GetScope(argument.Expression);
									}
								}
								else if (argument.NameEquals is not null)
								{
									if (argument.NameEquals.Name.Identifier.Text == "Routes")
									{
										if (argument.Expression is ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpression)
										{
											if (implicitArrayCreationExpression.Initializer.Kind() == SyntaxKind.ArrayInitializerExpression)
											{
												foreach (var expression in implicitArrayCreationExpression.Initializer.Expressions)
												{
													routes.Add(GetRoute(cds, expression));
												}
											}
										}
										else if (argument.Expression is ArrayCreationExpressionSyntax arrayCreationExpression)
										{
											if (arrayCreationExpression?.Initializer?.Kind() == SyntaxKind.ArrayInitializerExpression)
											{
												foreach (var expression in arrayCreationExpression.Initializer.Expressions)
												{
													routes.Add(GetRoute(cds, expression));
												}
											}
										}
									}
									else if (argument.NameEquals.Name.Identifier.Text == "Scope")
									{
										scope = GetScope(argument.Expression);
									}
									else if (argument.NameEquals.Name.Identifier.Text == "ImplicitViewModel")
									{
										if (argument.Expression is LiteralExpressionSyntax literalExpression)
										{
											bool.TryParse(literalExpression.Token.ValueText, out implicitViewModel);
										}
									}
									else if (argument.NameEquals.Name.Identifier.Text == "ViewModelType")
									{
										if (argument.Expression is TypeOfExpressionSyntax typeOfExpression)
										{
											//viewModelType = $"global::{GetNamespace(cds)}.{((IdentifierNameSyntax)typeOfExpression.Type).Identifier.Text}";
											viewModelType = ((IdentifierNameSyntax)typeOfExpression.Type).Identifier.Text;
										}
									}
								}
								else if (argument.Expression is not null)
								{
									if (routeArgument)
									{
										routeArgument = false;
										route = GetRoute(cds, argument.Expression);
									}
									else
									{
										scope = GetScope(argument.Expression);
									}
								}
							}
						}

						if (routes.Count == 0)
						{
							routes.Add(route);
						}

						_routedPages.Add(new RoutedPage(cds, routes, scope, implicitViewModel, viewModelType));
					}

					if (serviceAttribute is not null)
					{
						arguments = serviceAttribute.ArgumentList?.Arguments;

						scope = ServiceScope.Singleton;
						var registerFor = string.Empty;
						var useTryAdd = false;

						if (arguments is not null)
						{
							foreach (var argument in arguments)
							{
								if (argument.NameColon is not null)
								{
									if (argument.NameColon.Name.Identifier.Text == "scope")
									{
										scope = GetScope(argument.Expression);
									}
								}
								else if (argument.NameEquals is not null)
								{
									if (argument.NameEquals.Name.Identifier.Text == "RegisterFor")
									{
										if (argument.Expression is TypeOfExpressionSyntax typeOfExpression)
										{
											registerFor = ((IdentifierNameSyntax)typeOfExpression.Type).Identifier.Text;
										}
									}
									else if (argument.NameEquals.Name.Identifier.Text == "UseTryAdd")
									{
										if (argument.Expression is LiteralExpressionSyntax literalExpression)
										{
											bool.TryParse(literalExpression.Token.ValueText, out useTryAdd);
										}
									}
								}
								else if (argument.Expression is not null)
								{
									scope = GetScope(argument.Expression);
								}
							}
						}

						_services.Add(new Service(cds, scope, registerFor, useTryAdd));
					}
				}
			}
		}

		private static string GetRoute(ClassDeclarationSyntax classDeclaration, ExpressionSyntax? expression)
		{
			if (expression is LiteralExpressionSyntax literalExpression)
			{
				return $"\"{literalExpression.Token.ValueText}\"";
			}
			else if (expression is InvocationExpressionSyntax invocationExpression)
			{
				if (((IdentifierNameSyntax)invocationExpression.Expression).Identifier.Text == "nameof")
				{
					return $"nameof(global::{GetNamespace(classDeclaration)}.{invocationExpression.ArgumentList.Arguments})";
				}
				else
				{
					// Default route
					return string.Empty;
				}
			}
			else if (expression is MemberAccessExpressionSyntax memberAccessExpression)
			{
				return $"{((IdentifierNameSyntax)memberAccessExpression.Expression).Identifier.Text}.{memberAccessExpression.Name.Identifier.Text}";
			}
			else
			{
				// Default route
				return string.Empty;
			}
		}

		private static ServiceScope GetScope(ExpressionSyntax? expression)
		{
			if (expression is IdentifierNameSyntax identifierName)
			{
				return ParseScope(identifierName.Identifier.ValueText);
			}
			else if (expression is MemberAccessExpressionSyntax memberAccessExpression)
			{
				return ParseScope(memberAccessExpression.Name.Identifier.Text);
			}
			else
			{
				// Default scope
				return ServiceScope.Singleton;
			}

			static ServiceScope ParseScope(string scopeName) => scopeName switch
			{
				"Scoped" => ServiceScope.Scoped,
				"Transient" => ServiceScope.Transient,
				_ => ServiceScope.Singleton
			};
		}
	}

	internal class RoutedPage
	{
		public RoutedPage(
			ClassDeclarationSyntax type,
			IEnumerable<string> routes,
			ServiceScope scope,
			bool implicitViewModel = false,
			string? viewModelType = null)
		{
			Type = type;
			Routes = routes;
			Scope = scope;
			ImplicitViewModel = implicitViewModel;
			ViewModelType = viewModelType;
		}

		public ClassDeclarationSyntax Type { get; }

		public IEnumerable<string> Routes { get; }

		public ServiceScope Scope { get; set; }

		public bool ImplicitViewModel { get; set; }

		public string? ViewModelType { get; set; }
	}

	internal class Service
	{
		public Service(ClassDeclarationSyntax type, ServiceScope scope, string registerFor, bool useTryAdd)
			=> (Type, Scope, RegisterFor, UseTryAdd) = (type, scope, registerFor, useTryAdd);

		public ClassDeclarationSyntax Type { get; }

		public ServiceScope Scope { get; }

		public string RegisterFor { get; }

		public bool UseTryAdd { get; }
	}
}

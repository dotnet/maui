using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.Maui.Controls.SourceGen.Helper;

namespace Microsoft.Maui.Controls.SourceGen
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
			if (syntaxNode is not ClassDeclarationSyntax cds)
			{
				return;
			}

			if (cds.BaseList is not null)
			{
				foreach (var baseType in cds.BaseList.Types)
				{
					if (baseType.Type is not IdentifierNameSyntax identifierName)
					{
						continue;
					}

					if (identifierName.Identifier.ValueText == "Application")
					{
						MauiApp = cds;
						break;
					}

					if (identifierName.Identifier.ValueText == "Shell")
					{
						_shellPages.Add(cds);
						break;
					}
				}
			}

			if (cds.Identifier.ValueText == "MauiProgram")
			{
				foreach (var member in cds.Members)
				{
					if (member is MethodDeclarationSyntax { Identifier.ValueText: "CreateMauiApp" })
					{
						MauiStartup = cds;
						break;
					}
				}
			}
			else if (cds.AttributeLists.Count > 0)
			{
				IEnumerable<AttributeArgumentSyntax>? arguments;

				var routeAttributes = cds.AttributeLists.SelectMany(al => al.Attributes)
					.Where(a => a.Name.NormalizeWhitespace().ToFullString().Replace("Attribute", string.Empty) == "Route");

				var routeAttribute = routeAttributes.FirstOrDefault();

				var serviceAttributes = cds.AttributeLists.SelectMany(al => al.Attributes)
					.Where(a => a.Name.NormalizeWhitespace().ToFullString().Replace("Attribute", string.Empty) == "MauiService");

				var serviceAttribute = serviceAttributes.FirstOrDefault();

				string lifetime;

				if (routeAttribute is not null)
				{
					TypeSyntax? viewModelType = null;
					var implicitViewModel = false;
					var route = string.Empty;
					var routes = new List<string>();

					var routeArgument = true;
					lifetime = Singleton;
					arguments = routeAttribute.ArgumentList?.Arguments;

					if (arguments is not null)
					{
						foreach (var argument in arguments)
						{
							switch (argument.NameColon?.Name.Identifier.Text)
							{
								case "route":
									route = GetRoute(cds, argument.Expression);
									break;
								case "lifetime":
									lifetime = GetLifetime(argument.Expression);
									break;
							}

							switch (argument.NameEquals?.Name.Identifier.Text)
							{
								case "Routes" when argument.Expression is ArrayCreationExpressionSyntax arrayExpression:
									if (arrayExpression?.Initializer?.Kind() == SyntaxKind.ArrayInitializerExpression)
									{
										routes.AddRange(arrayExpression.Initializer.Expressions.Select(expr => GetRoute(cds, expr)));
									}
									break;
								case "Routes" when argument.Expression is ImplicitArrayCreationExpressionSyntax implicitArrayExpression:
									if (implicitArrayExpression.Initializer.Kind() == SyntaxKind.ArrayInitializerExpression)
									{
										routes.AddRange(implicitArrayExpression.Initializer.Expressions.Select(expr => GetRoute(cds, expr)));
									}
									break;
								case "Lifetime":
									lifetime = GetLifetime(argument.Expression);
									break;
								case "ImplicitViewModel" when argument.Expression is LiteralExpressionSyntax literalExpression:
									_ = bool.TryParse(literalExpression.Token.ValueText, out implicitViewModel);
									break;
								case "ViewModelType" when argument.Expression is TypeOfExpressionSyntax typeOfExpression:
									viewModelType = typeOfExpression.Type;
									break;
							}

							if (argument.NameColon is null && argument.NameEquals is null)
							{
								if (routeArgument)
								{
									routeArgument = false;
									route = GetRoute(cds, argument.Expression);
								}
								else
								{
									lifetime = GetLifetime(argument.Expression);
								}
							}
						}
					}

					if (routes.Count == 0)
					{
						routes.Add(route);
					}

					_routedPages.Add(new RoutedPage(cds, routes, lifetime, implicitViewModel, viewModelType));
				}

				if (serviceAttribute is not null)
				{
					arguments = serviceAttribute.ArgumentList?.Arguments;

					lifetime = Singleton;
					TypeSyntax? registerFor = null;
					var useTryAdd = false;

					if (arguments is not null)
					{
						foreach (var argument in arguments)
						{
							if (argument.NameColon?.Name.Identifier.Text == "lifetime")
							{
								lifetime = GetLifetime(argument.Expression);
							}

							switch (argument.NameEquals?.Name.Identifier.Text)
							{
								case "RegisterFor" when argument.Expression is TypeOfExpressionSyntax typeOfExpression:
									registerFor = typeOfExpression.Type;
									break;
								case "UseTryAdd" when argument.Expression is LiteralExpressionSyntax literalExpression:
									_ = bool.TryParse(literalExpression.Token.ValueText, out useTryAdd);
									break;
							}

							if (argument.Expression is not null)
							{
								lifetime = GetLifetime(argument.Expression);
							}
						}
					}

					_services.Add(new Service(cds, lifetime, registerFor, useTryAdd));
				}
			}
		}

		private static string GetRoute(ClassDeclarationSyntax classDeclaration, ExpressionSyntax? expression) => expression switch
		{
			LiteralExpressionSyntax literalExpression => $"\"{literalExpression.Token.ValueText}\"",
			InvocationExpressionSyntax invocationExpression => ((IdentifierNameSyntax)invocationExpression.Expression).Identifier.Text switch
			{
				"nameof" => $"nameof(global::{GetNamespace(classDeclaration)}.{invocationExpression.ArgumentList.Arguments})",
				_ => string.Empty,// Default route
			},
			MemberAccessExpressionSyntax memberAccessExpression => $"{((IdentifierNameSyntax)memberAccessExpression.Expression).Identifier.Text}.{memberAccessExpression.Name.Identifier.Text}",
			_ => string.Empty,// Default route
		};

		private static string GetLifetime(ExpressionSyntax? expression) => expression switch
		{
			IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
			MemberAccessExpressionSyntax memberAccessExpression => memberAccessExpression.Name.Identifier.Text,
			_ => Singleton
		};
	}

	internal class RoutedPage
	{
		public RoutedPage(
			ClassDeclarationSyntax type,
			IEnumerable<string> routes,
			string lifetime,
			bool implicitViewModel = false,
			TypeSyntax? viewModelType = null)
		{
			Type = type;
			Routes = routes;
			Lifetime = lifetime;
			ImplicitViewModel = implicitViewModel;
			ViewModelType = viewModelType;
		}

		public ClassDeclarationSyntax Type { get; }

		public IEnumerable<string> Routes { get; }

		public string Lifetime { get; }

		public bool ImplicitViewModel { get; }

		public TypeSyntax? ViewModelType { get; }
	}

	internal class Service
	{
		public Service(ClassDeclarationSyntax type, string lifetime, TypeSyntax? registerFor, bool useTryAdd)
			=> (Type, Lifetime, RegisterFor, UseTryAdd) = (type, lifetime, registerFor, useTryAdd);

		public ClassDeclarationSyntax Type { get; }

		public string Lifetime { get; }

		public TypeSyntax? RegisterFor { get; }

		public bool UseTryAdd { get; }
	}
}

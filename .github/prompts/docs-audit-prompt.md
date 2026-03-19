Analyze the following PR diff and generate detailed documentation requirements for a coding agent to implement.

Review the changes for:
- New public APIs, methods, classes, or interfaces (check for PublicAPI.Unshipped.txt changes)
- New features or capabilities
- Breaking changes requiring migration guides
- New configuration options, MSBuild properties, or settings
- Behavioral changes to existing controls, layouts, or platform behavior
- New platform-specific features (Android, iOS, macCatalyst, Windows)
- New XAML markup extensions, attached properties, or bindable properties
- Changes to .NET MAUI Essentials APIs

Before making recommendations, check the .NET MAUI documentation repository at https://github.com/dotnet/docs-maui to:
- Identify existing documentation pages that should be updated
- Determine if a new documentation page should be created
- Find related documentation that needs cross-references
- Understand the current documentation structure and patterns

If documentation is needed, provide:
1. A clear title for the documentation task
2. Whether to UPDATE existing page(s) or CREATE new page(s)
   - For updates: List the specific file paths in the docs-maui repo that need updating
   - For new pages: Suggest the file path and location where the new page should be added
3. Specific sections that need to be created or updated
4. Key points that must be covered in each section
5. Code examples that should be included (with placeholders for actual code)
6. Any warnings, notes, or best practices to document
7. Cross-references to related documentation pages
8. Platform-specific documentation needs (if the change is platform-specific)

If no documentation is needed (e.g., internal refactoring, test-only changes, CI/build changes, or bug fixes with no user-facing behavioral change), respond with exactly 'NO_DOCS_NEEDED'.

Format your response as a structured issue description that a coding agent can follow to implement the documentation.

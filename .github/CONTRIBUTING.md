# Contributing

Thank you for your interest in contributing to .NET MAUI! In this document, we'll outline what you need to know about contributing and how to get started.

First and foremost: if you are thinking about contributing a bigger change or feature, **please open an issue or talk to a core team member first**! By doing this we can coordinate and see if the change you are going to work on is something that aligns with out current priorities and is something we can commit to reviewing and merging within reasonable time. We would want to prevent that you are investing a lot of your valuable time in something that might not be in line with what we want for .NET MAUI.

## Code of Conduct

Please see our [Code of Conduct](/.github/CODE_OF_CONDUCT.md).

## Prerequisite

You will need to complete a Contribution License Agreement before any pull request can be accepted. Review the CLA at https://cla.dotnetfoundation.org/. When you submit a pull request, a CLA assistant bot will confirm you have completed the agreement, or provide you with an opportunity to do so.

## Contributing Code

Currently, we are in the beginning phases of building up MAUI. Yet, we are still very excited for you to join us during this exciting time :)

Have a look at our [Development Guide](/.github/DEVELOPMENT.md) to learn about setting up your development environment.

### What to work on

If you're looking for something to work on, please browse our [backlog](https://github.com/dotnet/maui/issues?q=is%3Aopen+is%3Aissue+milestone%3ABacklog). Any issue that is not already assigned is up for grabs. 

Follow the style used by the [.NET Foundation](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md), with two primary exceptions:

- We do not use the `private` keyword, as it is the default accessibility level in C#.
- We use hard tabs over spaces.

Read and follow our [Pull Request template](/.github/PULL_REQUEST_TEMPLATE.md).

### AI-Assisted Development

We have custom AI agents to help you work on issues more efficiently. See [Using AI to Work on .NET MAUI](https://github.com/dotnet/maui/wiki/Using-AI-to-Work-on-.NET-MAUI) for details.

### Pull Request Requirements

Please refer to our [Pull Request template](/.github/PULL_REQUEST_TEMPLATE.md).

Please check the "Allow edits from maintainers" checkbox on your pull request. This allows us to quickly make minor fixes and resolve conflicts for you.

### Performance-related Changes

Performance improvements can be tricky to get right, and can sometimes have unexpected consequences and impact code readability. If you're considering a performance-related change, here are a few things to keep in mind:

1. **Profile a real-world application**: Before making any performance-related changes, profile a real-world application (or at least a sample project) to understand where the bottlenecks are. This will help you understand if your change is actually improving performance, and if it's improving the right thing. We want to avoid micro-optimizations that don't actually improve real-world .NET MAUI applications.

See [our profiling wiki](https://aka.ms/profile-maui) for instructions on how to profile a .NET MAUI application.

2. **Benchmark your change**: If you're making a performance-related change, please include benchmarks in your pull request. This will help us understand the impact of your change, and will help us avoid performance regressions in the future.

Provide before & after numbers using BenchmarkDotNet where possible. See our existing [BenchmarkDotNet project](/src/Core/tests/Benchmarks/) for examples.

If a BenchmarkDotNet test case is not possible, share before & after profiling information from Visual Studio, `dotnet-trace`, etc. Keep in mind that sampling profilers can be inaccurate, so someone from the .NET MAUI team may need to reproduce your results before merging your change.

3. **Preserve existing behavior**: If you're making a performance-related change, please make sure that you're not changing the behavior of the code. For example, if you're changing the implementation of a method, make sure that the new implementation returns the same results as the old implementation. In some cases, you may need to add new unit tests to ensure that the behavior of the code hasn't changed.

4. **Avoid impacting readability**: Performance-related changes can sometimes make code harder to read and understand. In many cases, it will be worth it if the payoff is significant, but please be mindful of the trade-offs. Write code comments and unit tests to help others understand the code in the future.

## Proposals/Enhancements/Suggestions

To propose a change or new feature, open an issue using the [Feature request template](https://github.com/dotnet/maui/issues/new?template=feature-request.yml). You may also use the [Spec template](https://github.com/dotnet/maui/issues/new?template=spec.yml) if you have an idea of what the API should look like. Be sure to also browse current issues and [discussions](https://github.com/dotnet/maui/discussions) that may be related to what you have in mind.

## Review Process
All pull requests need to be reviewed and tested by at least two members of the .NET MAUI team. We do our best to review pull requests in a timely manner, but please be patient! Two reviewers will be assigned and will start the review process as soon as possible. If there are any changes requested, the contributor should make them at their earliest convenience or let the reviewers know that they are unable to make further contributions. If the pull request requires only minor changes, then someone else may pick it up and finish it. We will do our best to make sure that all credit is retained for contributors. 

Once a pull request has two approvals, it will receive an "approved" label. As long as no UI or unit tests are failing, this pull request can be merged at this time.

## Merge Process
Handler property PRs should target the main branch.

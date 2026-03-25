# Fixes Log

## Spec 01 — Solution Setup & Core Models

### Missing `using AwesomeAssertions;` in test file
**Problem**: The `Should()` extension methods from AwesomeAssertions were not found because the test file was missing the `using AwesomeAssertions;` directive. The test project's template does not add global usings for AwesomeAssertions automatically.  
**Fix**: Added `using AwesomeAssertions;` to `SeedDataTests.cs`.

### Paragraph split failing on Windows line endings
**Problem**: `story.SpanishText.Split("\n\n")` returned 1 segment instead of 3+ because the raw string literal in C# uses `\r\n` on Windows, so paragraphs are separated by `\r\n\r\n`, not `\n\n`.  
**Fix**: Normalized line endings before splitting: `story.SpanishText.Replace("\r\n", "\n").Trim().Split("\n\n", ...)`.

## Spec 03 — Quiz Feature

### ProgressService depends on IJSRuntime not available in Core class library
**Problem**: `SpanishLearning.Core` is a plain class library with no ASP.NET/Blazor references, so `Microsoft.JSInterop.IJSRuntime` was not found when compiling `ProgressService.cs`.  
**Fix**: Added `<PackageReference Include="Microsoft.JSInterop" Version="10.0.0" />` to `SpanishLearning.Core.csproj`.

## Spec 08 — Dashboard & Navigation

### Missing using directive for ProgressService in Home.razor
**Problem**: `Home.razor` injected `ProgressService` but the `SpanishLearning.Core.Services` namespace was not imported anywhere in the Blazor component tree.  
**Fix**: Added `@using SpanishLearning.Core.Services` to `SpanishLearning.Web/Components/_Imports.razor`.


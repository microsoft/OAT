# Dependabot PR Recommendations

This document provides specific recommendations and closing comments for each open Dependabot PR.

---

## PR #380: Bump Microsoft.NET.Test.Sdk from 17.14.1 to 18.0.0

**Recommendation: CLOSE (Already Applied)**

**Closing Comment:**
```
Thank you Dependabot! This update has already been applied to the main branch. The current version of Microsoft.NET.Test.Sdk is 18.0.0.

Verified with testing:
- ✅ Build successful across all frameworks
- ✅ All 66 tests passing on net8.0, net9.0, net10.0

Closing this PR as the update is already in place.
```

---

## PR #378: Bump Microsoft.AspNetCore.Components.WebAssembly from 8.0.19 to 8.0.21

**Recommendation: CLOSE (Already Applied)**

**Closing Comment:**
```
Thank you Dependabot! This update has already been applied to the main branch. The current version of Microsoft.AspNetCore.Components.WebAssembly is 8.0.21.

Both packages in OAT.Blazor are now at 8.0.21:
- Microsoft.AspNetCore.Components.WebAssembly: 8.0.21 ✓
- Microsoft.AspNetCore.Components.WebAssembly.DevServer: 8.0.21 ✓

Verified with testing:
- ✅ Build successful
- ✅ Framework compatibility maintained (net8.0)

Closing this PR as the update is already in place.
```

---

## PR #379: Bump Microsoft.AspNetCore.Components.WebAssembly.DevServer from 8.0.19 to 9.0.10

**Recommendation: CLOSE (Breaks Framework Compatibility)**

**Closing Comment:**
```
Thank you for the suggestion Dependabot, but we cannot accept this update as it would break framework compatibility.

**Issue:**
OAT.Blazor targets `net8.0` specifically, and upgrading to .NET 9 packages would introduce incompatibilities.

**Current State:**
The DevServer package has been updated to 8.0.21 (staying within the .NET 8 version family), which is the correct approach for a project targeting net8.0.

**Framework-Specific Versioning:**
The OAT repository intentionally uses framework-specific package versions:
- Projects targeting net8.0 use ASP.NET Core 8.x packages
- Projects targeting net9.0 use ASP.NET Core 9.x packages
- Projects targeting net10.0 use ASP.NET Core 10.x packages

See OAT.Blazor.Components.csproj for an example of this pattern with conditional package references.

**When to Upgrade:**
This package can be upgraded to 9.x when:
1. OAT.Blazor's TargetFramework is changed from net8.0 to net9.0
2. All dependencies are verified to be compatible with .NET 9

Closing this PR to prevent framework compatibility issues.
```

---

## PR #375: Bump Microsoft.NET.ILLink.Tasks from 8.0.18 to 8.0.20

**Recommendation: CLOSE (Unnecessary Explicit Reference)**

**Closing Comment:**
```
Thank you Dependabot, but we don't need to explicitly reference this package.

**Issue:**
Microsoft.NET.ILLink.Tasks is a transitive dependency that is automatically brought in by the .NET SDK and Blazor WebAssembly build tools. Adding an explicit PackageReference is unnecessary and can cause versioning conflicts.

**Current State:**
The project correctly relies on the SDK to provide the appropriate version of ILLink.Tasks based on the target framework.

**Best Practice:**
Only add explicit PackageReferences for:
1. Packages you directly use in your code
2. Packages where you need to pin a specific version for compatibility

Transitive dependencies like ILLink.Tasks should be left to the SDK's dependency resolution.

Closing this PR as the explicit reference is not needed.
```

---

## PR #367: Bump Microsoft.AspNetCore.Components.WebAssembly and 5 others

**Recommendation: CLOSE (Outdated + Breaks Framework Compatibility)**

**Closing Comment:**
```
Thank you Dependabot, but this PR has multiple issues:

**1. Outdated:**
This PR is from an older update cycle. More recent updates have superseded these changes.

**2. Framework Compatibility Issues:**
The PR attempts to update DevServer from 8.0.18 to 9.0.8, which would break compatibility with OAT.Blazor that targets net8.0.

**3. Unnecessary Explicit References:**
The PR adds explicit references to Microsoft.NET.ILLink.Tasks and Microsoft.NET.Sdk.WebAssembly.Pack, which are transitive dependencies managed by the SDK.

**Current State:**
All packages in this PR have been properly updated in subsequent updates:
- WebAssembly: 8.0.21 (correctly staying on 8.x) ✓
- DevServer: 8.0.21 (correctly staying on 8.x) ✓
- System.Net.Http.Json: 9.0.10 ✓
- System.Collections.Immutable: 9.0.10 ✓
- Newtonsoft.Json: 13.0.4 ✓

Closing this PR as it's outdated and the updates have been applied correctly in a framework-compatible manner.
```

---

## Additional Recommendation: Update Dependabot Configuration

To prevent future PRs that would break framework compatibility, consider updating `.github/dependabot.yml`:

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 10
    ignore:
      # Ignore major version updates for framework-specific packages
      - dependency-name: "Microsoft.AspNetCore.Components*"
        update-types: ["version-update:semver-major"]
      - dependency-name: "Microsoft.NET.ILLink.Tasks"
        # Ignore completely - transitive dependency
      - dependency-name: "Microsoft.NET.Sdk.WebAssembly.Pack"
        # Ignore completely - transitive dependency
```

This configuration will:
1. Allow minor and patch updates (e.g., 8.0.19 → 8.0.21) ✓
2. Prevent major version updates (e.g., 8.x → 9.x) that would break framework compatibility ✓
3. Ignore transitive dependencies that shouldn't be explicitly referenced ✓

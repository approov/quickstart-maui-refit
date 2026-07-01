# Changelog

## 3.5.11 — 2026-07-01
- Updated to target `Approov.Service.Maui` 3.5.11
- Upgraded from net7.0 to net9.0 (iOS minimum 15.0, Android API 21 unchanged)
- Added USAGE.md with practical integration patterns
- Added CHANGELOG.md
- Rewrote README to align with current Approov quickstart standards
- Fixed missing Approov package reference in ShapesApp project file
- Updated all API calls: `ApproovService.CreateHttpClient()` → `new ApproovHttpClient()`
- Removed obsolete ApproovRefit and Square.OkHttp3 package references

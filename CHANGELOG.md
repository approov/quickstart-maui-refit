# Changelog

Versions in this file identify the quickstart, independently of the service layer and native SDK versions.

## 3.3.0 — Unreleased

- Upgrade the sample to .NET 9, with iOS 15 and Android API 23 as the minimum OS versions.
- Consume the service layer through a sibling `ProjectReference`; document revision `3584257422de1ca9ee7cb7a78adad862cb05a27e` (declared version `3.5.5`) and the separate native SDK `3.5.3` downloads.
- Use standard Refit with `ApproovHttpClient`, without an Approov-specific Refit package.
- Organize the Shapes example into API-key-only (`v1`), token-only (`v3`) and token-plus-message-signing (`v5`) stages, with explicit signing control.
- Document installation-signing account configuration and an unsigned `v5` negative control.
- Update the README, API reference link, secure-string fetch example and secrets-protection walkthrough; add usage examples with versioned base URLs.
- Update Newtonsoft.Json to `13.0.3` and add Android manifest label-merge handling.
- Ignore generated build outputs.

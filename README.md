# SecureAI Browser

Phase 1 is a small WPF browser built on Microsoft WebView2. WebView2 provides all web rendering; this project does not implement a browser engine.

## Project structure

- `UI` contains the WPF shell.
- `Browser` manages WebView2 and browser actions.
- `Security` contains the centralized deny-by-default web-content policy.
- `AI` contains only the future-provider contract; no provider, SDK, credentials, or page-data extraction is included.
- `Models` and `Services` contain small shared types and address/search resolution.

## Security defaults

- Chromium/WebView2 TLS validation, sandboxing, and security features remain enabled.
- No native host object is exposed to webpage JavaScript.
- All webpage permission prompts are denied, including camera, microphone, location, and notifications.
- Popups are handled in the same WebView only for HTTP(S) addresses; other protocol launches are blocked.
- Downloads are blocked in this phase, pending an explicit secure download workflow.
- Navigation to non-HTTP(S) protocols is blocked.

## Run in Visual Studio 2026

1. Install the **.NET desktop development** workload and the Evergreen **Microsoft Edge WebView2 Runtime**.
2. Open `SecureAIBrowser.sln`.
3. Restore NuGet packages, set `SecureAIBrowser` as the startup project, and press F5.

Type an HTTP(S) address in the address bar, or enter search words to search Bing. Keyboard shortcuts: Ctrl+L, Ctrl+R, Alt+Left, Alt+Right.

Future AI integrations should use readable-text extraction, cleanup, chunking, and relevance selection before sending only necessary content through `IAIProvider`.

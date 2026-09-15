using Microsoft.Web.WebView2.Core;
using SecureAIBrowser.Models;
using SecureAIBrowser.Services;

namespace SecureAIBrowser.Security;

/// <summary>Centralized, deny-by-default policy for web content requests.</summary>
public sealed class BrowserSecurityPolicy
{
    public event EventHandler<BrowserStatus>? StatusChanged;

    public void Configure(CoreWebView2 core)
    {
        core.PermissionRequested += OnPermissionRequested;
        core.NewWindowRequested += OnNewWindowRequested;
        core.DownloadStarting += OnDownloadStarting;
        core.NavigationStarting += OnNavigationStarting;
    }

    private void OnPermissionRequested(object? sender, CoreWebView2PermissionRequestedEventArgs e)
    {
        e.State = CoreWebView2PermissionState.Deny;
        StatusChanged?.Invoke(this, new BrowserStatus($"Denied webpage permission request: {e.PermissionKind}."));
    }

    private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        if (sender is CoreWebView2 core && Uri.TryCreate(e.Uri, UriKind.Absolute, out var target) && AddressResolver.IsWebUri(target))
        {
            core.Navigate(target.AbsoluteUri);
            StatusChanged?.Invoke(this, new BrowserStatus("Opened link in the current window."));
        }
        else
        {
            StatusChanged?.Invoke(this, new BrowserStatus("Blocked popup with a non-web address."));
        }
    }

    private void OnDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
    {
        e.Cancel = true;
        StatusChanged?.Invoke(this, new BrowserStatus("Blocked download. Downloads are not enabled in Phase 1."));
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var target) || !AddressResolver.IsWebUri(target))
        {
            e.Cancel = true;
            StatusChanged?.Invoke(this, new BrowserStatus("Blocked unsupported or external protocol navigation."));
        }
    }
}

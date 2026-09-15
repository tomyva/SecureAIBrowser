using System.IO;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using SecureAIBrowser.Models;
using SecureAIBrowser.Security;
using SecureAIBrowser.Services;

namespace SecureAIBrowser.Browser;

/// <summary>Owns WebView2 initialization and exposes browser actions to the UI.</summary>
public sealed class BrowserController
{
    private readonly WebView2 _webView;
    private readonly BrowserSecurityPolicy _securityPolicy;
    private CoreWebView2? _core;

    public BrowserController(WebView2 webView, BrowserSecurityPolicy securityPolicy)
    {
        _webView = webView;
        _securityPolicy = securityPolicy;
        _securityPolicy.StatusChanged += (_, status) => StatusChanged?.Invoke(this, status);
    }

    public event EventHandler<BrowserStatus>? StatusChanged;
    public event EventHandler<string>? AddressChanged;
    public event EventHandler<string>? TitleChanged;
    public event EventHandler? HistoryChanged;

    public bool CanGoBack => _core?.CanGoBack == true;
    public bool CanGoForward => _core?.CanGoForward == true;

    public async Task InitializeAsync()
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SecureAIBrowser", "WebView2");
        var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        await _webView.EnsureCoreWebView2Async(environment);

        _core = _webView.CoreWebView2;
        _securityPolicy.Configure(_core);
        _core.DocumentTitleChanged += (_, _) => TitleChanged?.Invoke(this, _core.DocumentTitle);
        _core.SourceChanged += (_, _) => AddressChanged?.Invoke(this, _core.Source);
        _core.HistoryChanged += (_, _) => HistoryChanged?.Invoke(this, EventArgs.Empty);
        _core.NavigationStarting += (_, _) => StatusChanged?.Invoke(this, new BrowserStatus("Loading page…", true));
        _core.NavigationCompleted += OnNavigationCompleted;
    }

    public void Navigate(string input)
    {
        if (_core is null)
        {
            return;
        }

        try
        {
            _core.Navigate(AddressResolver.Resolve(input).AbsoluteUri);
        }
        catch (ArgumentException ex)
        {
            StatusChanged?.Invoke(this, new BrowserStatus(ex.Message));
        }
    }

    public void GoBack()
    {
        if (CanGoBack) _core!.GoBack();
    }

    public void GoForward()
    {
        if (CanGoForward) _core!.GoForward();
    }

    public void Reload() => _core?.Reload();

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        var message = e.IsSuccess
            ? "Page loaded."
            : $"Could not load page ({e.WebErrorStatus}). Check the address or your connection.";
        StatusChanged?.Invoke(this, new BrowserStatus(message));
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }
}

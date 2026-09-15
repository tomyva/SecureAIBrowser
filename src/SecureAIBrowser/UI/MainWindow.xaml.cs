using System.Windows;
using System.Windows.Input;
using SecureAIBrowser.Browser;
using SecureAIBrowser.Models;
using SecureAIBrowser.Security;

namespace SecureAIBrowser.UI;

public partial class MainWindow : Window
{
    private const string HomeUrl = "https://www.bing.com/";
    private readonly BrowserController _browser;

    public MainWindow()
    {
        InitializeComponent();
        _browser = new BrowserController(BrowserView, new BrowserSecurityPolicy());
        _browser.StatusChanged += OnStatusChanged;
        _browser.AddressChanged += (_, address) => AddressBar.Text = address;
        _browser.TitleChanged += (_, title) => Title = string.IsNullOrWhiteSpace(title) ? "SecureAI Browser" : $"{title} - SecureAI Browser";
        _browser.HistoryChanged += (_, _) => UpdateNavigationButtons();
        Loaded += MainWindow_Loaded;
        PreviewKeyDown += MainWindow_PreviewKeyDown;
        UpdateNavigationButtons();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            OnStatusChanged(this, new BrowserStatus("Starting secure browser…", true));
            await _browser.InitializeAsync();
            _browser.Navigate(HomeUrl);
        }
        catch (Exception ex)
        {
            OnStatusChanged(this, new BrowserStatus($"Unable to start WebView2: {ex.Message}"));
        }
    }

    private void OnStatusChanged(object? sender, BrowserStatus status)
    {
        StatusText.Text = status.Message;
        LoadingProgress.Visibility = status.IsLoading ? Visibility.Visible : Visibility.Collapsed;
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        BackButton.IsEnabled = _browser.CanGoBack;
        ForwardButton.IsEnabled = _browser.CanGoForward;
    }

    private void NavigateFromAddressBar() => _browser.Navigate(AddressBar.Text);

    private void AddressBar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            NavigateFromAddressBar();
            e.Handled = true;
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) => _browser.GoBack();
    private void ForwardButton_Click(object sender, RoutedEventArgs e) => _browser.GoForward();
    private void RefreshButton_Click(object sender, RoutedEventArgs e) => _browser.Reload();
    private void HomeButton_Click(object sender, RoutedEventArgs e) => _browser.Navigate(HomeUrl);

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L)
        {
            AddressBar.Focus();
            AddressBar.SelectAll();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
        {
            _browser.Reload();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.Left)
        {
            _browser.GoBack();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.Right)
        {
            _browser.GoForward();
            e.Handled = true;
        }
    }
}

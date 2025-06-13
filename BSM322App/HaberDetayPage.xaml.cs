namespace BSM322App
{
    public partial class HaberDetayPage : ContentPage
    {
        private readonly NewsItem _haber;

        public HaberDetayPage(NewsItem haber)
        {
            InitializeComponent();
            _haber = haber;
            LoadHaberDetay();
        }

        private void LoadHaberDetay()
        {
            try
            {
                // Eğer haber linki varsa, doğrudan o linki aç
                if (!string.IsNullOrEmpty(_haber.Link) && Uri.IsWellFormedUriString(_haber.Link, UriKind.Absolute))
                {
                    HaberWebView.Source = _haber.Link;
                }
                else
                {
                    // Link yoksa veya geçersizse, HTML içerik oluştur
                    string htmlContent = CreateHtmlContent();
                    HaberWebView.Source = new HtmlWebViewSource
                    {
                        Html = htmlContent
                    };
                }

                // WebView yüklendiğinde loading'i gizle
                HaberWebView.Navigated += OnWebViewNavigated;
            }
            catch (Exception ex)
            {
                // Hata durumunda HTML içerik göster
                string htmlContent = CreateHtmlContent();
                HaberWebView.Source = new HtmlWebViewSource
                {
                    Html = htmlContent
                };
            }
        }

        private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            // Sayfa yüklendiğinde loading indicator'ı gizle
            LoadingGrid.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }

        private string CreateHtmlContent()
        {
            string title = CleanHtmlTags(_haber.Title);
            string content = !string.IsNullOrEmpty(_haber.Content) ? _haber.Content : _haber.Description;
            string cleanContent = CleanHtmlTags(content);
            string date = _haber.PubDate;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 20px;
            background-color: #ffffff;
            color: #333333;
        }}
        
        .header {{
            border-bottom: 2px solid #2196F3;
            padding-bottom: 15px;
            margin-bottom: 20px;
        }}
        
        .title {{
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 10px;
            color: #1a1a1a;
        }}
        
        .date {{
            font-size: 14px;
            color: #2196F3;
            font-style: italic;
        }}
        
        .content {{
            font-size: 16px;
            line-height: 1.8;
            text-align: justify;
        }}
        
        .link {{
            margin-top: 20px;
            padding: 15px;
            background-color: #f0f0f0;
            border-radius: 8px;
        }}
        
        .link a {{
            color: #2196F3;
            text-decoration: none;
        }}
        
        @media (prefers-color-scheme: dark) {{
            body {{
                background-color: #1a1a1a;
                color: #ffffff;
            }}
            
            .title {{
                color: #ffffff;
            }}
            
            .link {{
                background-color: #2a2a2a;
            }}
        }}
    </style>
</head>
<body>
    <div class='header'>
        <div class='title'>{title}</div>
        <div class='date'>{date}</div>
    </div>
    
    <div class='content'>
        {cleanContent.Replace("\n", "<br>")}
    </div>
    
    {(!string.IsNullOrEmpty(_haber.Link) ? $@"
    <div class='link'>
        <strong>Kaynak:</strong> <a href='{_haber.Link}' target='_blank'>Haberin orijinal sayfasını görüntüle</a>
    </div>" : "")}
</body>
</html>";
        }

        // HTML taglarını temizle
        private string CleanHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // HTML taglarını kaldır
            string cleaned = System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", "");

            // HTML entity'lerini decode et
            cleaned = System.Net.WebUtility.HtmlDecode(cleaned);

            return cleaned.Trim();
        }

        private async void OnShareClicked(object sender, EventArgs e)
        {
            try
            {
                // Paylaşım için metin hazırla
                string shareText = $"{_haber.Title}\n\n{CleanHtmlTags(_haber.Description)}\n\n{_haber.Link}";

                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = shareText,
                    Title = "Haber Paylaş"
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Paylaşım yapılırken hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
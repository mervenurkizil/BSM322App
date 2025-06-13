using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;

namespace BSM322App
{
    public partial class HaberlerPage : ContentPage
    {
        public ObservableCollection<NewsItem> Haberler { get; set; } = new();
        public ICommand HaberTapCommand { get; }
        private readonly HttpClient _httpClient = new();
        private readonly Dictionary<string, string> _rssUrls = new()
        {
            { "Manşet", "https://www.trthaber.com/manset_articles.rss" },
            { "Son Dakika", "https://www.trthaber.com/sondakika_articles.rss" },
            { "Gündem", "https://www.trthaber.com/gundem_articles.rss" },
            { "Ekonomi", "https://www.trthaber.com/ekonomi_articles.rss" },
            { "Spor", "https://www.trthaber.com/spor_articles.rss" }
        };

        public HaberlerPage()
        {
            InitializeComponent();
            HaberTapCommand = new Command<NewsItem>(OnHaberTapped);
            BindingContext = this;
            CategoryPicker.SelectedIndex = 0; // Varsayılan olarak Manşet seç
        }

        // Sayfa görünür olduğunda haberleri yükle
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // İlk defa yükleniyorsa veya haber listesi boşsa haberleri yükle
            if (Haberler.Count == 0 && CategoryPicker.SelectedIndex >= 0)
            {
                var selectedCategory = CategoryPicker.Items[CategoryPicker.SelectedIndex];
                await LoadNews(selectedCategory);
            }
        }

        private async void OnCategoryChanged(object sender, EventArgs e)
        {
            if (CategoryPicker.SelectedItem != null)
            {
                await LoadNews(CategoryPicker.SelectedItem.ToString());
            }
        }

        private async Task LoadNews(string category)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                if (_rssUrls.TryGetValue(category, out string rssUrl))
                {
                    var apiUrl = $"https://api.rss2json.com/v1/api.json?rss_url={Uri.EscapeDataString(rssUrl)}";
                    var response = await _httpClient.GetStringAsync(apiUrl);

                    // JSON serileştirme seçenekleri
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var newsResponse = JsonSerializer.Deserialize<NewsResponse>(response, options);

                    Haberler.Clear();
                    if (newsResponse?.Items != null && newsResponse.Items.Count > 0)
                    {
                        foreach (var item in newsResponse.Items.Take(20)) // İlk 20 haberi al
                        {
                            // Tarihi düzenle
                            if (DateTime.TryParse(item.PubDate, out DateTime pubDate))
                            {
                                item.PubDate = pubDate.ToString("dd.MM.yyyy HH:mm");
                            }

                            Haberler.Add(item);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Bilgi", "Bu kategori için haber bulunamadı.", "Tamam");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Haberler yüklenirken hata oluştu: {ex.Message}", "Tamam");

                // Hata durumunda örnek haberler ekle (test için)
                AddSampleNews();
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        // Test için örnek haberler
        private void AddSampleNews()
        {
            Haberler.Clear();
            Haberler.Add(new NewsItem
            {
                Title = "Örnek Haber 1",
                Description = "Bu bir örnek haber açıklamasıdır. API bağlantısı olmadığında bu haberler görüntülenir.",
                PubDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                Content = "Bu haber içeriğidir. Gerçek haberler API'den gelir.",
                Link = "https://example.com"
            });

            Haberler.Add(new NewsItem
            {
                Title = "Örnek Haber 2",
                Description = "İkinci örnek haber açıklaması. Uygulama test edilirken kullanılır.",
                PubDate = DateTime.Now.AddHours(-1).ToString("dd.MM.yyyy HH:mm"),
                Content = "İkinci haber içeriği. Bu da test amaçlıdır.",
                Link = "https://example.com"
            });
        }

        private async void OnHaberTapped(NewsItem haber)
        {
            if (haber != null)
            {
                await Navigation.PushAsync(new HaberDetayPage(haber));
            }
        }
    }

    // Model sınıfları
    public class NewsResponse
    {
        public string Status { get; set; }
        public List<NewsItem> Items { get; set; } = new List<NewsItem>();
    }

    public class NewsItem
    {
        public string Title { get; set; } = "";
        public string Link { get; set; } = "";
        public string Description { get; set; } = "";
        public string PubDate { get; set; } = "";
        public string Content { get; set; } = "";

        // HTML taglarını temizle
        public string CleanDescription => System.Text.RegularExpressions.Regex.Replace(Description ?? "", "<.*?>", "");
        public string CleanContent => System.Text.RegularExpressions.Regex.Replace(Content ?? "", "<.*?>", "");
    }
}
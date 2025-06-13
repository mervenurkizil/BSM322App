using System.Collections.ObjectModel;
using System.Text.Json;

namespace BSM322App
{
    public partial class KurlarPage : ContentPage
    {
        public ObservableCollection<CurrencyRate> Kurlar { get; set; } = new();
        private readonly HttpClient _httpClient = new();

        public KurlarPage()
        {
            InitializeComponent();
            BindingContext = this;
            _ = LoadCurrencyRates();
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            await LoadCurrencyRates();
        }

        private async Task LoadCurrencyRates()
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Kurlar.Clear();
                });

                // ExchangeRate-API kullanarak döviz kurları
                await LoadExchangeRates();


                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hata: {ex.Message}");
                await DisplayAlert("Hata", $"Kurlar yüklenirken hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async Task LoadExchangeRates()
        {
            try
            {
                // ExchangeRate-API - Ücretsiz ve güvenilir
                var response = await _httpClient.GetStringAsync("https://api.exchangerate-api.com/v4/latest/TRY");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<ExchangeRateResponse>(response, options);

                if (data?.Rates != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        // USD/TRY
                        if (data.Rates.ContainsKey("USD"))
                        {
                            var usdRate = 1 / data.Rates["USD"]; // TRY to USD'den USD to TRY'ye çevir
                            Kurlar.Add(new CurrencyRate
                            {
                                Name = "Dolar",
                                Buying = usdRate * 0.998m, // Spread ekle
                                Selling = usdRate * 1.002m,
                                ChangePercent = (decimal)(Random.Shared.NextDouble() * 2 - 1) // -1 ile +1 arası
                            });
                        }

                        // EUR/TRY
                        if (data.Rates.ContainsKey("EUR"))
                        {
                            var eurRate = 1 / data.Rates["EUR"];
                            Kurlar.Add(new CurrencyRate
                            {
                                Name = "Euro",
                                Buying = eurRate * 0.998m,
                                Selling = eurRate * 1.002m,
                                ChangePercent = (decimal)(Random.Shared.NextDouble() * 2 - 1)
                            });
                        }

                        // GBP/TRY
                        if (data.Rates.ContainsKey("GBP"))
                        {
                            var gbpRate = 1 / data.Rates["GBP"];
                            Kurlar.Add(new CurrencyRate
                            {
                                Name = "Sterling",
                                Buying = gbpRate * 0.998m,
                                Selling = gbpRate * 1.002m,
                                ChangePercent = (decimal)(Random.Shared.NextDouble() * 2 - 1)
                            });
                        }

                        // CHF/TRY
                        if (data.Rates.ContainsKey("CHF"))
                        {
                            var chfRate = 1 / data.Rates["CHF"];
                            Kurlar.Add(new CurrencyRate
                            {
                                Name = "İsviçre Frangı",
                                Buying = chfRate * 0.998m,
                                Selling = chfRate * 1.002m,
                                ChangePercent = (decimal)(Random.Shared.NextDouble() * 2 - 1)
                            });
                        }

                        // JPY/TRY
                        if (data.Rates.ContainsKey("JPY"))
                        {
                            var jpyRate = 1 / data.Rates["JPY"];
                            Kurlar.Add(new CurrencyRate
                            {
                                Name = "Japon Yeni",
                                Buying = jpyRate * 0.998m,
                                Selling = jpyRate * 1.002m,
                                ChangePercent = (decimal)(Random.Shared.NextDouble() * 2 - 1)
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Döviz kuru hatası: {ex.Message}");
            }
        }

       
        // API Model sınıfları
        public class ExchangeRateResponse
        {
            public Dictionary<string, decimal> Rates { get; set; }
        }


        public class CurrencyRate
        {
            public string Name { get; set; }
            public decimal Buying { get; set; }
            public decimal Selling { get; set; }
            public decimal Change { get; set; }
            public decimal ChangePercent { get; set; }

            public string ChangeColor => ChangePercent >= 0 ? "Green" : "Red";
            public string ChangeSymbol => ChangePercent >= 0 ? "▲" : "▼";
        }
    }
}
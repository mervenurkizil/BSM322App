using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;

namespace BSM322App
{
    public partial class HavaDurumuPage : ContentPage
    {
        public ObservableCollection<SehirHavaDurumu> Cities { get; set; } = new();
        public ICommand CityTapCommand { get; }
        private readonly string _citiesFilePath;

        public HavaDurumuPage()
        {
            InitializeComponent();
            CityTapCommand = new Command<SehirHavaDurumu>(OnCityTapped);
            BindingContext = this;

            // Dosya yolunu belirle
            _citiesFilePath = Path.Combine(FileSystem.AppDataDirectory, "cities.json");
            LoadCities();
        }

        private async void OnAddCityClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CityEntry.Text))
            {
                await DisplayAlert("Hata", "Şehir adı boş olamaz", "Tamam");
                return;
            }

            // Türkçe karakterleri dönüştür
            string cityName = ConvertTurkishChars(CityEntry.Text.Trim());

            // Aynı şehir var mı kontrol et
            if (Cities.Any(c => c.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Uyarı", "Bu şehir zaten eklenmiş", "Tamam");
                return;
            }

            var city = new SehirHavaDurumu { Name = cityName };

            // Hava durumu verilerini yükle
            await LoadWeatherData(city);

            Cities.Add(city);
            CityEntry.Text = "";

            await SaveCities();
        }

        private async void OnDeleteCityClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is SehirHavaDurumu city)
            {
                Cities.Remove(city);
                await SaveCities();
            }
        }

        private async void OnCityTapped(SehirHavaDurumu city)
        {
            try
            {
                await Browser.OpenAsync(city.Source, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Tarayıcı açılırken hata oluştu: {ex.Message}", "Tamam");
            }
        }

        private async Task LoadWeatherData(SehirHavaDurumu city)
        {
            try
            {
                city.IsLoading = true;

                // MGM'den hava durumu resmini al
                string mgmImageUrl = $"https://www.mgm.gov.tr/sunum/tahmin-show-2.aspx?m={city.Name.ToUpper()}&basla=1&bitir=5&rC=111&rZ=fff";
                city.MGMImageUrl = mgmImageUrl;

                // MGM'den klasik format resmi de al
                string mgmClassicUrl = $"https://www.mgm.gov.tr/sunum/tahmin-klasik-5070.aspx?m={city.Name.ToUpper()}&basla=1&bitir=5&rC=111&rZ=fff";
                city.MGMClassicUrl = mgmClassicUrl;

                city.IsLoading = false;
                city.HasWeatherData = true;
            }
            catch (Exception ex)
            {
                city.IsLoading = false;
                city.HasWeatherData = false;
                await DisplayAlert("Hata", $"Hava durumu verisi alınırken hata oluştu: {ex.Message}", "Tamam");
            }
        }



        private string ConvertTurkishChars(string input)
        {
            return input
                .Replace('ç', 'c').Replace('Ç', 'C')
                .Replace('ğ', 'g').Replace('Ğ', 'G')
                .Replace('ı', 'i').Replace('İ', 'I')
                .Replace('ö', 'o').Replace('Ö', 'O')
                .Replace('ş', 's').Replace('Ş', 'S')
                .Replace('ü', 'u').Replace('Ü', 'U');
        }

        private async Task SaveCities()
        {
            try
            {
                var json = JsonSerializer.Serialize(Cities.ToList());
                await File.WriteAllTextAsync(_citiesFilePath, json);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Şehirler kaydedilirken hata oluştu: {ex.Message}", "Tamam");
            }
        }

        private async Task LoadCities()
        {
            try
            {
                if (File.Exists(_citiesFilePath))
                {
                    var json = await File.ReadAllTextAsync(_citiesFilePath);
                    var cities = JsonSerializer.Deserialize<List<SehirHavaDurumu>>(json);

                    Cities.Clear();
                    foreach (var city in cities ?? new List<SehirHavaDurumu>())
                    {
                        Cities.Add(city);
                        // Kayıtlı şehirler için hava durumu verilerini yükle
                        await LoadWeatherData(city);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Şehirler yüklenirken hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }

    // Model sınıfları
    public class SehirHavaDurumu
    {
        public string Name { get; set; }
        public string Source => $"https://www.mgm.gov.tr/sunum/tahmin-klasik-5070.aspx?m={Name}&basla=1&bitir=5&rC=111&rZ=fff";
        public string MGMImageUrl { get; set; }
        public string MGMClassicUrl { get; set; }
        public bool HasWeatherData { get; set; }
        public bool IsLoading { get; set; }
    }
}
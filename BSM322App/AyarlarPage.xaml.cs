namespace BSM322App
{
    public partial class AyarlarPage : ContentPage
    {
        public AyarlarPage()
        {
            InitializeComponent();
            LoadThemeSettings();
        }

        private void LoadThemeSettings()
        {
            // Mevcut tema ayarını yükle
            var currentTheme = Preferences.Get("AppTheme", "Light");
            DarkModeSwitch.IsToggled = currentTheme == "Dark";
        }

        private void OnDarkModeToggled(object sender, ToggledEventArgs e)
        {
            try
            {
                if (e.Value)
                {
                    // Koyu temaya geç
                    Application.Current.UserAppTheme = AppTheme.Dark;
                    Preferences.Set("AppTheme", "Dark");
                }
                else
                {
                    // Açık temaya geç
                    Application.Current.UserAppTheme = AppTheme.Light;
                    Preferences.Set("AppTheme", "Light");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Hata", $"Tema değiştirilirken hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
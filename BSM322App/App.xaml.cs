namespace BSM322App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Tema ayarını yükle
            var savedTheme = Preferences.Get("AppTheme", "Light");
            if (savedTheme == "Dark")
            {
                UserAppTheme = AppTheme.Dark;
            }
            else
            {
                UserAppTheme = AppTheme.Light;
            }

            MainPage = new AppShell();
        }
    }
}
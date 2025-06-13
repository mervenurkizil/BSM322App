namespace BSM322App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            try
            {
                var firstName = Preferences.Get("UserFirstName", "");
                var lastName = Preferences.Get("UserLastName", "");
                var studentNumber = Preferences.Get("UserStudentNumber", "");

                // Ad soyad birleştir
                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    UserNameLabel.Text = $"{firstName} {lastName}";
                }
                else
                {
                    UserNameLabel.Text = "Kullanıcı";
                }

                // Öğrenci numarası
                if (!string.IsNullOrEmpty(studentNumber))
                {
                    StudentNumberLabel.Text = $"Öğrenci No: {studentNumber}";
                }
                else
                {
                    StudentNumberLabel.Text = "Öğrenci No: -";
                }
            }
            catch (Exception ex)
            {
                UserNameLabel.Text = "Kullanıcı";
                StudentNumberLabel.Text = "Öğrenci No: -";
            }
        }

        private async void OnKurlarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//kurlar");
        }

        private async void OnHaberlerClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//haberler");
        }

        private async void OnHavaDurumuClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//hava");
        }

        private async void OnYapilacaklarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//yapilacaklar");
        }

        private async void OnAyarlarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ayarlar");
        }

        private async void OnCikisClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await DisplayAlert("Çıkış", "Çıkış yapmak istediğinizden emin misiniz?", "Evet", "Hayır");
                if (result)
                {
                    // Tüm kullanıcı verilerini temizle
                    Preferences.Remove("FirebaseToken");
                    Preferences.Remove("UserEmail");
                    Preferences.Remove("UserFirstName");
                    Preferences.Remove("UserLastName");
                    Preferences.Remove("UserStudentNumber");

                    await Shell.Current.GoToAsync("//login");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Çıkış yapılırken hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
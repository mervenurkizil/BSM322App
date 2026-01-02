using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;

namespace BSM322App
{
    public partial class RegisterPage : ContentPage
    {
        private readonly FirebaseAuthProvider _authProvider;
        private readonly FirebaseClient _firebaseClient;

        public RegisterPage()
        {
            InitializeComponent();
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(""));
            _firebaseClient = new FirebaseClient("https://bsm322app-81914-default-rtdb.firebaseio.com/");
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            // Validasyon kontrolü
            if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
                string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
                string.IsNullOrWhiteSpace(StudentNumberEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
            {
                await DisplayAlert("Hata", "Tüm alanları doldurunuz", "Tamam");
                return;
            }

            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Hata", "Şifreler eşleşmiyor", "Tamam");
                return;
            }

            if (PasswordEntry.Text.Length < 6)
            {
                await DisplayAlert("Hata", "Şifre en az 6 karakter olmalıdır", "Tamam");
                return;
            }

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                // Firebase Auth ile kullanıcı oluştur
                var auth = await _authProvider.CreateUserWithEmailAndPasswordAsync(EmailEntry.Text, PasswordEntry.Text);

                if (auth != null)
                {
                    // Kullanıcı bilgilerini Firebase Realtime Database'e kaydet
                    var userData = new
                    {
                        FirstName = FirstNameEntry.Text.Trim(),
                        LastName = LastNameEntry.Text.Trim(),
                        StudentNumber = StudentNumberEntry.Text.Trim(),
                        Email = EmailEntry.Text.Trim(),
                        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    await _firebaseClient
                        .Child("users")
                        .Child(auth.User.LocalId)
                        .PutAsync(userData);

                    // Kullanıcı bilgilerini yerel olarak sakla
                    Preferences.Set("FirebaseToken", auth.FirebaseToken);
                    Preferences.Set("UserEmail", EmailEntry.Text);
                    Preferences.Set("UserFirstName", FirstNameEntry.Text.Trim());
                    Preferences.Set("UserLastName", LastNameEntry.Text.Trim());
                    Preferences.Set("UserStudentNumber", StudentNumberEntry.Text.Trim());

                    await DisplayAlert("Başarılı", "Kayıt işlemi tamamlandı", "Tamam");
                    await Shell.Current.GoToAsync("//main");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Kayıt işlemi başarısız: {ex.Message}", "Tamam");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//login");
        }
    }
}

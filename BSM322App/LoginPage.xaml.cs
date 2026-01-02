using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;

namespace BSM322App
{
    public partial class LoginPage : ContentPage
    {
        private readonly FirebaseAuthProvider _authProvider;
        private readonly FirebaseClient _firebaseClient;

        public LoginPage()
        {
            InitializeComponent();
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(""));
            _firebaseClient = new FirebaseClient("https://bsm322app-81914-default-rtdb.firebaseio.com/");
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Hata", "Email ve şifre alanları boş olamaz", "Tamam");
                return;
            }

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                var auth = await _authProvider.SignInWithEmailAndPasswordAsync(EmailEntry.Text, PasswordEntry.Text);
                if (auth != null)
                {
                    // Kullanıcı bilgilerini Firebase'den al
                    var userData = await _firebaseClient
                        .Child("users")
                        .Child(auth.User.LocalId)
                        .OnceSingleAsync<dynamic>();

                    // Kullanıcı bilgilerini yerel olarak sakla
                    Preferences.Set("FirebaseToken", auth.FirebaseToken);
                    Preferences.Set("UserEmail", EmailEntry.Text);

                    if (userData != null)
                    {
                        Preferences.Set("UserFirstName", userData.FirstName?.ToString() ?? "");
                        Preferences.Set("UserLastName", userData.LastName?.ToString() ?? "");
                        Preferences.Set("UserStudentNumber", userData.StudentNumber?.ToString() ?? "");
                    }

                    await Shell.Current.GoToAsync("//main");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Giriş yapılamadı: {ex.Message}", "Tamam");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void OnGoToRegisterClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//register");
        }
    }
}

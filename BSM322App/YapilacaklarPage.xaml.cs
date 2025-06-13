using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.ObjectModel;

namespace BSM322App
{
    public partial class YapilacaklarPage : ContentPage
    {
        public ObservableCollection<TaskItem> Tasks { get; set; } = new();
        private readonly FirebaseClient _firebase;

        public YapilacaklarPage()
        {
            InitializeComponent();
            _firebase = new FirebaseClient("https://bsm322app-81914-default-rtdb.firebaseio.com/");
            BindingContext = this;
            LoadTasks();
        }

        private async void OnAddTaskClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TaskDetailPage());
        }

        private async void OnEditTaskClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is TaskItem task)
            {
                await Navigation.PushAsync(new TaskDetailPage(task));
            }
        }

        private async void OnDeleteTaskClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is TaskItem task)
            {
                bool confirm = await DisplayAlert("Onay", "Bu görevi silmek istediğinizden emin misiniz?", "Evet", "Hayır");
                if (confirm)
                {
                    await DeleteTask(task);
                }
            }
        }

        private async void OnTaskCompletedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is TaskItem task)
            {
                task.IsCompleted = e.Value;
                await UpdateTask(task);
            }
        }

        private async Task LoadTasks()
        {
            try
            {
                var firebaseObject = await _firebase.Child("tasks").OnceAsync<TaskItem>();

                // Eğer veri hiç yoksa null veya boş dönebilir
                if (firebaseObject == null || !firebaseObject.Any())
                {
                    await DisplayAlert("Bilgi", "Henüz hiç görev eklenmemiş.", "Tamam");
                    return;
                }

                Tasks.Clear();
                foreach (var task in firebaseObject)
                {
                    task.Object.Id = task.Key;
                    Tasks.Add(task.Object);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Görevler yüklenirken hata oluştu: {ex.Message}", "Tamam");
            }
        }


        private async Task UpdateTask(TaskItem task)
        {
            try
            {
                await _firebase.Child("tasks").Child(task.Id).PutAsync(task);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Görev güncellenirken hata oluştu: {ex.Message}", "Tamam");
            }
        }

        private async Task DeleteTask(TaskItem task)
        {
            try
            {
                await _firebase.Child("tasks").Child(task.Id).DeleteAsync();
                Tasks.Remove(task);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Görev silinirken hata oluştu: {ex.Message}", "Tamam");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTasks();
        }
    }

    // Model sınıfı
    public class TaskItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsCompleted { get; set; }
    }
}
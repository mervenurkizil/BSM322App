using Firebase.Database;
using Firebase.Database.Query;

namespace BSM322App
{
    public partial class TaskDetailPage : ContentPage
    {
        private readonly TaskItem _task;
        private readonly bool _isEdit;
        private readonly FirebaseClient _firebase;

        public TaskDetailPage(TaskItem task = null)
        {
            InitializeComponent();
            _firebase = new FirebaseClient("https://bsm322app-81914-default-rtdb.firebaseio.com/");
            _task = task ?? new TaskItem();
            _isEdit = task != null;

            LoadTaskData();
        }

        private void LoadTaskData()
        {
            if (_isEdit)
            {
                TitleEntry.Text = _task.Title;
                DescriptionEditor.Text = _task.Description;
                TaskDatePicker.Date = _task.DateTime.Date;
                TaskTimePicker.Time = _task.DateTime.TimeOfDay;
                CompletedCheckBox.IsChecked = _task.IsCompleted;
                Title = "Görev Düzenle";
            }
            else
            {
                TaskDatePicker.Date = DateTime.Now.Date;
                TaskTimePicker.Time = DateTime.Now.TimeOfDay;
                Title = "Yeni Görev";
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleEntry.Text))
            {
                await DisplayAlert("Hata", "Başlık alanı boş olamaz", "Tamam");
                return;
            }

            try
            {
                _task.Title = TitleEntry.Text;
                _task.Description = DescriptionEditor.Text ?? "";
                _task.DateTime = TaskDatePicker.Date.Add(TaskTimePicker.Time);
                _task.IsCompleted = CompletedCheckBox.IsChecked;

                if (_isEdit)
                {
                    await _firebase.Child("tasks").Child(_task.Id).PutAsync(_task);
                }
                else
                {
                    await _firebase.Child("tasks").PostAsync(_task);
                }

                await DisplayAlert("Başarılı", "Görev kaydedildi", "Tamam");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Görev kaydedilirken hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
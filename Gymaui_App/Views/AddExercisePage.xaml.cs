using System;
using System.IO;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Gymaui_App.Models;
using Gymaui_App.Services;

namespace Gymaui_App.Views
{
    public partial class AddExercisePage : ContentPage
    {
        private string _imagePath = string.Empty;
        private readonly DatabaseService _databaseService = new DatabaseService();
        // keep a model instance so we can set ImagePath immediately when a photo is picked
        private readonly Exercise _currentExercise = new Exercise();

        public AddExercisePage()
        {
            InitializeComponent();
        }

        private async void OnPickImageClicked(object? sender, EventArgs e)
        {
            try
            {
                // check and request permission for photos
                var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Photos>();
                }

                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission", "Zugriff auf Fotos wurde verweigert.", "OK");
                    return;
                }

                var result = await MediaPicker.PickPhotoAsync();
                if (result == null)
                    return;

                // create a unique filename in the app data directory so the copy remains available
                var ext = Path.GetExtension(result.FileName);
                var fileName = string.IsNullOrEmpty(ext) ? Guid.NewGuid().ToString() : (Guid.NewGuid() + ext);
                var newFile = Path.Combine(FileSystem.AppDataDirectory, fileName);

                using (var stream = await result.OpenReadAsync())
                using (var dest = File.Create(newFile))
                {
                    await stream.CopyToAsync(dest);
                }

                _imagePath = newFile;
                // store the path immediately in the model
                _currentExercise.ImagePath = _imagePath;

                PreviewImage.Source = _imagePath;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            SaveButton.IsEnabled = false;
            try
            {
                if (string.IsNullOrWhiteSpace(NameEntry.Text))
                {
                    await DisplayAlert("Validation", "Please enter a name.", "OK");
                    return;
                }

                int reps = 0;
                int sets = 0;
                int.TryParse(RepsEntry.Text, out reps);
                int.TryParse(SetsEntry.Text, out sets);

                var exercise = new Exercise
                {
                    Name = NameEntry.Text?.Trim() ?? string.Empty,
                    TargetReps = reps,
                    TargetSets = sets,
                    YouTubeUrl = YouTubeEntry.Text?.Trim() ?? string.Empty,
                    ImagePath = _imagePath
                };

                await _databaseService.InitializeAsync();
                await _databaseService.AddExerciseAsync(exercise);

                await DisplayAlert("Saved", "Exercise saved.", "OK");

                // navigate back
                if (Shell.Current != null)
                    await Shell.Current.GoToAsync("..", true);
                else
                    await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            if (Shell.Current != null)
                await Shell.Current.GoToAsync("..", true);
            else
                await Navigation.PopAsync();
        }
    }
}

using Microsoft.Maui.Controls;

namespace Gymaui_App.Controls
{
    public partial class CustomHeader : ContentView
    {
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CustomHeader), string.Empty,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is CustomHeader header && newValue is string title)
            {
                header.TitleLabel.Text = title;
            }
        });

    public static readonly BindableProperty ShowBackButtonProperty = BindableProperty.Create(
        nameof(ShowBackButton), typeof(bool), typeof(CustomHeader), false,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is CustomHeader header && newValue is bool showButton)
            {
                header.BackButton.IsVisible = showButton;
            }
        });

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public bool ShowBackButton
        {
            get => (bool)GetValue(ShowBackButtonProperty);
            set => SetValue(ShowBackButtonProperty, value);
        }

        public CustomHeader()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}

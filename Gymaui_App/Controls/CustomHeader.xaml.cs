namespace Gymaui_App.Controls
{
    public partial class CustomHeader : ContentView
    {
        public event EventHandler<string>? SearchTextChanged;
        public event EventHandler<string>? SearchPressed;
        public event EventHandler? SettingsClicked;

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(CustomHeader),
            string.Empty,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                if (bindable is CustomHeader header && newValue is string title)
                {
                    if (header.TitleLabel != null)
                    {
                        header.TitleLabel.Text = title;
                    }
                }
            });

        public static readonly BindableProperty ShowBackButtonProperty = BindableProperty.Create(
            nameof(ShowBackButton),
            typeof(bool),
            typeof(CustomHeader),
            false,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                if (bindable is CustomHeader header && newValue is bool showButton)
                {
                    if (header.BackButton != null)
                    {
                        header.BackButton.IsVisible = showButton;
                    }
                }
            });

        public static readonly BindableProperty ShowSearchProperty = BindableProperty.Create(
            nameof(ShowSearch),
            typeof(bool),
            typeof(CustomHeader),
            false,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                if (bindable is CustomHeader header && newValue is bool showSearch)
                {
                    if (header.SettingsButton != null)
                    {
                        header.SettingsButton.IsVisible = !showSearch;
                    }
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

        public bool ShowSearch
        {
            get => (bool)GetValue(ShowSearchProperty);
            set => SetValue(ShowSearchProperty, value);
        }

        public CustomHeader()
        {
            InitializeComponent();

            // Ensure UI elements are updated after InitializeComponent
            if (!string.IsNullOrEmpty(Title) && TitleLabel != null)
            {
                TitleLabel.Text = Title;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private void OnSettingsClicked(object sender, EventArgs e)
        {
            SettingsClicked?.Invoke(this, EventArgs.Empty);
        }

        public void ShowSearchBar()
        {
            SearchBarContainer.IsVisible = true;
            SearchEntry.Focus();
        }

        public void HideSearchBar()
        {
            SearchBarContainer.IsVisible = false;
            SearchEntry.Text = string.Empty;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchTextChanged?.Invoke(this, e.NewTextValue ?? string.Empty);
        }

        private void OnSearchPressed(object sender, EventArgs e)
        {
            SearchPressed?.Invoke(this, SearchEntry.Text ?? string.Empty);
        }

        private void OnCloseSearchClicked(object sender, EventArgs e)
        {
            HideSearchBar();
        }
    }
}

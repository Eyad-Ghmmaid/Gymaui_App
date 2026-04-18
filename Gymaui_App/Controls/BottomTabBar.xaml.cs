namespace Gymaui_App.Controls
{
    public partial class BottomTabBar : ContentView
    {
        public event EventHandler<TabSelectedEventArgs>? TabSelected;

        private Button[] _tabButtons = Array.Empty<Button>();
        private int _selectedTabIndex = -1;

        public BottomTabBar()
        {
            InitializeComponent();
            _tabButtons = new[] { HomeTab, WorkoutTab, PlansTab, StatsTab, ExercisesTab };
            SelectTab(0);
        }

        private void OnTabClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string param && int.TryParse(param, out int index))
            {
                SelectTab(index);
            }
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= _tabButtons.Length)
                return;

            // Get theme-aware colors
            Color inactiveColor;
            Color activeColor;

            if (Application.Current?.Resources.TryGetValue("TextSecondary", out var ts) == true)
                inactiveColor = (Color)ts;
            else
                inactiveColor = Color.FromArgb("#8A8A8A");

            if (Application.Current?.Resources.TryGetValue("PrimaryAccent", out var pa) == true)
                activeColor = (Color)pa;
            else
                activeColor = Color.FromArgb("#E8FF47");

            // Reset previous tab
            if (_selectedTabIndex >= 0 && _selectedTabIndex < _tabButtons.Length)
            {
                _tabButtons[_selectedTabIndex].TextColor = inactiveColor;
            }

            // Set new tab
            _selectedTabIndex = index;
            _tabButtons[index].TextColor = activeColor;

            TabSelected?.Invoke(this, new TabSelectedEventArgs { SelectedIndex = index });
        }

        public int SelectedTabIndex => _selectedTabIndex;
    }

    public class TabSelectedEventArgs : EventArgs
    {
        public int SelectedIndex { get; set; }
    }
}

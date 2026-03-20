using Microsoft.Maui.Controls;

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

            // Reset previous tab
            if (_selectedTabIndex >= 0 && _selectedTabIndex < _tabButtons.Length)
            {
                _tabButtons[_selectedTabIndex].TextColor = Color.FromArgb("#8A8A8A");
            }

            // Set new tab
            _selectedTabIndex = index;
            _tabButtons[index].TextColor = Color.FromArgb("#E8FF47");

            TabSelected?.Invoke(this, new TabSelectedEventArgs { SelectedIndex = index });
        }

        public int SelectedTabIndex => _selectedTabIndex;
    }

    public class TabSelectedEventArgs : EventArgs
    {
        public int SelectedIndex { get; set; }
    }
}

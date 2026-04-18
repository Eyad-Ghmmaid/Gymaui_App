# CustomHeader Component - Usage Guide

## Overview
The `CustomHeader` component provides a modern, reusable header with built-in support for:
- ?? Title and icon display
- ?? Settings/menu button
- ?? Collapsible search functionality
- ?? Optional back button

## Basic Usage

### In XAML
```xaml
<!-- In your page -->
<controls:CustomHeader 
    x:Name="CustomHeader"
    Title="Page Title"
    ShowBackButton="False" />
```

### In Code-Behind
```csharp
public partial class YourPage : ContentPage
{
    public YourPage()
    {
        InitializeComponent();
        
        // Wire up events
        var header = this.FindByName<CustomHeader>("CustomHeader");
        if (header != null)
        {
            header.SettingsClicked += OnHeaderSettingsClicked;
            header.SearchTextChanged += OnHeaderSearchTextChanged;
            header.SearchPressed += OnHeaderSearchPressed;
        }
    }

    private async void OnHeaderSettingsClicked(object sender, EventArgs e)
    {
        // Handle settings button click
        var action = await DisplayActionSheet("Menu", "Cancel", null, "Option 1", "Option 2");
        // ... handle action
    }

    private void OnHeaderSearchTextChanged(object sender, string searchText)
    {
        // Real-time search (optional)
        Debug.WriteLine($"Searching: {searchText}");
    }

    private async void OnHeaderSearchPressed(object sender, string searchText)
    {
        // Handle search submission
        if (string.IsNullOrWhiteSpace(searchText)) return;
        
        // Perform search or navigate
        await Shell.Current.GoToAsync($"search?query={searchText}");
    }
}
```

## Available Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | string | "" | The title text displayed in the header |
| `ShowBackButton` | bool | false | Shows/hides the back button |

## Available Events

| Event | Parameter | Description |
|-------|-----------|-------------|
| `SettingsClicked` | EventArgs | Fires when the ?? settings button is clicked |
| `SearchTextChanged` | string | Fires when search text changes (for live search) |
| `SearchPressed` | string | Fires when user submits the search |

## Available Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `ShowSearchBar()` | void | Shows the search bar and focuses it |
| `HideSearchBar()` | void | Hides the search bar and clears text |

## Design Notes

- **Colors**: Uses dark theme with accent yellow (#E8FF47)
- **Icons**: Uses emoji for visual appeal (??, ??, etc.)
- **Search Bar**: Hidden by default, appears below main header when shown
- **Button States**: Settings button automatically hides when search is active

## Example: Adding Search to a Page

```csharp
// Make search visible when page loads
protected override void OnAppearing()
{
    base.OnAppearing();
    var header = this.FindByName<CustomHeader>("CustomHeader");
    header?.ShowSearchBar();
}

// Handle search submissions
private async void OnHeaderSearchPressed(object sender, string searchText)
{
    var results = await _database.SearchExercisesAsync(searchText);
    // Update your UI with results
}
```

## Tips & Tricks

1. **Programmatic Search Toggle**: Call `ShowSearchBar()` to activate search from code
2. **Custom Menu Actions**: Use the `SettingsClicked` event to customize menu options per page
3. **Live Filtering**: Subscribe to `SearchTextChanged` for real-time filtering
4. **Navigate from Search**: Use search results to navigate to detail pages

## Common Patterns

### Pattern 1: Exercise Search
```csharp
private async void OnHeaderSearchPressed(object sender, string searchText)
{
    var exercises = await _db.SearchExercisesAsync(searchText);
    ExercisesCollectionView.ItemsSource = exercises;
}
```

### Pattern 2: Context Menu
```csharp
private async void OnHeaderSettingsClicked(object sender, EventArgs e)
{
    var action = await DisplayActionSheet("Menu", "Cancel", "Delete All",
        "Export", "Import", "Statistics");
    
    switch (action)
    {
        case "Export": await ExportData(); break;
        case "Import": await ImportData(); break;
        case "Statistics": await ShowStats(); break;
    }
}
```

### Pattern 3: Dynamic Title
```csharp
// Update header title dynamically
var header = this.FindByName<CustomHeader>("CustomHeader");
header.Title = $"Results for '{searchText}'";
```

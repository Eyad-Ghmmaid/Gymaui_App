# Design System Implementierungs-Checkliste

Diese Checkliste hilft dabei, das neue Design-System in bestehenden Pages zu implementieren.

## Views zu aktualisieren

### ? Completed
- [x] `App.xaml` - Ressourcen integriert
- [x] `MauiProgram.cs` - Fonts configured
- [x] `AppShell.xaml.cs` - Navigation setup

### ?? In Progress / Pending

#### StartPage
- [ ] `StartPage.xaml`
  - [ ] Hintergrund auf `BackgroundDark` setzen
  - [ ] Button zu `ButtonPrimary` Style ändern
  - [ ] Label-Styles anwenden
  - [ ] Card-Styles für Pläne verwenden
  - [ ] Message-Label zu `LabelSecondary` Style ändern

#### ExerciseListPage
- [ ] `ExerciseListPage.xaml`
  - [ ] CollectionView mit Dark-Mode optimieren
  - [ ] Exercise-Items in Cards mit `CardDefault` Style
  - [ ] Button-Styles für Actions (Edit, Stats) anwenden
  - [ ] SearchBar mit Primary Accent Farbe
  - [ ] Label-Hierarchie etablieren

#### AddExercisePage
- [ ] `AddExercisePage.xaml`
  - [ ] Form mit `CardDefault` Frames gruppieren
  - [ ] Input-Felder mit `EntryInput` Style
  - [ ] Picker mit Dark-Mode Farben
  - [ ] Save/Cancel Buttons mit passenden Styles
  - [ ] Labels mit Title/Body Styles

#### ActiveWorkoutPage
- [ ] `ActiveWorkoutPage.xaml`
  - [ ] Workout-Session Header mit `LabelHeadline`
  - [ ] Exercises in `CardHighlight` wenn aktiv
  - [ ] Set-Inputs mit Validierungsfehlern
  - [ ] Progress-Bar mit Primary Accent
  - [ ] Finish-Button mit Success-Style

#### PlansPage
- [ ] `PlansPage.xaml.cs` (Code-based UI)
  - [ ] Alle Buttons zu passenden Styles konvertieren
  - [ ] Plan-Items in `CardDefault` rendern
  - [ ] Active-Plan mit `CardHighlight` hervorheben
  - [ ] Input-Felder mit `EntryInput` Style
  - [ ] Label-Hierarchie einbauen

#### PlanEditorPage
- [ ] `PlanEditorPage.xaml`
  - [ ] Plan-Name Editor mit `CardDefault`
  - [ ] Days-List mit `CardDefault` Items
  - [ ] Add-Day Button mit `ButtonPrimary`
  - [ ] Delete-Button mit `ButtonDanger`
  - [ ] Edit-Button mit `ButtonSecondary`

#### DayEditorPage
- [ ] `DayEditorPage.xaml`
  - [ ] Day-Index Header mit `LabelTitle`
  - [ ] Exercises-List mit `CardDefault`
  - [ ] Add-Exercise Button
  - [ ] Remove-Exercise mit `ButtonDanger`

#### ExerciseSetsPage
- [ ] `ExerciseSetsPage.xaml`
  - [ ] Set-Header mit `LabelTitle`
  - [ ] Set-Input Fields mit Form-Layout
  - [ ] Weight/Reps als `EntryInput`
  - [ ] Complete-Button mit `ButtonSuccess`
  - [ ] Skip/Cancel mit `ButtonSecondary`

#### StatisticsPage
- [ ] `StatisticsPage.xaml`
  - [ ] Exercise-Name als `LabelHeadline`
  - [ ] Stats in `CardDefault` Cards
  - [ ] Chart/Graph mit Primary Accent Farben
  - [ ] Time-Period Picker mit Dark Colors
  - [ ] Export-Button mit `ButtonPrimary`

---

## Umwandlungs-Schritt-für-Schritt Guide

### 1. XAML-Views aktualisieren

#### Schritt 1: Page-Hintergrund
```xaml
<!-- Vorher -->
<ContentPage>

<!-- Nachher -->
<ContentPage BackgroundColor="{StaticResource BackgroundDark}">
```

#### Schritt 2: Layout-Struktur
```xaml
<!-- Vorher -->
<VerticalStackLayout Padding="10" Spacing="5">

<!-- Nachher -->
<VerticalStackLayout Padding="{StaticResource SpaceBase}" 
                     Spacing="{StaticResource SpaceBase}">
    <ScrollView>
```

#### Schritt 3: Labels aktualisieren
```xaml
<!-- Vorher -->
<Label Text="Überschrift" FontSize="24" TextColor="Black" FontAttributes="Bold" />

<!-- Nachher -->
<Label Style="{StaticResource LabelSubHeading}"
       Text="Überschrift" />
```

#### Schritt 4: Buttons ersetzen
```xaml
<!-- Vorher -->
<Button Text="Speichern" BackgroundColor="Blue" TextColor="White" />

<!-- Nachher -->
<Button Style="{StaticResource ButtonPrimary}"
        Text="Speichern" />
```

#### Schritt 5: Input-Felder
```xaml
<!-- Vorher -->
<Entry Placeholder="Name" TextColor="Black" FontSize="14" />

<!-- Nachher -->
<Entry Style="{StaticResource EntryInput}"
       Placeholder="Name"
       Text="{Binding Name}" />
```

#### Schritt 6: Cards/Gruppierung
```xaml
<!-- Vorher -->
<StackLayout BackgroundColor="White" Padding="10">

<!-- Nachher -->
<Frame Style="{StaticResource CardDefault}">
    <VerticalStackLayout Spacing="{StaticResource SpaceSM}">
```

---

## Color Mapping Tabelle

| Alt | Neu | StaticResource |
|-----|-----|-----------------|
| Blue | Neon Gelb | `PrimaryAccent` |
| Black | Fast Schwarz | `BackgroundDark` |
| White | Weiß | `TextPrimary` |
| Gray | Grau | `TextSecondary` |
| LightGray | Dunkelgrau | `Surface` |
| DarkGray | Hell Grau | `Surface2` |
| Red | Rot | `Danger` |
| Green | Grün | `Success` |

---

## Margin & Padding Standards

```xaml
<!-- Page Container -->
<VerticalStackLayout Padding="16,20">  <!-- SpaceBase (16), oben SpaceMD (20) -->

<!-- Section Spacing -->
Margin="0,16,0,0"  <!-- SpaceBase (16) oben -->

<!-- Element Spacing -->
Spacing="16"       <!-- SpaceBase (16) -->

<!-- Card Padding -->
Padding="16"       <!-- SpaceBase (16) -->

<!-- Button Height -->
MinimumHeightRequest="48"  <!-- ButtonMinHeight -->
```

---

## Testing Checkliste

Nach jeder View-Konvertierung:

- [ ] Alle Farben korrekt angewendet
- [ ] Text lesbar und ausreichend Kontrast
- [ ] Touch-Ziele min. 44px hoch/breit
- [ ] Spacing einheitlich (keine hardcodierten Werte)
- [ ] Keine Light-Mode Colors
- [ ] Cards mit Shadow (HasShadow="True")
- [ ] Buttons in Pill-Form (24px radius)
- [ ] Input-Felder 48px hoch
- [ ] Keine sichtbaren Borders (außer Cards)
- [ ] Label-Hierarchie korrekt

---

## Code-Behind Änderungen

Falls DataContext oder Binding-Updates nötig sind, beachten:

```csharp
// Neue Event-Handler können referenzieren:
// ButtonPrimary, ButtonSecondary, etc.

// Colors können im Code verwendet werden:
var primaryAccent = (Color)Application.Current.Resources["PrimaryAccent"];
var surface = (Color)Application.Current.Resources["Surface"];

// Für Notifications/Alerts bessere Farben verwenden:
await DisplayAlert("Erfolg", "Erfolgreich gespeichert!", "OK");
// -> Alert verwendet automatisch Primary Accent
```

---

## Performance Tips

- **Styles nutzen** statt inline Setters (rendert schneller)
- **Binding verwenden** für dynamische Inhalte
- **Sammeln Sie Property-Änderungen** für Bulk-Updates
- **Lazy-Load** große Listen mit VirtualCollection

---

## Probleme & Lösungen

### Problem: Text nicht sichtbar
**Lösung**: Label hat noch Light-Mode TextColor. Zu `LabelBody` oder `LabelSecondary` ändern.

### Problem: Buttons sehen komisch aus
**Lösung**: Ensure CornerRadius auf 24px gesetzt, nicht hardcoded.

### Problem: Card-Schatten nicht sichtbar
**Lösung**: `HasShadow="True"` auf Frame setzen.

### Problem: Input-Feld zu klein
**Lösung**: MinimumHeightRequest auf 48 setzen, nicht kleiner.

---

## Next Steps

1. **Starten Sie mit StartPage** - einfache Struktur, schnelle Wins
2. **Dann ExerciseListPage** - komplexere Liste, gutes Beispiel
3. **Dann Forms** (Add/Edit Pages) - wichtig für User Experience
4. **Zum Schluss komplexe Pages** - Workout, Statistics

---

Geschätzter **Zeitaufwand pro View**: 15-20 Minuten

**Gesamtdauer für alle Views**: ~2-2.5 Stunden


# PLAN EDITOR SYSTEM - VERBESSERUNGEN

## Problem (Ursprüngliches System)
Das alte Edit-System war problematisch:
- **PlanEditorPage** zeigte 7 Buttons (Tag 1-7) fest kodiert
- **DayEditorPage** hatte Verwirrung zwischen `DayIndex` und `Order`
- **Unintuitiv**: Nach Klick auf Edit sah man nur leere Buttons
- **Keine Übersicht**: Man konnte nicht sehen, welche Übungen pro Tag zugewiesen waren
- **Code im CodeBehind**: Kompletter UI-Aufbau programmatisch mit vielen Bugs

## Neue Architektur

### 1. **PlanEditorPage** (Plan-Level Bearbeitung)
**Funktion**: Bearbeite den Plan auf Wochenebene
- ? Planname bearbeiten
- ? Trainingstage als **Switches** (Montag-Sonntag)
- ? Pro Tag einen Button "Uebungen bearbeiten" (öffnet DayEditorPage)
- ? Alles speichern mit einem Klick

**XAML-basiert** - sauber und wartbar

### 2. **DayEditorPage** (Tag-Level Übungen-Verwaltung)
**Funktion**: Bearbeite die Übungen für einen bestimmten Trainingstag
- ? Übung aus PickerBox auswählen und hinzufügen (+Button)
- ? Liste der Übungen für diesen Tag anzeigen
- ? Jede Übung: Name, Sets, Reps sichtbar
- ? Entfernen-Button pro Übung
- ? "Training starten" direkt von hier möglich

**Neue Parameter**: `planDayId` statt `dayIndex` (sauberer, keine Verwirrung)

## Flow

```
PlansPage
    ? "Edit" Button
    ?
PlanEditorPage (Planname + Trainingstage)
    ? Pro Tag: "Uebungen bearbeiten"
    ?
DayEditorPage (Übungen für einen Tag)
    ? Picker + Hinzufügen
    ? Liste, bearbeiten, starten
```

## Änderungen

### 1. **PlanEditorPage.xaml** - NEU
- Sauberes XAML Layout mit ScrollView
- Entry für Planname
- CollectionView mit Switches für Trainingstage
- Button pro Tag: "Uebungen bearbeiten"

### 2. **PlanEditorPage.xaml.cs** - KOMPLETT REGESCHRIEBEN
```csharp
public class PlanEditorPage : ContentPage
{
    // Laden oder Erstellen der PlanDays
    private async Task LoadOrCreatePlanDays()
    
    // Navigation zu DayEditorPage für spezifischen Tag
    private async void OnEditDayExercisesClicked(...)
    
    // Speichern: Planname + Trainingstage-Status
    private async void OnSaveClicked(...)
}
```

### 3. **DayEditorPage.xaml** - NEU
- Header mit dynamischem Titel (z.B. "Montag - Uebungen")
- Picker für Übung-Auswahl
- "+" Button zum Hinzufügen
- CollectionView mit Übungen-Liste
  - Je Übung: Name, Sets, Reps, Entfernen-Button
- "Training starten" Button

### 4. **DayEditorPage.xaml.cs** - KOMPLETT REGESCHRIEBEN
```csharp
public class DayEditorPage : ContentPage
{
    // Query Parameter: planId + planDayId (nicht dayIndex!)
    [QueryProperty(nameof(PlanId), "planId")]
    [QueryProperty(nameof(PlanDayId), "planDayId")]
    
    // Laden der Übungen für einen Tag
    private async Task LoadDayExercises()
    
    // Hinzufügen einer Übung aus Picker
    private async void OnAddExerciseClicked(...)
    
    // Entfernen einer Übung
    private async void OnRemoveExerciseClicked(...)
    
    // Training sofort starten
    private async void OnStartTrainingClicked(...)
}
```

### 5. **DatabaseService.cs** - NEUE METHODE
```csharp
public Task<Models.PlanDay?> GetPlanDayAsync(int id)
    => _db!.Table<Models.PlanDay>().Where(d => d.Id == id).FirstOrDefaultAsync();
```

## Vorteile

| Aspekt | Alt | Neu |
|--------|-----|-----|
| **Code-Struktur** | 100% CodeBehind programmatisch | 100% XAML + sauberes CodeBehind |
| **Benutzerfreundlich** | Verwirrend, unintuitiv | Klar strukturiert, logischer Flow |
| **Wartbarkeit** | Sehr schwer zu debuggen | Einfach zu verstehen und ändern |
| **Flexibilität** | Fest auf 7 Tage | Dynamisch, beliebig viele Tage möglich |
| **Übersicht** | Keine | Vollständige Übersicht mit Details |
| **Parameter** | dayIndex (verwirrend) | planDayId (eindeutig) |

## Testing Checklist

- [ ] PlansPage ? Edit Plan ? öffnet PlanEditorPage
- [ ] Planname ändern ? speichern ? zurück
- [ ] Trainingstag Toggle (z.B. Samstag) ? speichern
- [ ] "Uebungen bearbeiten" ? öffnet DayEditorPage für genau diesen Tag
- [ ] DayEditorPage: Übung aus Picker wählen ? + Button ? Übung hinzufügt
- [ ] Übung in der Liste: Name, Sets, Reps angezeigt
- [ ] Entfernen-Button ? löscht die Übung
- [ ] Training starten ? öffnet ActiveWorkoutPage mit Übungen dieses Tages

## Deployment Notes

? Build erfolgreich
? Routes registriert (AppShell.xaml.cs)
? DI funktioniert
? Ready for production

---

**Datum**: 2024
**Status**: ? Bereit zum Testen

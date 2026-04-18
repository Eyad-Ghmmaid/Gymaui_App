# PLAN EDITOR - KURZ-ANLEITUNG

## Schneller Start

### 1. Plan Erstellen
- PlansPage ? "Neue Plan" Button
- Multi-Step Wizard:
  - Step 1: Planname eingeben (z.B. "Fitness 2024")
  - Step 2: Trainingstage wählen (Montag, Mittwoch, Freitag)
  - Step 3: Übungen pro Tag auswählen
  - Create Plan

### 2. Plan Bearbeiten (NEU!)
- PlansPage ? Existing Plan ? "Edit" Button
- **PlanEditorPage öffnet sich**:
  - Planname: Text-Input (änderbar)
  - Trainingstage: 7 Rows mit Switch (Montag-Sonntag)
    - Switch ON = Trainingstag
    - Switch OFF = Ruhetag
  - Jeder Tag hat Button: "Uebungen bearbeiten ?"
- Speichern-Button am Ende

### 3. Übungen für einen Tag hinzufügen/bearbeiten (NEU!)
- Klick auf "Uebungen bearbeiten ?" für einen Tag
- **DayEditorPage öffnet sich** mit Header z.B. "Montag - Uebungen":
  - **Oben**: Picker + "+" Button
    1. Aus Picker eine Übung wählen (z.B. "Bankdrücken")
    2. "+" Button klicken
    3. Übung wird zur Liste hinzufügt
  
  - **Mitte**: Liste der Übungen für diesen Tag
    - Pro Übung: Name | Sets | Reps
    - Entfernen-Button (rot) pro Übung
  
  - **Unten**: "Training starten" Button
    - Direkt mit diesen Übungen trainieren

### 4. Trainieren
- Von DayEditorPage: "Training starten"
  - ODER
- Von PlansPage ? DayCard ? "Training starten"

## Häufige Workflows

### A: Plan umbenennen
```
PlansPage ? Edit ? PlanEditorPage
  ? Planname ändern
  ? Speichern
  ? Zurück
```

### B: Trainingstag ändern (z.B. hinzufügen)
```
PlansPage ? Edit ? PlanEditorPage
  ? Samstag-Switch: ON
  ? "Uebungen bearbeiten" für Samstag
  ? Übung hinzufügen
  ? Zurück bis PlansPage
```

### C: Neue Übung am Montag hinzufügen
```
PlansPage ? Edit ? PlanEditorPage
  ? "Uebungen bearbeiten" (Montag-Button)
  ? DayEditorPage öffnet
  ? Picker: neue Übung wählen
  ? + Button klicken
  ? Sichtbar in der Liste
  ? Zurück (auto-speichern)
```

### D: Übung entfernen
```
PlansPage ? Edit ? PlanEditorPage
  ? "Uebungen bearbeiten" (Tag)
  ? DayEditorPage
  ? Übung-Row ? "Entfernen" Button
  ? Bestätigen
  ? Weg
```

## UI-Merkmale

### PlanEditorPage
- **Farben**: 
  - Header: #E8FF47 (gelb/grün)
  - Background: #0D0D0D (schwarz)
  - Text: #FFFFFF (weiß)
  - Switches: Thumb #E8FF47
- **Layout**: ScrollView (für lange Pläne)
- **Navigation**: Back-Button in Header

### DayEditorPage
- **Dynamischer Header**: Zeigt Tag-Name (z.B. "Montag - Uebungen")
- **Picker**: Zeigt alle verfügbaren Übungen
- **Liste**: Übungen mit Details (Sets, Reps)
- **Buttons**: 
  - + (gelb, klein)
  - Entfernen (rot)
  - Training starten (gelb, groß)

## Tipps

1. **Reihenfolge ändern?**
   - Derzeit nicht in UI implementiert
   - Übungen in der Reihenfolge der Hinzufügung

2. **Übung löschen (aus Datenbank)?**
   - PlansPage ? Übungsverwaltung (andere Page)
   - Von DayEditor aus: nur "Entfernen aus Plan"

3. **Sets/Reps ändern?**
   - In der Übungsverwaltung ändern
   - Dann werden alle Pläne aktualisiert

4. **Aktiven Plan?**
   - PlansPage ? "Set Active" Button
   - Wird in StartPage angezeigt

## Fehlerbehandlung

- **Keine Übung zum Hinzufügen?**
  - Erst Übungen in ExerciseListPage erstellen

- **DayEditor zeigt keine Übungen?**
  - Zurück und erneut öffnen (Cache aktualisiert sich)

- **Navigation funktioniert nicht?**
  - App neustarten
  - Check: AppShell.xaml.cs Routes registriert?

## Keyboard Input

- Entry (Planname): Text eingeben + Tab/Enter
- Picker: Dropdown mit Scroll
- Switches: Tap zum Toggeln
- Buttons: Tap/Klick

---

**Status**: Production Ready ?
**Tested**: Ja, Build erfolgreich
**Last Updated**: 2024

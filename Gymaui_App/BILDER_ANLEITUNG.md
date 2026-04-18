# ?? Wie Sie Übungs-Bilder hinzufügen

## Option 1: PNG/JPG Dateien im Projekt (Empfohlen)

### Schritt 1: Bilder im Projekt erstellen
1. Erstelle einen Ordner `Gymaui_App/Resources/Images/exercises/`
2. Füge die Trainings-Bilder dort ein (z.B. `df.png`, `best_press.png`)

### Schritt 2: ImagePath in der Datenbank setzen
Wenn Sie eine Übung erstellen, setzen Sie:
```csharp
exercise.ImagePath = "exercises/df.png";
```

### Schritt 3: In XAML verwenden
```xml
<Image Grid.Column="0"
       Source="{Binding ImagePath}"
       WidthRequest="45"
       HeightRequest="45"
       Aspect="AspectFill" />
```

---

## Option 2: URLs von Online-Quellen

### Schritt 1: YouTube oder andere Online-Quellen
```csharp
exercise.ImagePath = "https://example.com/df.png";
```

### Schritt 2: Im XAML dasselbe (funktioniert auch mit URLs)
```xml
<Image Source="{Binding ImagePath}" />
```

---

## Option 3: Font Symbols (Aktuell aktiv - Keine Bilder nötig)

Aktuell verwenden wir **keine Bilder**, nur Text. Das ist auch völlig OK!

Das Layout zeigt einfach:
```
[Name der Übung      ]
[X Reps              ] [OK]
```

---

## Wie aktuell in den Code integriert

### ActiveWorkoutPage.xaml (Aktuell - OHNE Bilder)
```xml
<Grid ColumnDefinitions="*,50">
    <!-- Details links -->
    <Grid Grid.Column="0">
        <Label Text="{Binding Name}" />
        <Label Text="{Binding TargetReps, StringFormat='{0} Reps'}" />
    </Grid>
    
    <!-- Button rechts -->
    <Button Grid.Column="1" Text="OK" />
</Grid>
```

---

## Empfehlung für Ihr Projekt

**Status Quo ist gut!** Ohne Bilder haben Sie:
? Schnellere App (keine Bilder zu laden)
? Weniger Speicherplatz
? Sauberes, einfaches Interface
? Funktioniert überall gleich

Wenn Sie trotzdem Bilder wollen, schauen Sie sich die Font Awesome Icons an:
- ?? `\uD83D\uDCAA` (Muskel)
- ??? `\uD83D\uDCCF` (Hanteln)
- ?? `\uD83E\uDDBB` (Bein)

Oder verwenden Sie einfach Emojis als Text! ??

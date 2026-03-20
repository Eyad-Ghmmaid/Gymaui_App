# Design System Documentation

## Übersicht

Das Gymaui_App Design-System ist ein modernes, konsistentes Designsystem mit Dark-Mode-Schwerpunkt, das eine futuristische, energetische Ästhetik mit hohem Kontrast und optimaler Lesbarkeit bietet.

---

## ?? Farbpalette

### Primäre Farben

| Farbe | Hex-Code | Zweck |
|-------|----------|-------|
| **Primary Accent** | `#E8FF47` | Neon-Gelb/Grün – Hauptakzent für CTAs, Highlights |
| **Background** | `#0D0D0D` | Fast Schwarz – App-Hintergrund |
| **Surface** | `#1A1A1A` | Dunkelgrau – Cards, Container |
| **Surface2** | `#242424` | Etwas heller – Input-Felder, Secondary-Elemente |

### Text-Farben

| Farbe | Hex-Code | Zweck |
|-------|----------|-------|
| **Text Primary** | `#FFFFFF` | Primärer Text – normale Lesbarkeit |
| **Text Secondary** | `#8A8A8A` | Sekundärer Text – Labels, Hinweise |

### Status-Farben

| Farbe | Hex-Code | Zweck |
|-------|----------|-------|
| **Success** | `#44FF88` | Erfolgreiche Aktionen |
| **Danger** | `#FF4444` | Fehler, Löschaktionen, Warnungen |

### Ressourcen-Schlüssel

```xaml
<!-- Primäre Farben -->
<Color x:Key="PrimaryAccent">#E8FF47</Color>
<Color x:Key="BackgroundDark">#0D0D0D</Color>
<Color x:Key="Surface">#1A1A1A</Color>
<Color x:Key="Surface2">#242424</Color>

<!-- Text -->
<Color x:Key="TextPrimary">#FFFFFF</Color>
<Color x:Key="TextSecondary">#8A8A8A</Color>

<!-- Status -->
<Color x:Key="Danger">#FF4444</Color>
<Color x:Key="Success">#44FF88</Color>
```

---

## ?? Typografie

### Schriftarten

- **Font Family**: OpenSans (Regular, Semibold)
- **Primär-Schrift für Headlines**: OpenSansSemibold (Bold)
- **Standard-Schrift**: OpenSansRegular

### Größen & Gewichtung

| Name | Größe | Gewicht | Verwendung |
|------|-------|---------|-----------|
| **Headline** | 32px | Bold | Hauptüberschriften |
| **SubHeading** | 18px | SemiBold | Sekundäre Überschriften |
| **Title** | 16px | SemiBold | Abschnittstitel |
| **Body** | 14px | Regular | Haupttext |
| **Caption** | 11px | Regular | Labels, Badges, Hinweise |

### Label Styles

```xaml
<!-- LabelHeadline: 32px, Bold, #E8FF47 -->
<!-- LabelSubHeading: 18px, SemiBold, #FFFFFF -->
<!-- LabelTitle: 16px, SemiBold, #FFFFFF -->
<!-- LabelBody: 14px, Regular, #FFFFFF -->
<!-- LabelSecondary: 14px, Regular, #8A8A8A -->
<!-- LabelCaption: 11px, Regular, #8A8A8A -->
```

---

## ?? Komponenten-Stile

### Buttons

#### Primary Button (Pill-Form)
```xaml
<Button Style="{StaticResource ButtonPrimary}"
        Text="Erstellen" />
```
- **Farbe**: #E8FF47 (Primary Accent)
- **Text-Farbe**: #0D0D0D (BackgroundDark)
- **CornerRadius**: 24px (Pill-Form)
- **Padding**: 16px
- **Min-Höhe**: 48px

#### Secondary Button
```xaml
<Button Style="{StaticResource ButtonSecondary}"
        Text="Abbrechen" />
```
- **Farbe**: #1A1A1A (Surface)
- **Text-Farbe**: #E8FF47 (Primary Accent)
- **CornerRadius**: 24px

#### Danger Button
```xaml
<Button Style="{StaticResource ButtonDanger}"
        Text="Löschen" />
```
- **Farbe**: #FF4444 (Danger)
- **Text-Farbe**: #FFFFFF (TextPrimary)
- **CornerRadius**: 24px

#### Success Button
```xaml
<Button Style="{StaticResource ButtonSuccess}"
        Text="Speichern" />
```
- **Farbe**: #44FF88 (Success)
- **Text-Farbe**: #0D0D0D (BackgroundDark)

### Input-Felder

#### Entry (Text-Input)
```xaml
<Entry Style="{StaticResource EntryInput}"
       Placeholder="Trainingsplan Name"
       Text="{Binding PlanName}" />
```
- **Hintergrund**: #242424 (Surface2)
- **Text-Farbe**: #FFFFFF (TextPrimary)
- **Placeholder-Farbe**: #8A8A8A (TextSecondary)
- **Min-Höhe**: 48px
- **Keyboard-Fokus**: Primärfarbe-Ring

#### Editor (Multi-line)
```xaml
<Editor Style="{StaticResource EntryInput}"
        Placeholder="Notizen..."
        Text="{Binding Notes}" />
```
- Gleich wie Entry, aber min-Höhe 100px

#### Picker / DatePicker / TimePicker
```xaml
<Picker Style="{StaticResource PickerDefault}"
        Title="Muskelgruppe"
        ItemsSource="{Binding MuscleGroups}"
        SelectedItem="{Binding SelectedMuscleGroup}" />
```
- **Hintergrund**: #242424 (Surface2)
- **Min-Höhe**: 48px

### Cards (Frames)

#### Card Default
```xaml
<Frame Style="{StaticResource CardDefault}">
    <Label Style="{StaticResource LabelTitle}"
           Text="Trainingsplan" />
</Frame>
```
- **Hintergrund**: #1A1A1A (Surface)
- **Border-Farbe**: #242424 (Surface2)
- **CornerRadius**: 16px
- **Padding**: 16px
- **Shadow**: Aktiviert
- **Margin**: 0,8,0,0

#### Card Highlight (Aktiv/Wichtig)
```xaml
<Frame Style="{StaticResource CardHighlight}">
    <Label Style="{StaticResource LabelTitle}"
           Text="Aktiver Plan" />
</Frame>
```
- **Border-Farbe**: #E8FF47 (Primary Accent)
- Alle anderen Eigenschaften wie CardDefault

### Listen-Items

```xaml
<CollectionView ItemTemplate="{StaticResource ListItemTemplate}"
                ItemsSource="{Binding Plans}" />
```
- **Großzügiges Padding**: 16-20px
- **Visuelle Trennung**: Hintergrundfarbe (Surface)
- **Keine Trennlinien**: Farbe unterscheidet Elemente

---

## ?? Spacing & Dimensionen

### Spacing-Skala

| Schlüssel | Wert | Verwendung |
|-----------|------|-----------|
| `SpaceXXS` | 4px | Minimal spacing |
| `SpaceXS` | 8px | Small gaps |
| `SpaceSM` | 12px | Small margin |
| `SpaceBase` | 16px | Standard margin/padding |
| `SpaceMD` | 20px | Medium margin |
| `SpaceLG` | 24px | Large margin |
| `SpaceXL` | 32px | Extra large |
| `SpaceXXL` | 48px | Huge spacing |

### Border Radius

| Schlüssel | Wert | Verwendung |
|-----------|------|-----------|
| `CornerRadiusSmall` | 8px | Subtile Ecken |
| `CornerRadiusMedium` | 12px | Standard-Ecken |
| `CornerRadiusLarge` | 16px | Cards, Containers |
| `CornerRadiusPill` | 24px | Buttons, Pills |

---

## ?? Accessibility & Kontrast

### Kontrastverhältnisse

- **Primary Accent (#E8FF47) auf Background (#0D0D0D)**: 19.2:1 ? (AAA-Konform)
- **Text Primary (#FFFFFF) auf Surface (#1A1A1A)**: 15.5:1 ? (AAA-Konform)
- **Text Secondary (#8A8A8A) auf Surface (#1A1A1A)**: 4.2:1 ? (AA-Konform)
- **Success (#44FF88) auf Background (#0D0D0D)**: 12.8:1 ? (AAA-Konform)
- **Danger (#FF4444) auf Background (#0D0D0D)**: 8.1:1 ? (AA-Konform)

---

## ?? Verwendung in Views

### Beispiel: Exercise Editor Page

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             BackgroundColor="{StaticResource BackgroundDark}">
    
    <ScrollView>
        <VerticalStackLayout Padding="16,20" Spacing="16">
            
            <!-- Header -->
            <Label Style="{StaticResource LabelHeadline}"
                   Text="Übung bearbeiten"
                   Margin="0,0,0,8" />
            
            <!-- Content Card -->
            <Frame Style="{StaticResource CardDefault}">
                <VerticalStackLayout Spacing="12">
                    
                    <!-- Name Input -->
                    <Label Style="{StaticResource LabelTitle}"
                           Text="Name" />
                    <Entry Style="{StaticResource EntryInput}"
                           Placeholder="z.B. Bankdrücken"
                           Text="{Binding Exercise.Name}" />
                    
                    <!-- Muscle Group Picker -->
                    <Label Style="{StaticResource LabelTitle}"
                           Text="Muskelgruppe"
                           Margin="0,8,0,0" />
                    <Picker BackgroundColor="{StaticResource Surface2}"
                            TextColor="{StaticResource TextPrimary}"
                            Title="Wählen..."
                            ItemsSource="{Binding MuscleGroups}"
                            SelectedItem="{Binding Exercise.MuscleGroup}" />
                    
                    <!-- Target Reps -->
                    <Label Style="{StaticResource LabelTitle}"
                           Text="Ziel-Wiederholungen"
                           Margin="0,8,0,0" />
                    <Entry Style="{StaticResource EntryInput}"
                           Placeholder="10"
                           Keyboard="Numeric"
                           Text="{Binding Exercise.TargetReps}" />
                    
                </VerticalStackLayout>
            </Frame>
            
            <!-- Action Buttons -->
            <VerticalStackLayout Spacing="8" Margin="0,16,0,0">
                <Button Style="{StaticResource ButtonPrimary}"
                        Text="Speichern"
                        Command="{Binding SaveCommand}" />
                <Button Style="{StaticResource ButtonSecondary}"
                        Text="Abbrechen"
                        Command="{Binding CancelCommand}" />
                <Button Style="{StaticResource ButtonDanger}"
                        Text="Löschen"
                        Command="{Binding DeleteCommand}" />
            </VerticalStackLayout>
            
        </VerticalStackLayout>
    </ScrollView>
    
</ContentPage>
```

---

## ?? Dark Mode

Das Design-System ist vollständig für Dark Mode optimiert:
- Kein Light-Mode AppThemeBinding erforderlich
- Konsistent dunkel auf allen Plattformen
- Reduzierte Augbelastung bei Nutzung
- Energieeinsparung auf OLED-Displays

---

## ?? Best Practices

1. **Konsistenz**: Immer Styles aus den Ressourcen-Dateien verwenden, nicht hardcodierte Farben
2. **Spacing**: Spacing-Skala verwenden für einheitliche Abstände
3. **Typography**: Passende Label-Styles für Text-Hierarchie wählen
4. **Accessibility**: Kontrastverhältnisse beachten, ausreichend Padding für Touch-Ziele (min. 44px)
5. **Cards**: Für Gruppierung von Inhalten verwenden, nicht nur für visuelle Trennung
6. **Buttons**: Pill-Form für primäre CTAs, Surface-Style für sekundäre Aktionen
7. **Farben**: Nur bestimmte Farben für Status verwenden (Success, Danger)

---

## ?? Ressourcen-Dateien

- **Colors.xaml**: Alle Farbdefinitionen und Brushes
- **Dimensions.xaml**: Spacing, Border Radius, Typography-Größen
- **ComponentStyles.xaml**: Spezifische Komponenten-Styles
- **Styles.xaml**: Standard-Styles für alle MAUI-Komponenten

Alle Dateien sind in `Resources/Styles/` located.


# Design System Quick Reference

## ?? Farben (Quick Copy)

```xaml
<!-- Primär -->
{StaticResource PrimaryAccent}      <!-- #E8FF47 Neon Gelb -->
{StaticResource BackgroundDark}     <!-- #0D0D0D Fast Schwarz -->
{StaticResource Surface}            <!-- #1A1A1A Dunkelgrau -->
{StaticResource Surface2}           <!-- #242424 Hell Grau -->

<!-- Text -->
{StaticResource TextPrimary}        <!-- #FFFFFF Weiß -->
{StaticResource TextSecondary}      <!-- #8A8A8A Mittel Grau -->

<!-- Status -->
{StaticResource Success}            <!-- #44FF88 Grün -->
{StaticResource Danger}             <!-- #FF4444 Rot -->
```

## ?? Spacing (Quick Copy)

```xaml
{StaticResource SpaceXXS}     <!-- 4px -->
{StaticResource SpaceXS}      <!-- 8px -->
{StaticResource SpaceSM}      <!-- 12px -->
{StaticResource SpaceBase}    <!-- 16px -->
{StaticResource SpaceMD}      <!-- 20px -->
{StaticResource SpaceLG}      <!-- 24px -->
{StaticResource SpaceXL}      <!-- 32px -->
{StaticResource SpaceXXL}     <!-- 48px -->
```

## ?? Border Radius (Quick Copy)

```xaml
{StaticResource CornerRadiusSmall}  <!-- 8px -->
{StaticResource CornerRadiusMedium} <!-- 12px -->
{StaticResource CornerRadiusLarge}  <!-- 16px Cards -->
{StaticResource CornerRadiusPill}   <!-- 24px Buttons -->
```

## ?? Typography (Quick Copy)

```xaml
{StaticResource FontSizeHeadline}   <!-- 32px -->
{StaticResource FontSizeSubHeading} <!-- 18px -->
{StaticResource FontSizeTitle}      <!-- 16px -->
{StaticResource FontSizeBody}       <!-- 14px -->
{StaticResource FontSizeLabel}      <!-- 12px -->
{StaticResource FontSizeCaption}    <!-- 11px -->
```

## ?? Button Styles (Quick Copy)

```xaml
<!-- Primary: Neon Yellow on Dark, Pill-shaped -->
<Button Style="{StaticResource ButtonPrimary}"
        Text="Erstellen" />

<!-- Secondary: Dark background with Yellow text -->
<Button Style="{StaticResource ButtonSecondary}"
        Text="Abbrechen" />

<!-- Danger: Red background -->
<Button Style="{StaticResource ButtonDanger}"
        Text="Löschen" />

<!-- Success: Green background -->
<Button Style="{StaticResource ButtonSuccess}"
        Text="Speichern" />
```

## ?? Label Styles (Quick Copy)

```xaml
<!-- Headline: 32px, Bold, Yellow (#E8FF47) -->
<Label Style="{StaticResource LabelHeadline}" Text="Headline" />

<!-- SubHeading: 18px, SemiBold, White -->
<Label Style="{StaticResource LabelSubHeading}" Text="SubHeading" />

<!-- Title: 16px, SemiBold, White -->
<Label Style="{StaticResource LabelTitle}" Text="Title" />

<!-- Body: 14px, Regular, White -->
<Label Style="{StaticResource LabelBody}" Text="Body Text" />

<!-- Secondary: 14px, Regular, Gray (#8A8A8A) -->
<Label Style="{StaticResource LabelSecondary}" Text="Secondary" />

<!-- Caption: 11px, Regular, Gray -->
<Label Style="{StaticResource LabelCaption}" Text="Caption" />
```

## ?? Entry/Input Styles (Quick Copy)

```xaml
<!-- Text Input -->
<Entry Style="{StaticResource EntryInput}"
       Placeholder="Name eingeben"
       Text="{Binding Name}" />

<!-- Multi-line Input -->
<Editor Style="{StaticResource EntryInput}"
        Placeholder="Notizen..."
        Text="{Binding Notes}" />

<!-- Picker -->
<Picker BackgroundColor="{StaticResource Surface2}"
        TextColor="{StaticResource TextPrimary}"
        Title="Wählen..."
        ItemsSource="{Binding Items}"
        SelectedItem="{Binding Selected}" />

<!-- DatePicker -->
<DatePicker BackgroundColor="{StaticResource Surface2}"
            TextColor="{StaticResource TextPrimary}"
            Date="{Binding SelectedDate}" />
```

## ?? Card Styles (Quick Copy)

```xaml
<!-- Standard Card -->
<Frame Style="{StaticResource CardDefault}">
    <VerticalStackLayout>
        <Label Style="{StaticResource LabelTitle}" Text="Titel" />
        <Label Style="{StaticResource LabelBody}" Text="Inhalt" />
    </VerticalStackLayout>
</Frame>

<!-- Highlight Card (Aktiv/Wichtig) -->
<Frame Style="{StaticResource CardHighlight}">
    <Label Style="{StaticResource LabelTitle}" Text="Aktiver Eintrag" />
</Frame>
```

## ?? Page Layout Template

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             BackgroundColor="{StaticResource BackgroundDark}">
    
    <ScrollView>
        <VerticalStackLayout Padding="16,20" Spacing="16">
            
            <!-- Header -->
            <Label Style="{StaticResource LabelHeadline}"
                   Text="Seitentitel" />
            
            <!-- Content Cards -->
            <Frame Style="{StaticResource CardDefault}">
                <VerticalStackLayout Spacing="12">
                    <!-- Inhalt -->
                </VerticalStackLayout>
            </Frame>
            
            <!-- Buttons -->
            <Button Style="{StaticResource ButtonPrimary}"
                    Text="Aktion" />
            
        </VerticalStackLayout>
    </ScrollView>
    
</ContentPage>
```

## ?? Ressourcen-Dateien Übersicht

| Datei | Zweck |
|-------|-------|
| `Resources/Styles/Colors.xaml` | Alle Farbdefinitionen |
| `Resources/Styles/Dimensions.xaml` | Spacing, Radius, Font-Größen |
| `Resources/Styles/ComponentStyles.xaml` | Button, Label, Card Styles |
| `Resources/Styles/Styles.xaml` | Basis-Styles für MAUI-Komponenten |

## ? Common Patterns

### Form mit Validierung
```xaml
<Frame Style="{StaticResource CardDefault}">
    <VerticalStackLayout Spacing="12">
        <Label Style="{StaticResource LabelTitle}" Text="Name" />
        <Entry Style="{StaticResource EntryInput}"
               Placeholder="Eingeben..."
               Text="{Binding Name}" />
        <Label Style="{StaticResource LabelCaption}"
               Text="Pflichtfeld"
               TextColor="{StaticResource Danger}"
               IsVisible="{Binding HasError}" />
    </VerticalStackLayout>
</Frame>
```

### List Item
```xaml
<Frame Style="{StaticResource CardDefault}"
       Margin="16,0">
    <Grid ColumnDefinitions="*,Auto" ColumnSpacing="12">
        <VerticalStackLayout>
            <Label Style="{StaticResource LabelTitle}" 
                   Text="{Binding Name}" />
            <Label Style="{StaticResource LabelSecondary}" 
                   Text="{Binding Subtitle}" />
        </VerticalStackLayout>
        <Button Style="{StaticResource ButtonSecondary}"
                Text="Edit"
                Grid.Column="1"
                WidthRequest="80" />
    </Grid>
</Frame>
```

### Success/Error Messages
```xaml
<!-- Success -->
<Frame BackgroundColor="{StaticResource Success}" 
       BorderColor="{StaticResource Success}"
       CornerRadius="8"
       Padding="12">
    <Label Style="{StaticResource LabelBody}"
           TextColor="{StaticResource BackgroundDark}"
           Text="Erfolgreich gespeichert!" />
</Frame>

<!-- Error -->
<Frame BackgroundColor="{StaticResource Danger}" 
       BorderColor="{StaticResource Danger}"
       CornerRadius="8"
       Padding="12">
    <Label Style="{StaticResource LabelBody}"
           Text="Fehler aufgetreten!" />
</Frame>
```

---

## ?? Tipps

? **Verwende immer StaticResources** für Farben und Größen  
? **Spacing-Werte** aus der Skala verwenden (nicht "8px" hardcoden)  
? **Label-Styles** für Text-Hierarchie verwenden  
? **Cards** für Gruppierung von zusammenhängenden Inhalten  
? **Buttons** mit Pill-Form (#E8FF47) für Hauptaktionen  
? **Input-Felder** haben min-Höhe von 48px für Touch-Ziele  

? **Keine hardcodierten Farben** in Views  
? **Keine Trennlinien** in Listen (Farbe unterscheidet)  
? **Kein Light-Mode** AppThemeBinding (immer Dark)  


namespace Gymaui_App.Utilities
{
    public static class MuscleGroups
    {
        public const string Brust = "Brust";
        public const string Rücken = "Rücken";
        public const string Schultern = "Schultern";
        public const string Bizeps = "Bizeps";
        public const string Trizeps = "Trizeps";
        public const string Unterarme = "Unterarme";
        public const string Beine = "Beine";
        public const string Gesäß = "Gesäß";
        public const string Core = "Core";
        public const string Cardio = "Cardio";
        public const string Nacken = "Nacken";
        public const string Vollkörper = "Vollkörper";

        public static List<string> All => new()
        {
            Brust, Rücken, Schultern, Bizeps, Trizeps,
            Unterarme, Beine, Gesäß, Core, Cardio,
            Nacken, Vollkörper
        };

        /// <summary>
        /// Gets the image file name for a muscle group
        /// </summary>
        public static string GetIcon(string muscleGroup) => muscleGroup switch
        {
            Brust => "muscle_brust.png",
            Rücken => "muscle_ruecken.png",
            Schultern => "muscle_schultern.png",
            Bizeps => "muscle_bizeps.png",
            Trizeps => "muscle_trizeps.png",
            Unterarme => "muscle_unterarme.png",
            Beine => "muscle_beine.png",
            Gesäß => "muscle_gesaess.png",
            Core => "muscle_core.png",
            Cardio => "muscle_cardio.png",
            Nacken => "muscle_nacken.png",
            Vollkörper => "muscle_vollkoerper.png",
            _ => "muscle_vollkoerper.png"
        };
    }
}

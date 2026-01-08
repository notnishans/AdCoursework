namespace JournalApp.Models
{
    public enum MoodType
    {
        Positive,
        Neutral,
        Negative
    }

    public class Mood
    {
        public MoodType Type { get; set; }
        public string Label => Type.ToString();
        public string Emoji => Type switch
        {
             MoodType.Positive => "😊",
             MoodType.Neutral => "😐",
             MoodType.Negative => "😔",
             _ => "😐"
        };
    }
}
namespace JournalApp.Models
{
    public static class MoodExtensions
    {
        public static string ToEmoji(this MoodType type)
        {
             return type switch
            {
                 MoodType.Positive => "😊",
                 MoodType.Neutral => "😐",
                 MoodType.Negative => "😔",
                 _ => "😐"
            };
        }
    }
}
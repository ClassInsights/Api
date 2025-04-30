using System.ComponentModel;

namespace Api.Models.Dto;

public class SettingsDto
{
    public class Dashboard
    {
        [Description("Check if there are gaps between lessons")]
        public bool CheckGap { get; set; } = true;

        [Description("Valid gap between lessons without shutdown")]
        public int LessonGapMinutes { get; set; } = 20;

        [Description("Time until a shutdown should be sent if no lessons left")]
        public int NoLessonsTime { get; set; } = 50;

        [Description("Shutdown if no user is logged in")]
        public bool CheckUser { get; set; } = true;

        [Description("Shutdown if user is not active")]
        public bool CheckAfk { get; set; } = true;

        [Description("Time until a user is considered afk")]
        public int AfkTimeout { get; set; } = 15;

        [Description("Delay shutdown")] public bool DelayShutdown { get; set; } = true;

        [Description("Delay in minutes before shutdown is sent")]
        public int ShutdownDelay { get; set; } = 3;
    }
}
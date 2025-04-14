namespace Api.Models.Dto;

public class SettingsDto
{
    public class Dashboard
    {
        // check if there are gaps between lessons 
        public bool CheckGap { get; set; } = true;
        
        // how long can a gap between lessons be
        public int LessonGapMinutes { get; set; } = 20;
        
        // how long until a shutdown should be sent if no lessons
        public int NoLessonsTime { get; set; } = 50;
        
        // check if any user is logged in
        public bool CheckUser { get; set; } = true;
        
        // check if user is active
        public bool CheckAfk { get; set; } = true;
        
        // how long can user be inactive until shutdown
        public int AfkTimeout { get; set; } = 15;
        
        // wait after lessons before shutdown
        public bool DelayShutdown { get; set; } = true;
        
        // how long should be waited before shutdown
        public int ShutdownDelay { get; set; } = 3;
    }
}
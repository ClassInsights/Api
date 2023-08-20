﻿namespace Api.Models;

public class TabClass
{
    public int ClassId { get; set; }

    public string Name { get; set; } = null!;

    public string Head { get; set; } = null!;

    public string? AzureGroupID { get; set; }

    public virtual TabGroup? GroupNavigation { get; set; }

    public virtual ICollection<TabLesson> TabLessons { get; set; } = new List<TabLesson>();

    public virtual ICollection<TabUser> TabUsers { get; set; } = new List<TabUser>();
}

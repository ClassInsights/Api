using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class TabClass
{
    public int ClassId { get; set; }

    public string Name { get; set; } = null!;

    public string Head { get; set; } = null!;

    public string? AzureGroupId { get; set; }

    public virtual TabAzureGroup? AzureGroup { get; set; }

    public virtual ICollection<TabLesson> TabLessons { get; set; } = new List<TabLesson>();

    public virtual ICollection<TabUser> TabUsers { get; set; } = new List<TabUser>();
}

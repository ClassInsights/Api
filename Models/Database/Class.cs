using System;
using System.Collections.Generic;

namespace Api.Models.Database;

public partial class Class
{
    public long ClassId { get; set; }

    public string DisplayName { get; set; } = null!;

    public string? AzureGroupId { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

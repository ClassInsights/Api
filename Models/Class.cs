using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class Class
{
    public int ClassId { get; set; }

    public string Name { get; set; } = null!;

    public string Head { get; set; } = null!;

    public string? AzureGroupId { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

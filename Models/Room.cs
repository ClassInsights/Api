using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string? Name { get; set; }

    public string? LongName { get; set; }

    public virtual ICollection<Computer> Computers { get; set; } = new List<Computer>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

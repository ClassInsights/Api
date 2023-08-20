using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class TabAzureGroup
{
    public string GroupId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<TabClass> TabClasses { get; set; } = new List<TabClass>();
}

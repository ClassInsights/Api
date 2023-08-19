namespace Api.Models;

public partial class TabGroup
{
    public string GroupId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<TabClass> TabClasses { get; set; } = new List<TabClass>();
}

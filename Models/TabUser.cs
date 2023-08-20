using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class TabUser
{
    public int UserId { get; set; }

    public string? AzureUserId { get; set; }

    public int? ClassId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public string? AccessToken { get; set; }

    public virtual TabClass? Class { get; set; }
}

using System;
using System.Collections.Generic;
using NodaTime;

namespace Api.Models.Database;

public partial class User
{
    public int UserId { get; set; }

    public string? AzureUserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public Instant? LastSeen { get; set; }
}

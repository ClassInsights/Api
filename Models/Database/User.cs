using System;
using System.Collections.Generic;
using NodaTime;

namespace Api.Models.Database;

public partial class User
{
    public long UserId { get; set; }

    public string? AzureUserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public Instant? LastSeen { get; set; }

}

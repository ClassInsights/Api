﻿using NodaTime;

namespace Api.Models.Database;

public class User
{
    public long UserId { get; set; }

    public string? AzureUserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public Instant? LastSeen { get; set; }
}
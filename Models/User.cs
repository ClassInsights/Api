﻿using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? AzureUserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public DateTime? LastSeen { get; set; }
}

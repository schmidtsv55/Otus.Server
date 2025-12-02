using System;

namespace Otus.NBomber.ConsoleApp.DTO;

public class UserProfile
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public DateTime CreatedAt { get; set; }
}

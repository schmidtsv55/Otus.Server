namespace Otus.Server.ConsoleApp;

public class UserProfile
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public DateTime CreatedAt { get; set; }
    public override bool Equals(object? obj)
    {
        if (obj is UserProfile userProfile)
        {
            return Equals(this, userProfile);
        }
        return false;
    }
    private bool Equals(UserProfile x, UserProfile y)
    {
        if (x.Username == null && y.Username != null)
        {
            return false;
        }
        if (x.Username != null && y.Username == null)
        {
            return false;
        }
        return
        x.Id == y.Id &&
        ((x.Username == null && y.Username == null) || x.Username!.Equals(y.Username)) &&
        x.CreatedAt.Equals(y.CreatedAt);
    }
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

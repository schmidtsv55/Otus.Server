using System.Text;
using NBomber.CSharp;
using Otus.NBomber.ConsoleApp;
using Otus.NBomber.ConsoleApp.DTO;

NBomber.Contracts.ScenarioProps scenario = Scenario.Create("Set Scenario ", async context =>
{
    using Client client = new();
    await client.ConnectAsync();
    UserProfile userProfile = new()
    {
        Id = Random.Shared.Next(),
        Username = Generate(4),
        CreatedAt = DateTime.Now
    };
    string sendResult = await client.SetAsync(Generate(4), userProfile);

    return sendResult.Trim() == "OK"
                    ? Response.Ok()
                    : Response.Fail();
})
.WithWarmUpDuration(TimeSpan.FromSeconds(10))
.WithLoadSimulations(
    Simulation.Inject(rate: 100,
                      interval: TimeSpan.FromSeconds(1),
                      during: TimeSpan.FromSeconds(30))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();

string Generate(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var random = new Random();
    var sb = new StringBuilder(length);
    for (int i = 0; i < length; i++)
    {
        sb.Append(chars[random.Next(chars.Length)]);
    }
    return sb.ToString();
}

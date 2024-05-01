using System.Net.Http.Headers;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

var apiUrl = app.Configuration.GetValue<string>("ClashAPIUrl") ?? string.Empty;
var apiToken = app.Configuration.GetValue<string>("ClashAPIToken") ?? string.Empty;

var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri(apiUrl);
if (!string.IsNullOrEmpty(apiToken))
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

app.MapGet("/", async (HttpContext context) =>
{
    if (!context.Request.Query.TryGetValue("tag", out var tag))
    {
        context.Response.Redirect("index.html");
    }
    else
    {
        var clan = await httpClient.GetFromJsonAsync<Clan>($"/v1/clans/%23{tag.FirstOrDefault()}");

        var ret = clan?.MemberList.Select(member =>
            new MemberDto(member.Tag, member.Name, member.Donations, member.DonationsReceived)
        );

        await context.Response.WriteAsJsonAsync(ret);
    }
});

app.UseStaticFiles();

app.Run();

public record Clan(string Tag, string Name, Member[] MemberList);

public record Member(string Tag, string Name, int Donations, int DonationsReceived);

public record MemberDto(string Tag, string Name, int Donated, int Received);

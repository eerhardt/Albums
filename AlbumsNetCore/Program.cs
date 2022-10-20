// Albums slice to seed record album data.
using System.Diagnostics.CodeAnalysis;

var myAlbums = new[]
{
    new Album("1", "Pawn Hearts", "Van der Graaf Generator", 26.99),
    new Album("2", "A Passion Play", "Jethro Tull", 17.99),
    new Album("3", "Tales from Topographic Oceans", "Yes", 32.99)
};

#if MINIMAL_STARTUP

var app = new WebHostBuilder()
    .UseKestrel(c => c.ListenLocalhost(5000))
    .UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .ConfigureServices(services =>
    {
        services.AddRouting();
    })
    .Configure(app =>
    {
        app.UseRouting();
        app.UseEndpoints(routes =>
        {
            routes.MapGet("/albums", GetAlbums);
            routes.MapGet("/albums/{id}", GetAlbumById);
            routes.MapPost("/albums", PostAlbums);
        });
    })
    .Build();

#else

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/albums", GetAlbums);
app.MapGet("/albums/{id}", GetAlbumById);
app.MapPost("/albums", PostAlbums);

#endif

app.Run();

// GetAlbums responds with the list of all albums as JSON.
[DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Album))] // allows JSON serialization to work
IResult GetAlbums() => Results.Ok(myAlbums);

// PostAlbums adds an album from JSON received in the request body.
IResult PostAlbums(Album album)
{
    myAlbums = myAlbums.Append(album).ToArray();
    return Results.Created($"/albums/{album.Id}", album);
}

// GetAlbumById locates the album whose ID value matches the id parameter sent by the client, then returns that album as a response.
async Task GetAlbumById(HttpContext context)
{
    if (context.Request.RouteValues.TryGetValue("id", out var idObj) && idObj is string id)
    {
        Album? album = myAlbums.FirstOrDefault(a => a.Id == id);
        if (album is not null) 
        {
            context.Response.StatusCode = 200;
            await context.Response.Body.WriteAsync(Data.ResponseBody);
            return;
        }
    }

    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("album not found");
}

// Album represents data about a record album.
internal record Album(string Id, string Title, string Artist, double Price);

static class Data
{
    public static byte[] ResponseBody = """{ "Id": "1", "Title": "Pawn Hearts", "Artist": "Van der Graaf Generator", "Price": 26.99 }"""u8.ToArray();
}

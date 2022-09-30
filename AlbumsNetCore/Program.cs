// Albums slice to seed record album data.
using System.Diagnostics.CodeAnalysis;

var myAlbums = new[]
{
    new Album("1", "Pawn Hearts", "Van der Graaf Generator", 26.99),
    new Album("2", "A Passion Play", "Jethro Tull", 17.99),
    new Album("3", "Tales from Topographic Oceans", "Yes", 32.99)
};

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/albums", GetAlbums);
app.MapGet("/albums/{id}", GetAlbumById);
app.MapPost("/albums", PostAlbums);
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
IResult GetAlbumById(string id) => myAlbums.FirstOrDefault(a => a.Id == id) is { } album ? Results.Ok(album) : Results.NotFound("album not found");

// Album represents data about a record album.
internal record Album(string Id, string Title, string Artist, double Price);

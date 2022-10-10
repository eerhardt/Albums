using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

var http = new HttpListener();

var host = "127.0.0.1";
var port = 5000;
var contentType = "application/json";
var concurrency = 10;

http.Prefixes.Add($"http://{host}:{port}/");
http.Start();

var myAlbums = new[]
{
    new Album("1", "Pawn Hearts", "Van der Graaf Generator", 26.99),
    new Album("2", "A Passion Play", "Jethro Tull", 17.99),
    new Album("3", "Tales from Topographic Oceans", "Yes", 32.99)
};

#pragma warning disable 4014 // no await
for(var i=0; i<concurrency; i++) http.GetContextAsync().ContinueWith(ProcessRequestHandler);
#pragma warning disable 4014

System.Console.ReadLine();

async void ProcessRequestHandler(Task<HttpListenerContext> result)
{
    var context = await result;

    if (!http.IsListening) return;
    http.GetContextAsync().ContinueWith(ProcessRequestHandler);

    var responseString = "";
    if (context.Request.RawUrl.StartsWith("/albums", StringComparison.OrdinalIgnoreCase))
    {
        // POST
        if (context.Request.HttpMethod.Equals("POST",  StringComparison.OrdinalIgnoreCase))
        {
            // read content
            Album album;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                album = await JsonSerializer.DeserializeAsync<Album>(context.Request.InputStream, AlbumJsonSerializerContext.Default.Album);
            }
            responseString = PostAlbums(album);
        }

        // GET
        else
        {
            var idindx = context.Request.RawUrl.LastIndexOf('/');
            if (idindx > 0 && idindx != context.Request.RawUrl.Length - 1) responseString = GetAlbumById(context.Request.RawUrl.Substring(idindx+1));
            else responseString = GetAlbums();
        }
    }
    else responseString = "not found";

    // write details
    var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
    context.Response.ContentLength64 = buffer.Length;
    context.Response.ContentType = contentType;
    using (var output = context.Response.OutputStream)
    {
        await output.WriteAsync(buffer, 0, buffer.Length);
    }
}

// GetAlbums responds with the list of all albums as JSON.
string GetAlbums() => JsonSerializer.Serialize(myAlbums, typeof(Album[]), AlbumJsonSerializerContext.Default);

// PostAlbums adds an album from JSON received in the request body.
string PostAlbums(Album album)
{
    myAlbums = myAlbums.Append(album).ToArray();
    return "success";
}

// GetAlbumById locates the album whose ID value matches the id parameter sent by the client, then returns that album as a response.
string GetAlbumById(string id) => myAlbums.FirstOrDefault(a => a.Id == id) is { } album ? 
                                           JsonSerializer.Serialize(album, AlbumJsonSerializerContext.Default.Album) : 
                                           "album not found";

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Album))]
[JsonSerializable(typeof(Album[]))]
partial class AlbumJsonSerializerContext : JsonSerializerContext
{
}

// Album represents data about a record album.
internal record Album(string Id, string Title, string Artist, double Price);

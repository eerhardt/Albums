using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

var http = new HttpListener();

var host = OperatingSystem.IsWindows() ? "127.0.0.1" : "*";
var port = 5000;
var concurrency = 10;
var url = $"http://{host}:{port}/";

http.Prefixes.Add(url);
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

Console.WriteLine($"Application started. Listening on {url}");
Console.WriteLine("Press CTRL+C to shut down.");
await WaitForShutdown();
Console.WriteLine("Application is shutting down...");

async void ProcessRequestHandler(Task<HttpListenerContext> result)
{
    var context = await result;

    if (!http.IsListening) return;
    http.GetContextAsync().ContinueWith(ProcessRequestHandler);

    Response response;
    if (context.Request.RawUrl?.StartsWith("/albums", StringComparison.OrdinalIgnoreCase) == true)
    {
        // POST
        if (context.Request.HttpMethod.Equals("POST",  StringComparison.OrdinalIgnoreCase))
        {
            var album = await JsonSerializer.DeserializeAsync(context.Request.InputStream, AlbumJsonSerializerContext.Default.Album);
            response = PostAlbums(album!);
        }

        // GET
        else
        {
            var idindx = context.Request.RawUrl.LastIndexOf('/');
            if (idindx > 0 && idindx != context.Request.RawUrl.Length - 1) response = GetAlbumById(context.Request.RawUrl.Substring(idindx+1));
            else response = GetAlbums();
        }
    }
    else response = new Response("not found", 404);

    // write details
    var buffer = System.Text.Encoding.UTF8.GetBytes(response.Content);
    context.Response.ContentLength64 = buffer.Length;
    context.Response.ContentType = response.Code == 200 ? "application/json" : "text/html";
    context.Response.StatusCode = response.Code;
    using (var output = context.Response.OutputStream)
    {
        await output.WriteAsync(buffer, 0, buffer.Length);
    }
}

// GetAlbums responds with the list of all albums as JSON.
Response GetAlbums() => new Response(JsonSerializer.Serialize(myAlbums, AlbumJsonSerializerContext.Default.AlbumArray), 200);

// PostAlbums adds an album from JSON received in the request body.
Response PostAlbums(Album album)
{
    myAlbums = myAlbums.Append(album).ToArray();
    return new Response(JsonSerializer.Serialize(album, AlbumJsonSerializerContext.Default.Album), 200);
}

// GetAlbumById locates the album whose ID value matches the id parameter sent by the client, then returns that album as a response.
Response GetAlbumById(string id) => myAlbums.FirstOrDefault(a => a.Id == id) is { } album ? 
                                           new Response(JsonSerializer.Serialize(album, AlbumJsonSerializerContext.Default.Album), 200) : 
                                           new Response("album not found", 404);

Task WaitForShutdown()
{
    TaskCompletionSource taskSource = new TaskCompletionSource();
    Action<PosixSignalContext> handler = ctx =>
    {
        ctx.Cancel = true;
        taskSource.SetResult();
    };

    PosixSignalRegistration.Create(PosixSignal.SIGINT, handler);
    PosixSignalRegistration.Create(PosixSignal.SIGQUIT, handler);
    PosixSignalRegistration.Create(PosixSignal.SIGTERM, handler);

    return taskSource.Task;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Album))]
[JsonSerializable(typeof(Album[]))]
partial class AlbumJsonSerializerContext : JsonSerializerContext
{
}

// Album represents data about a record album.
internal record Album(string Id, string Title, string Artist, double Price);

internal record Response(string Content, int Code);

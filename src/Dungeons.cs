using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

interface IArtifact
{
    string File { get; }

    string SHA1 { get; }

    string Url { get; }
}

file readonly struct Artifact(string file, string sha1, string url, int size) : IArtifact
{
    public string File => file;

    public string SHA1 => sha1;

    public string Url => url;

    public int Size => size;
}

file struct SynchronizationContextRemover : INotifyCompletion
{
    internal readonly bool IsCompleted => SynchronizationContext.Current == null;

    internal readonly SynchronizationContextRemover GetAwaiter() => this;

    internal readonly void GetResult() { }

    public readonly void OnCompleted(Action continuation)
    {
        var syncContext = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            continuation();
        }
        finally { SynchronizationContext.SetSynchronizationContext(syncContext); }
    }
}

static class Dungeons
{
    static readonly HttpClient httpClient = new();

    static readonly JavaScriptSerializer javaScriptSerializer = new();

    static async Task<dynamic> GetAsync(string requestUri)
    {
        using var response = await httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return javaScriptSerializer.Deserialize<dynamic>(await response.Content.ReadAsStringAsync());
    }

    internal static async Task<IList<IArtifact>> GetAsync()
    {
        await default(SynchronizationContextRemover);

        List<IArtifact> artifacts = [];
        foreach (var file in (await GetAsync(
            (await GetAsync("https://piston-meta.mojang.com/v1/products/dungeons/f4c685912beb55eb2d5c9e0713fe1195164bba27/windows-x64.json"))["dungeons"][0]["manifest"]["url"]))["files"])
        {
            if (file.Value["type"].Equals("directory")) continue;
            var raw = file.Value["downloads"]["raw"];
            artifacts.Add(new Artifact(Path.Combine("Content", file.Key), raw["sha1"], raw["url"], raw["size"]));
        }
        artifacts.Sort((a, b) => ((Artifact)a).Size.CompareTo(((Artifact)b).Size));

        return artifacts;
    }
}
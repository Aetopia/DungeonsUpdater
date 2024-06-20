using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

interface IArtifact
{
    string File { get; }

    string SHA1 { get; }

    string Url { get; }
}

file class Artifact(string file, string sha1, string url, int size) : IArtifact
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
        try { SynchronizationContext.SetSynchronizationContext(null); continuation(); }
        finally { SynchronizationContext.SetSynchronizationContext(syncContext); }
    }
}


static class Dungeons
{
    static readonly WebClient client = new();

    static async Task<XmlDocument> DeserializeAsync(string address)
    {
        using XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(await client.DownloadStringTaskAsync(address)), XmlDictionaryReaderQuotas.Max);
        XmlDocument xml = new();
        xml.Load(reader);
        return xml;
    }

    internal static async Task<ReadOnlyCollection<IArtifact>> GetAsync()
    {
        await default(SynchronizationContextRemover);
        List<IArtifact> artifacts = [];

        foreach (XmlNode raw in (await DeserializeAsync((await DeserializeAsync("https://piston-meta.mojang.com/v1/products/dungeons/f4c685912beb55eb2d5c9e0713fe1195164bba27/windows-x64.json"))
        .GetElementsByTagName("url")[0].InnerText))
        .GetElementsByTagName("raw"))
        {
            var item = (XmlElement)raw.ParentNode.ParentNode;
            var file = item.GetAttribute("item");

            artifacts.Add(new Artifact(
                Path.Combine("Content", string.IsNullOrEmpty(file) ? item.Name : file),
                raw["sha1"].InnerText,
                raw["url"].InnerText,
                int.Parse(raw["size"].InnerText)));
        }

        artifacts.Sort((x, y) => ((Artifact)x).Size.CompareTo(((Artifact)y).Size));
        return artifacts.AsReadOnly();
    }
}
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Json;

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

static class Dungeons
{
    static readonly WebClient client = new();

    static XmlDocument Deserialize(string address)
    {
        using XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(client.DownloadString(address)), XmlDictionaryReaderQuotas.Max);
        XmlDocument xml = new();
        xml.Load(reader);
        return xml;
    }

    internal static ReadOnlyCollection<IArtifact> GetAsync()
    {
        List<IArtifact> artifacts = [];

        foreach (XmlNode raw in Deserialize(Deserialize("https://piston-meta.mojang.com/v1/products/dungeons/f4c685912beb55eb2d5c9e0713fe1195164bba27/windows-x64.json")
        .GetElementsByTagName("url")[0].InnerText)
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
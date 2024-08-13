using System.Net;
using System.Xml;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

class Artifact { internal string Path; internal string File; internal string SHA1; internal string Url; internal int Size; }

static class Dungeons
{
    static readonly WebClient client = new();

    static async Task<XmlElement> DeserializeAsync(string address)
    {
        using XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(await client.DownloadDataTaskAsync(address), XmlDictionaryReaderQuotas.Max);
        XmlDocument document = new();
        document.Load(reader);
        return document["root"];
    }


    internal static async Task<List<Artifact>> GetAsync()
    {
        List<Artifact> list = [];
        string path = default;

        foreach (XmlNode node in (await DeserializeAsync(
            (await DeserializeAsync("https://piston-meta.mojang.com/v1/products/dungeons/f4c685912beb55eb2d5c9e0713fe1195164bba27/windows-x64.json")).
            GetElementsByTagName("url")[0].InnerText))["files"])
        {
            if (node["type"].InnerText == "directory") { path = node.Attributes["item"].InnerText; continue; }
            var item = node["downloads"]["raw"];
            var value = node.Attributes["item"]?.InnerText;

            list.Add(new()
            {
                Path = path,
                File = string.IsNullOrEmpty(value) ? node.Name : value,
                SHA1 = item["sha1"].InnerText,
                Url = item["url"].InnerText,
                Size = int.Parse(item["size"].InnerText)
            });
        }

        list.Sort((x, y) => x.Size.CompareTo(y.Size));
        return list;
    }
}
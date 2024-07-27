using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Json;
using System;

struct Artifact
{
    internal string File;

    internal string SHA1;

    internal string Url;

    internal int Size;
}

static class Dungeons
{
    static readonly WebClient client = new();

    static XmlDocument Deserialize(string address)
    {
        using XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(client.DownloadData(address), XmlDictionaryReaderQuotas.Max);
        XmlDocument document = new();
        document.Load(reader);
        return document;
    }

    internal static Artifact[] Get()
    {
        var nodes = Deserialize(
            Deserialize("https://piston-meta.mojang.com/v1/products/dungeons/f4c685912beb55eb2d5c9e0713fe1195164bba27/windows-x64.json")
            .GetElementsByTagName("url")[0].InnerText)
        .GetElementsByTagName("raw");
        Artifact[] array = new Artifact[nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var item = (XmlElement)node.ParentNode.ParentNode;
            var value = item.GetAttribute("item");

            array[i] = new()
            {
                File = Path.Combine("Content", string.IsNullOrEmpty(value) ? item.Name : value),
                SHA1 = node["sha1"].InnerText,
                Url = node["url"].InnerText,
                Size = int.Parse(node["size"].InnerText)
            };
        }
        
        Array.Sort(array, (x, y) => x.Size.CompareTo(y.Size));
        return array;
    }
}
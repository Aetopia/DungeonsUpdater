using System.IO;
using System.Xml;
using System.Linq;
using static Program;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

sealed class Item
{
    internal string Url;
    internal string Path;
    internal string Hash;
    internal long Size;
}

static class Endpoint
{
    const string Address = "https://piston-meta.mojang.com/v1/products/dungeons/f4c685912beb55eb2d5c9e0713fe1195164bba27/windows-x64.json";

    internal static Request Get()
    {
        List<Item> items = [];

        foreach (var _ in Get(Get(Address).Descendants("url").First().Value).Element("files").Elements())
        {
            var path = _._();
            if (_.Element("type").Value is "directory")
            {
                if (!string.IsNullOrWhiteSpace(path))
                    Directory.CreateDirectory(path);
            }
            else
            {
                var @object = _.Descendants("raw").First();

                items.Add(new()
                {
                    Size = long.Parse(@object.Element("size").Value),
                    Hash = @object.Element("sha1").Value,
                    Url = @object.Element("url").Value,
                    Path = path,
                });
            }
        }

        items.Sort((x, y) => x.Size.CompareTo(y.Size));
        return new(items);
    }

    static XElement Get(string _)
    {
        using var stream = Client.GetStreamAsync(_).GetAwaiter().GetResult();
        return Parse(stream);
    }

    static XElement Parse(Stream _)
    {
        using var reader = JsonReaderWriterFactory.CreateJsonReader(_, XmlDictionaryReaderQuotas.Max);
        return XElement.Load(reader);
    }

    static string _(this XElement _) => string.IsNullOrEmpty(_.Name.NamespaceName) ? _.Name.LocalName : _.Attribute("item").Value;
}
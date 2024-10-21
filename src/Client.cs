using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

struct Job
{
    public string Path;
    public string Hash;
    public string Url;
    public long Size;
}

static class Client
{
    static readonly HttpClient client = new() {Timeout = Timeout.InfiniteTimeSpan};

    static readonly SHA1 algorithm = SHA1.Create();

    static readonly int count = Environment.SystemPageSize;

    static XElement Parse(string address)
    {
        using var stream = client.GetStreamAsync(address).Result;
        using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);
        return XElement.Load(reader);
    }

    internal static IList<Job> Get()
    {
        List<Job> jobs = [];

        foreach (var element in Parse(Parse("https://piston-meta.mojang.com/v1/products/dungeons/f4c685912beb55eb2d5c9e0713fe1195164bba27/windows-x64.json")
        .Descendants("url").First().Value).Element("files").Elements())
        {
            if (element.Element("type").Value[0] == 'd')
            {
                var path = element.Name(); if (!string.IsNullOrEmpty(path)) Directory.CreateDirectory(path);
                continue;
            }

            var raw = element.Descendants("raw").First();
            jobs.Add(new()
            {
                Path = element.Name(),
                Hash = raw.Element("sha1").Value,
                Size = long.Parse(raw.Element("size").Value),
                Url = raw.Element("url").Value
            });
        }

        jobs.Sort((x, y) => x.Size.CompareTo(y.Size)); return jobs;
    }

    internal static void Download(this IList<Job> source, Action<double, double, double> action)
    {
        double current = 0, total = source.Select(_ => _.Size).Sum();
        Parallel.ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (_) =>
        {
            using var stream = client.GetStreamAsync(_.Url).Result; using var destination = File.OpenWrite(_.Path);
            var count = 0; var buffer = new byte[Client.count];
            while ((count = stream.Read(buffer, 0, Client.count)) != 0)
            {
                destination.Write(buffer, 0, count);
                lock (action)
                {
                    var progress = Math.Round((current += count) * 100 / total);
                    action(progress, current, total);
                }
            }
        });
    }

    internal static IList<Job> Verify(this IList<Job> source, Action<int, int> action)
    {
        List<Job> jobs = [];

        for (int index = 0; index < source.Count; index++)
        {
            var job = source[index]; action(index + 1, source.Count);
            if (File.Exists(job.Path))
            {
                using var stream = File.OpenRead(job.Path);
                if (job.Hash.Equals(BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", string.Empty), StringComparison.OrdinalIgnoreCase)) continue;
            }
            jobs.Add(job);
        }

        return jobs;
    }

    static string Name(this XElement source) => source.Name.NamespaceName.Length == 0 ? source.Name.LocalName : source.Attribute("item").Value;
}

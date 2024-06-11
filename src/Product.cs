using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

interface IFile
{
    string Path { get; }

    string Url { get; }
}

file readonly struct File(string path, string url, int size) : IFile
{
    public string Path => path;

    public string Url => url;

    internal int Size => size;
}

static class Product
{
    static readonly string ProductPath = string.Empty;

    static readonly HttpClient httpClient = new();

    static readonly JavaScriptSerializer javaScriptSerializer = new();

    static readonly SHA1 sha1 = SHA1.Create();

    static async Task<dynamic> GetAsync(string requestUri)
    {
        using var response = await httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return javaScriptSerializer.Deserialize<dynamic>(await response.Content.ReadAsStringAsync());
    }

    internal static async Task<IEnumerable<IFile>> GetFilesAsync()
    {
        return await Task.Run(async () =>
        {
            List<IFile> files = [];

            foreach (var file in (await GetAsync(
                (await GetAsync("https://piston-meta.mojang.com/v1/products/dungeons/f4c685912beb55eb2d5c9e0713fe1195164bba27/windows-x64.json"))["dungeons"][0]["manifest"]["url"]))["files"])
            {
                if (file.Value["type"].Equals("directory"))
                    try { Directory.CreateDirectory(file.Key); }
                    catch (ArgumentException) { }
                else
                {
                    var raw = file.Value["downloads"]["raw"];
                    if (System.IO.File.Exists(file.Key))
                    {
                        using FileStream inputStream = System.IO.File.OpenRead(file.Key);
                        if (raw["sha1"].Equals(BitConverter.ToString(sha1.ComputeHash(inputStream)).Replace("-", string.Empty), StringComparison.OrdinalIgnoreCase))
                            continue;
                    }
                    files.Add(new File(file.Key, raw["url"], raw["size"]));
                }
            }

            return files.OrderBy(entry => ((File)entry).Size);
        });
    }
}
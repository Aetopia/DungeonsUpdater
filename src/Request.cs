using System;
using System.IO;
using System.Linq;
using static Program;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;

sealed class Request
{
    readonly int Size = Environment.SystemPageSize;

    List<Item> Items;

    internal Request(List<Item> _) => Items = _;

    static readonly ThreadLocal<HashAlgorithm> Algorithm = new(SHA1.Create);

    internal void Verify()
    {
        List<Item> items = [];
        Parallel.ForEach(Items, _ =>
        {
            try
            {
                using var stream = File.OpenRead(_.Path);
                if (_.Hash.Equals(BitConverter.ToString(Algorithm.Value.ComputeHash(stream)).Replace("-", string.Empty), StringComparison.OrdinalIgnoreCase))
                    return;
            }
            catch (FileNotFoundException) { }
            lock (items) items.Add(_);
        });
        Items = items;
    }

    internal void Download(Action<(int Percentage, long Current, long Total)> action)
    {
        (int Percentage, long Current, long Total) progress = new() { Total = Items.Select(_ => _.Size).Sum() };

        Parallel.ForEach(Items, _ =>
        {
            using var stream = Client.GetStreamAsync(_.Url).GetAwaiter().GetResult();
            using var destination = File.OpenWrite(_.Path);

            var count = 0;
            var buffer = new byte[Size];

            while ((count = stream.Read(buffer, 0, buffer.Length)) is not 0)
            {
                destination.Write(buffer, 0, count);
                lock (action)
                {
                    progress.Current += count;
                    progress.Percentage = (int)(100 * progress.Current / progress.Total);
                    action(progress);
                }
            }
        });
    }
}
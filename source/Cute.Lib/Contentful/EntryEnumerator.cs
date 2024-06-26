﻿using Contentful.Core.Models;
using Contentful.Core.Search;
using Newtonsoft.Json.Linq;
using Contentful.Core;

namespace Cute.Lib.Contentful;

public static class EntryEnumerator
{
    public static async IAsyncEnumerable<(Entry<JObject>, ContentfulCollection<Entry<JObject>>)> Entries(ContentfulManagementClient client, string contentType, string orderByField)
    {
        var skip = 0;
        var page = 100;

        while (true)
        {
            var query = new QueryBuilder<Entry<JObject>>()
                .ContentTypeIs(contentType)
                .Include(2)
                .Skip(skip)
                .Limit(page)
                .OrderBy($"fields.{orderByField}")
                .Build();

            var entries = await client.GetEntriesCollection<Entry<JObject>>(query);

            if (!entries.Any()) break;

            foreach (var entry in entries)
            {
                yield return (entry, entries);
            }

            skip += page;
        }
    }
}
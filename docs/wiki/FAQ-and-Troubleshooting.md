# FAQ and Troubleshooting

Quick answers to the questions that come up most. Start with the symptom you are seeing.

## Seeing what is sent

**What URL did my query produce?**

```csharp
string url = await client.For<Product>().Filter(p => p.UnitPrice > 10).Top(3).GetCommandTextAsync();
```

To watch full requests and responses turn on tracing:

```csharp
settings.OnTrace = (format, args) => Console.WriteLine(format, args);
settings.TraceFilter = ODataTrace.RequestContent | ODataTrace.ResponseContent;
```

## Errors

| Symptom | Likely cause | Fix |
|---|---|---|
| `UnresolvableObjectException: Entity collection 'X' not found` | The set name differs from your class name, or the name matcher is too strict | Pass the name: `For<Product>("Items")`, register it with `settings.TypeCache.Register<Product>("Items")`, or relax `NameMatchResolver` |
| `UnresolvableObjectException` about a property | Typo, or the property is not in `$metadata` | Check the metadata; for open types use `WithProperties` |
| `WebRequestException` with status 401 or 403 | Missing credentials | Add a header in `settings.BeforeRequest`, or pass an `HttpClient` that handles authentication |
| `WebRequestException` 404 on `Find...` | Wrong service root or key | The service root must end with `/`. Set `IgnoreResourceNotFoundException = true` to get `null` instead |
| `InvalidOperationException: No entry key specified.` | Update, delete or link without `Key(...)` or a filter | Add `.Key(id)` or a `.Filter(...)` |
| `$metadata` download fails | Service needs authentication, or is unreachable | Fix auth in `BeforeRequest`, or supply `MetadataDocument` / `MetadataModel` |
| Properties missing from results | Mapping by name failed | Check spelling and case; use `NameMatchResolver = ODataNameMatchResolver.AlphanumericCaseInsensitive` |
| Reads fail on undeclared properties | Strict validation | The default accepts them; if you set `Validations = ValidationKinds.All`, relax it again |

## Behaviour

**Do I need to generate classes?** No. Use dictionaries or `dynamic`, or write your own minimal classes with only the properties you need.

**Is metadata fetched on every call?** No. It is downloaded once per service root and cached for the life of the process (`ODataClient.ClearMetadataCache()` resets it).

**How do I authenticate?**

```csharp
var settings = new ODataClientSettings(new Uri("https://example.com/odata/"))
{
    BeforeRequest = request => request.Headers.Authorization = new("Bearer", token),
};
```

For token refresh use `BeforeRequestAsync`, or give the client an `HttpClient` built on your own `DelegatingHandler`.

**How do I get every row from a paged service?** `await foreach (var p in client.For<Product>().FindEntriesAsAsyncEnumerable())` follows every page. See [Paging, Counts and Change Tracking](Paging,-Counts-and-Change-Tracking).

**How do I send many changes at once?** Use `ODataBatch`; see [Batches](Batches).

**My service only speaks OData 3 (or older).** Not supported. This library targets OData V4 and V4.01.

**Does it support Native AOT or trimming?** Not at the moment; the client relies on reflection and expression trees.

**How do I test code that uses the client?** Point `settings.RequestExecutor` at a function returning canned responses, supply `MetadataDocument`, and replace `TimeProvider`. See [Testing and Benchmarks](Testing-and-Benchmarks).

## Still stuck?

Open an issue at https://github.com/KuwaTechnologies/Kuwa.OData.Client/issues with the service (or a metadata snippet), your code, the URL from `GetCommandTextAsync()`, and what you expected.

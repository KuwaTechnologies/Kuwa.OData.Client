# Kuwa.OData.Client

> A friendly, fast OData client for .NET 10. Point it at any OData V4 / V4.01 service and start querying, with no code generation.

<p align="center">
<a href="https://www.nuget.org/packages/Kuwa.OData.Client/"><img src="https://img.shields.io/nuget/v/Kuwa.OData.Client.svg" alt="NuGet version"></a>
<a href="https://github.com/KuwaTechnologies/Kuwa.OData.Client/releases"><img src="https://img.shields.io/github/v/release/KuwaTechnologies/Kuwa.OData.Client" alt="GitHub release"></a>
<a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-green.svg" alt="MIT license"></a>
<img src="https://img.shields.io/badge/.NET-10-512bd4.svg" alt=".NET 10">
</p>

Kuwa.OData.Client reads the service's metadata once, then lets you work with the data the way you prefer:
**plain dictionaries**, **your own classes**, or **dynamic expressions**. It supports the complete
**OData 4.01** feature set on top of Microsoft.OData.Core 9, with a modern async API (`CancellationToken`,
`IAsyncEnumerable`, nullable annotations).

## Contents

- [Why Kuwa.OData.Client](#why-kuwaodataclient)
- [Install](#install)
- [A 60-second tour](#a-60-second-tour)
- [Where to go next](#where-to-go-next)
- [Quick start](#quick-start)
- [Feature overview](#feature-overview)
- [Settings](#settings)
- [Testing](#testing)
- [Migrating from Simple.OData.Client 6.x](#migrating-from-simpleodataclient-6x)
- [Contributing](#contributing)
- [Origin and license](#origin)

## Why Kuwa.OData.Client

| You want to… | Kuwa.OData.Client gives you… |
|---|---|
| Skip generated proxies | Metadata-driven requests: no `Connected Service`, no codegen step |
| Choose your style | Typed LINQ-style filters, `IDictionary<string, object?>` results, or `dynamic` |
| Use the newest OData | `$compute`, `$apply`, `$each`, delta, JSON batch, `respond-async`, paging, key-as-segment |
| Keep control of HTTP | Bring your own `HttpClient`, hooks before and after every request, re-runnable built requests |
| Test with confidence | 1000+ tests, including 500+ end-to-end scenarios against an in-process OData service |

## Install

You need the **.NET 10 SDK** and an OData V4 or V4.01 service.

**From NuGet:**

```bash
dotnet add package Kuwa.OData.Client
```

**From GitHub Packages** (alternative feed; needs a GitHub token with `read:packages`):

```bash
dotnet nuget add source https://nuget.pkg.github.com/kuwatechnologies/index.json \
  --name github-kuwatechnologies --username <your-github-user> --password <token-with-read:packages> \
  --store-password-in-clear-text
dotnet add package Kuwa.OData.Client
```

The NuGet package id, assembly and root namespace are all `Kuwa.OData.Client`.

## A 60-second tour

```csharp
using Kuwa.OData.Client;

var client = new ODataClient(new Uri("https://services.odata.org/V4/Northwind/Northwind.svc/"));

// 1. Read as dictionaries: no classes needed
var chai = await client.For("Products").Filter("ProductName eq 'Chai'").FindEntryAsync();

// 2. Read as your own classes, with LINQ-style filters
var pricey = await client.For<Product>()
    .Filter(p => p.UnitPrice > 50)
    .OrderByDescending(p => p.UnitPrice)
    .Top(5)
    .FindEntriesAsync();

// 3. Write
var tea = await client.For<Product>().Set(new { ProductName = "Tea", UnitPrice = 4.5m }).InsertEntryAsync();
await client.For<Product>().Key(tea!.ProductID).Set(new { UnitPrice = 5m }).UpdateEntryAsync();
await client.For<Product>().Key(tea.ProductID).DeleteEntryAsync();
```

## Where to go next

The [wiki](https://github.com/KuwaTechnologies/Kuwa.OData.Client/wiki) is the full guide. Pick the page that matches what you are doing:

| I want to… | Read |
|---|---|
| Install and run my first query | [Getting Started](docs/wiki/Getting-Started.md) |
| Choose typed, dynamic or dictionaries | [Typed, Dynamic and Untyped APIs](docs/wiki/Typed,-Dynamic-and-Untyped-APIs.md) |
| Filter, sort, expand and page | [Querying Data](docs/wiki/Querying-Data.md) |
| Create, change and delete data | [Modifying Data](docs/wiki/Modifying-Data.md) |
| Call functions and actions | [Functions and Actions](docs/wiki/Functions-and-Actions.md) |
| Group, aggregate and compute | [Aggregation and Computed Properties](docs/wiki/Aggregation-and-Computed-Properties.md) |
| Stream large sets and track changes | [Paging, Counts and Change Tracking](docs/wiki/Paging,-Counts-and-Change-Tracking.md) |
| Send many operations at once | [Batches](docs/wiki/Batches.md) |
| Work with files and streams | [Media and Streams](docs/wiki/Media-and-Streams.md) |
| Tune headers, auth, timeouts, tracing | [Configuration](docs/wiki/Configuration.md) and [Advanced Requests](docs/wiki/Advanced-Requests.md) |
| Understand an error | [Error Handling](docs/wiki/Error-Handling.md) and [FAQ and Troubleshooting](docs/wiki/FAQ-and-Troubleshooting.md) |
| Move from Simple.OData.Client 6.x | [Migrating from 6.x](docs/wiki/Migrating-from-6.x.md) |

## Quick start

```csharp
using Kuwa.OData.Client;

var client = new ODataClient(new Uri("https://services.odata.org/V4/Northwind/Northwind.svc/"));

// Untyped: dictionaries in, dictionaries out
var chai = await client.For("Products").Filter("ProductName eq 'Chai'").FindEntryAsync();
Console.WriteLine(chai!["UnitPrice"]);

// Typed: your own classes, mapped by name
public class Product
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = "";
    public decimal? UnitPrice { get; set; }
    public Category? Category { get; set; }
}

var expensive = await client.For<Product>()
    .Filter(x => x.UnitPrice > 50 && x.ProductName.StartsWith("C"))
    .Expand(x => x.Category)
    .Select(x => new { x.ProductID, x.ProductName, x.UnitPrice, x.Category })
    .OrderByDescending(x => x.UnitPrice)
    .Top(5)
    .FindEntriesAsync();

// Dynamic: no classes at all
var x = ODataDynamic.Expression;
IEnumerable<dynamic> beverages = await client.For(x.Products).Filter(x.CategoryID == 1).FindEntriesAsync();
```

### Modifying data

```csharp
var created = await client.For<Product>().Set(new { ProductName = "Tea", UnitPrice = 4.5m }).InsertEntryAsync();
await client.For<Product>().Key(created!.ProductID).Set(new { UnitPrice = 5m }).UpdateEntryAsync();
await client.For<Product>().Key(created.ProductID).WithIfMatch(etag).Set(new { UnitPrice = 6m }).UpdateEntryAsync();
await client.For<Product>().Key(created.ProductID).LinkEntryAsync(x => x.Category, new Category { CategoryID = 1 });
await client.For<Product>().Key(created.ProductID).DeleteEntryAsync();

// Filter-based bulk operations, sent as a single PATCH/DELETE on Products/$each when BulkOperationMode.Each is set
await client.For<Product>().Filter(x => x.Discontinued).Set(new { UnitsOnOrder = 0 }).UpdateEntriesAsync();
```

### Paging, counts and change tracking

```csharp
var annotations = new ODataFeedAnnotations();
var firstPage = await client.For<Product>().WithMaxPageSize(50).WithCount().FindEntriesAsync(annotations);
Console.WriteLine($"{firstPage.Count} of {annotations.Count}; next: {annotations.NextPageLink}");

await foreach (var product in client.For<Product>().WithMaxPageSize(50).FindEntriesAsAsyncEnumerable())
{
    // every page is followed for you
}

await client.For<Product>().TrackChanges().FindEntriesAsync(annotations);
var delta = await client.FindDeltaAsync(annotations.DeltaLink!);   // Changed, Deleted, AddedLinks, DeletedLinks
```

### Functions, actions, aggregation and computed properties

```csharp
var best = await client.For<Product>().Function("MostExpensive").ExecuteAsSingleAsync();
var price = await client.For<Product>().Key(1).Function("Discounted").Set(new { percent = 10m }).ExecuteAsScalarAsync<decimal>();
await client.For<Product>().Key(1).Action("Rate").Set(new { rating = 5 }).ExecuteAsync();
var echoed = await client.Unbound().Action("Echo").Set(new { value = 42 }).ExecuteAsScalarAsync<int>();

var perCategory = await client.For<Product>()
    .Apply(b => b.Filter(p => p.UnitPrice > 20).GroupBy((p, a) => new CategoryCount { CategoryID = p.CategoryID, Count = a.Count() }))
    .FindEntriesAsync();

var doubled = await client.For("Products").Key(1).Compute("UnitPrice mul 2 as Doubled").Select("ProductName", "Doubled").FindEntryAsync();
```

### Batches

```csharp
var batch = new ODataBatch(client);               // or new ODataBatch(settings) with BatchFormat.Json
Product? tea = null;
batch += async c => tea = await c.For<Product>().Set(new { ProductName = "Tea" }).InsertEntryAsync();
batch += c => c.For<Product>().Set(new { ProductName = "Milk", Category = tea }).InsertEntryAsync(false); // $1 reference
batch += c => c.For<Product>().Key(1).Set(new { UnitPrice = 19m }).UpdateEntryAsync(false);
await batch.ExecuteAsync();
```

### Media and streams

```csharp
using var image = await client.For("Pictures").Key(1).Media().GetStreamAsync();
await client.For("Pictures").Key(1).Media("Content").SetStreamAsync("plain text");
var picture = await client.For<Picture>().Key(1).WithMedia(x => x.Media).FindEntryAsync(); // byte[] filled from $value
```

### Two-phase requests and custom transports

```csharp
var request = await client.For<Product>().Filter(x => x.UnitPrice > 10).BuildRequestFor().FindEntriesAsync();
HttpRequestMessage message = request.GetRequest().RequestMessage;   // inspect or send it yourself
using var response = await request.RunAsync();                        // or request.FromResponse(httpResponseMessage)
var products = await response.ReadAsCollectionAsync();
```

## Feature overview

| Area | Supported |
|---|---|
| Querying | `$filter` with every 4.01 built-in (string, date/time, arithmetic, `in`, `has`, `any`/`all`, `isof`/`cast`, `matchesPattern`, `hassubset`/`hassubsequence` and geo functions through `ODataFunctions`), `$select`, nested `$expand` options, `$orderby`, `$top`, `$skip`, `$count`, `/$count`, `$search`, `$index`, `$compute`, `$apply`, `$schemaversion` |
| Keys | single, compound, named, alternate keys, key-as-segment, filter-as-key |
| Model | singletons, containment, derived types, open types, enums (flags), complex types and collections, media entities, stream properties |
| Writing | POST with deep insert and `@odata.bind`, PATCH/PUT, DELETE, `$ref` links, `Prefer: return=representation|minimal`, `omit-values=nulls`, ETags (`If-Match`/`If-None-Match`, `Core.OptimisticConcurrency`), `$each` bulk operations |
| Operations | bound and unbound functions and actions, imports, complex/collection/entity parameters, parameter aliases |
| Batch | multipart and JSON batch, changesets and atomicity groups, `Content-ID` references, partial failures, `continue-on-error`, media in batches |
| Protocol | server-driven paging, delta links and delta payloads, `respond-async` with status monitors, instance annotations, `odata.metadata=minimal|full|none`, `IEEE754Compatible`, OData error payloads |
| Metadata | CSDL XML and CSDL JSON, the built-in vocabularies (Core, Capabilities, Community alternate keys) resolved through `edmx:Reference`, a process-wide model cache; supply `MetadataModel` yourself for services that reference external schemas |

## Settings

Create a client from a `Uri`, from an `HttpClient` (its `BaseAddress` becomes the service root) or from an
`ODataClientSettings` instance. The most useful settings:

| Setting | Default | Effect |
|---|---|---|
| `HttpClient`, `OnCreateMessageHandler`, `ConfigureHandler`, `RequestExecutor` | | Bring your own transport, customize the default `SocketsHttpHandler`, or replace HTTP entirely (tests) |
| `BeforeRequest(Async)`, `AfterResponse(Async)` | | Hooks around every request (authentication headers, logging) |
| `MetadataDocument`, `MetadataModel`, `MetadataFormat` | download, XML | Supply metadata yourself or request CSDL JSON |
| `ProtocolVersion` | `V4` | `V401` sends `OData-Version: 4.01` and enables 4.01 payload shortcuts |
| `MetadataLevel` | `Minimal` | `Full` exposes `@odata.type`/`@odata.id` through entry annotations |
| `KeyAsSegment`, `UnqualifiedNameCall`, `EnumPrefixFree` | `false` | URL conventions |
| `BatchFormat`, `ContinueOnError` | `Multipart`, `false` | JSON batch; keep processing after a failed operation |
| `BulkOperationMode` | `ClientSide` | `Each` sends one `PATCH`/`DELETE` to `Set/$each?$filter=` instead of iterating |
| `PreferredUpdateMethod` | `Patch` | `Put` when the whole entity is sent |
| `IgnoreResourceNotFoundException` | `false` | 404 becomes an empty result (outside batches) |
| `IgnoreUnmappedProperties`, `IgnoreNavigationPropertiesOnUpdate` | `false` | Tolerate POCO properties missing from the model; never unlink on update |
| `IncludeAnnotationsInResults`, `IncludeAnnotations` | `false`, none | Add an `__annotations` entry (`ODataEntryAnnotations`) to results; request `odata.include-annotations` |
| `MaxPageSize`, `TrackChanges`, `OmitNullValues`, `RespondAsync`, `AsyncPollingInterval` | | Preferences sent with every request |
| `Ieee754Compatible` | `false` | Exchange 64-bit integers and decimals as strings |
| `Validations` | duplicates + type conflicts | Undeclared properties (`$compute`, `$apply`, server extensions) are accepted |
| `WebRequestExceptionMessageSource` | `ReasonPhrase` | Use the response body as the exception message instead |
| `NameMatchResolver`, `TypeCache` | alphanumeric, shared | Loose name matching; property mapping and conversion |
| `OnTrace`, `TraceFilter` | | Request/response tracing |

`ODataClient.ClearMetadataCache()` forgets every downloaded model; downloaded models are cached per service root for the lifetime of the process.

## Testing

```bash
dotnet test src/Kuwa.OData.Client.slnx                 # unit tests + end-to-end tests against the in-process server
ODATA_LIVE_TESTS=1 dotnet test src/Kuwa.OData.Client.Tests.Integration   # public services.odata.org endpoints
dotnet run -c Release --project src/Kuwa.OData.Client.Benchmarks -- --filter '*'
```

The in-process server (`src/Kuwa.OData.Client.Tests.TestServer`) is a Northwind-shaped OData 4.01 service built on
Microsoft.AspNetCore.OData and hosted on `TestServer`; no sockets are opened. Benchmark results are kept in
[docs/benchmarks.md](docs/benchmarks.md).

## Migrating from Simple.OData.Client 6.x

- One package (`Kuwa.OData.Client`), `net10.0` only, OData V4/V4.01 only. `Kuwa.OData.Client.Core` and the
  `V3`/`V4` adapter packages no longer exist; V1–V3 services are not supported.
- Removed: `ODataClientSettings.PayloadFormat`, `RenewHttpConnection`, `AdapterFactory`, `UseAbsoluteReferenceUris`;
  `WithExtensions()` (call `Apply` directly); `ODataNameMatchResolver.Alpahumeric` is now `ODataNameMatchResolver.Alphanumeric`.
- Every operation takes an optional `CancellationToken` as its last parameter instead of duplicated overloads.
  Collection results are `IReadOnlyList<T>`; `GetMetadataAsync()` returns `IEdmModel`;
  `Unbound()` returns `IUnboundClient<object>` (use `Unbound<T>()` or `Unbound<IDictionary<string, object?>>()` for shaped results).
- URLs keep apostrophes literal (`Products(ProductName='Chai')`), collection members are unlinked with a key
  segment (`Categories(1)/Products(2)/$ref`), and complex function arguments travel as JSON parameter aliases.
- Inside a batch a `404` fails the batch unless `ContinueOnError` is set; `IgnoreResourceNotFoundException` applies to
  direct requests only.
- Undeclared properties no longer fail a read by default (`Validations`); set `ValidationKinds.All` to restore the old behaviour.
- New: `FindEntriesAsAsyncEnumerable`, `Compute`, `Index`, `WithIfMatch`/`WithIfNoneMatch`, `WithMaxPageSize`,
  `TrackChanges`, `FindDeltaAsync`, JSON batch, `$each`, `respond-async`, CSDL JSON metadata, `WebRequestException.Error`,
  `BuildRequestFor().…RunAsync()`.

The 6.x documentation on the [wiki](https://github.com/object/Simple.OData.Client/wiki) still describes the fluent
API accurately for querying and modifying data; the differences above are the ones to watch for.

## License

MIT. See [LICENSE](LICENSE).

## Contributing

Bug reports, ideas and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for how to build, test and propose a change.

## Origin

Kuwa.OData.Client is a continuation of [Simple.OData.Client](https://github.com/simple-odata-client/Simple.OData.Client) by Vagif Abilov, David Bond, Robert McLaws and contributors, rewritten for .NET 10 and OData V4/V4.01. The original MIT license and copyright notice are kept in `LICENSE`.

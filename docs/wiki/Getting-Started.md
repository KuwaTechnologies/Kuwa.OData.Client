# Getting Started

## 1. Install the package

You need the **.NET 10 SDK**.

```bash
dotnet add package Kuwa.OData.Client
```

The package id, assembly and root namespace are all `Kuwa.OData.Client`.

**Using GitHub Packages instead?** Add the feed once (it needs a GitHub token with `read:packages`), then install as above:

```bash
dotnet nuget add source https://nuget.pkg.github.com/kuwatechnologies/index.json \
  --name github-kuwatechnologies \
  --username <your-github-user> \
  --password <token-with-read:packages> \
  --store-password-in-clear-text
```

You can also download the `.nupkg` from the [latest release](https://github.com/KuwaTechnologies/Kuwa.OData.Client/releases) and add its folder as a local source.

## 2. Check it works

```csharp
using Kuwa.OData.Client;

var client = new ODataClient(new Uri("https://services.odata.org/V4/Northwind/Northwind.svc/"));
var first = await client.For("Products").Top(1).FindEntryAsync();
Console.WriteLine(first!["ProductName"]);
```

If this prints a product name, you are set. Otherwise see [[FAQ and Troubleshooting]].

## 3. Create a client

```csharp
using Kuwa.OData.Client;

// From a service root
var client = new ODataClient(new Uri("https://services.odata.org/V4/Northwind/Northwind.svc/"));

// From an HttpClient you own (its BaseAddress is the service root)
var http = new HttpClient { BaseAddress = new Uri("https://example.com/odata/") };
var client2 = new ODataClient(http);

// From settings
var settings = new ODataClientSettings(new Uri("https://example.com/odata/"))
{
    ProtocolVersion = ODataProtocolVersion.V401,
    IgnoreResourceNotFoundException = true,
};
var client3 = new ODataClient(settings);
```

The first request downloads `$metadata` and caches the parsed model for the lifetime of the process, keyed by service root. Every later client for the same root reuses it. See [[Configuration]] for supplying the metadata yourself.

## 4. Read data

```csharp
// Untyped: dictionaries
IDictionary<string, object?>? chai = await client.For("Products").Filter("ProductName eq 'Chai'").FindEntryAsync();

// Typed: your own classes, mapped by property name
public class Product
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = "";
    public decimal? UnitPrice { get; set; }
    public Category? Category { get; set; }
}

IReadOnlyList<Product> products = await client.For<Product>()
    .Filter(p => p.UnitPrice > 20 && p.ProductName.StartsWith("C"))
    .Expand(p => p.Category)
    .OrderByDescending(p => p.UnitPrice)
    .Top(5)
    .FindEntriesAsync();

// Dynamic: no classes
var x = ODataDynamic.Expression;
IEnumerable<dynamic> beverages = await client.For(x.Products).Filter(x.CategoryID == 1).FindEntriesAsync();
```

## 5. Write data

```csharp
var created = await client.For<Product>().Set(new { ProductName = "Tea", UnitPrice = 4.5m }).InsertEntryAsync();
await client.For<Product>().Key(created!.ProductID).Set(new { UnitPrice = 5m }).UpdateEntryAsync();
await client.For<Product>().Key(created.ProductID).DeleteEntryAsync();
```

## 6. Cancel and time out

Every asynchronous method takes an optional `CancellationToken` as its last parameter. `ODataClientSettings.RequestTimeout` bounds each HTTP request.

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var orders = await client.For<Order>().Top(100).FindEntriesAsync(cts.Token);
```

Continue with [[Typed, Dynamic and Untyped APIs]] or [[Querying Data]].

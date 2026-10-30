# Kuwa.OData.Client

A metadata-driven OData V4 / V4.01 client for .NET 10. Work with any OData service through dictionaries, your own
POCOs or dynamic expressions; no code generation required.

```csharp
var client = new ODataClient(new Uri("https://services.odata.org/V4/Northwind/Northwind.svc/"));

var products = await client.For<Product>()
    .Filter(x => x.UnitPrice > 50)
    .Expand(x => x.Category)
    .OrderByDescending(x => x.UnitPrice)
    .Top(5)
    .FindEntriesAsync();

var created = await client.For("Products").Set(new { ProductName = "Tea", UnitPrice = 4.5m }).InsertEntryAsync();
```

Supports the full OData 4.01 surface: `$compute`, `$apply`, `$each`, delta links, multipart and JSON batches,
`respond-async`, server-driven paging, key-as-segment, CSDL XML and JSON metadata, media streams and more.

Documentation, feature list, settings and migration notes:
https://github.com/KuwaTechnologies/Kuwa.OData.Client#readme

Kuwa.OData.Client is a continuation of [Simple.OData.Client](https://github.com/simple-odata-client/Simple.OData.Client)
by Vagif Abilov, David Bond, Robert McLaws and contributors, released under the MIT license.

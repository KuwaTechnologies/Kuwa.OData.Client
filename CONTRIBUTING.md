# Contributing to Kuwa.OData.Client

Thanks for helping! This guide gets you from a fresh clone to a pull request.

## What you need

- The **.NET 10 SDK** (`global.json` pins the minimum).
- Git. No database, no external service: the end-to-end tests run against an in-process OData server.

## Build and test

```bash
dotnet build src/Kuwa.OData.Client.slnx -c Release
dotnet test src/Kuwa.OData.Client.slnx -c Release
```

The library builds with warnings treated as errors, so a clean build means zero warnings. The suite takes about 20 seconds.

Optional extras:

```bash
ODATA_LIVE_TESTS=1 dotnet test src/Kuwa.OData.Client.Tests.Integration                       # real public OData services
dotnet run -c Release --project src/Kuwa.OData.Client.Benchmarks -- --filter '*'              # BenchmarkDotNet
```

## Where things live

| Folder | What is in it |
|---|---|
| `src/Kuwa.OData.Client` | the library |
| `src/Kuwa.OData.Client.Tests` | unit tests and end-to-end tests (`Server/`) |
| `src/Kuwa.OData.Client.Tests.TestServer` | the in-process Northwind OData 4.01 service |
| `src/Kuwa.OData.Client.Tests.Integration` | opt-in live tests |
| `src/Kuwa.OData.Client.Benchmarks` | benchmarks |
| `docs/wiki` | the user guide; mirrored to the GitHub wiki |

## Making a change

1. **Write the test first.** For behaviour that talks to a server, add a scenario under `src/Kuwa.OData.Client.Tests/Server` and watch it fail before you fix the code.
2. Keep the change small and focused; follow the surrounding code style (nullable enabled, async with `CancellationToken`).
3. Run the full test suite and a Release build with no warnings.
4. Update the docs in `docs/wiki` when you change behaviour or add an option, and add a line to `RELEASE_NOTES.md`.
5. Open a pull request describing what changed and why.

## Reporting bugs

Open an issue with: the OData service (or a metadata snippet), the code you ran, the URL the client sent (use `GetCommandTextAsync()` or `OnTrace`), and what you expected instead. See the [FAQ and Troubleshooting](docs/wiki/FAQ-and-Troubleshooting.md) page first.

## Releasing (maintainers)

See [Releasing](docs/wiki/Releasing.md): bump `<Version>`, add the notes, run the **Release** workflow.

## License

By contributing you agree that your contributions are licensed under the MIT License (see `LICENSE`).

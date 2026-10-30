# Releasing

Releases are produced by the `Release` workflow (`.github/workflows/release.yml`). It builds and tests the solution in Release, packs `Kuwa.OData.Client`, creates the GitHub release with the `.nupkg` and `.snupkg` attached, and publishes the package to GitHub Packages.

## Cut a release

1. Set `<Version>` in `src/Kuwa.OData.Client/Kuwa.OData.Client.csproj`.
2. Add the notes: a new section at the top of `RELEASE_NOTES.md`, and optionally a full page at `docs/releases/v<version>.md` (used as the release body when present).
3. Run the **Release** workflow from the Actions tab on the branch to release. The workflow creates the `v<version>` tag at that commit and the release.

Pushing a `v<version>` tag triggers the same workflow; the tag must match the project version.

## Workflow inputs

| Input | Effect |
|---|---|
| `recreate_release` | If the release and tag already exist, delete them and recreate them at the current commit. |
| `publish_to_nuget` | Also push to nuget.org through Trusted Publishing. Needs the one-time setup below. |

## Where the package goes

- **GitHub Packages:** `https://nuget.pkg.github.com/kuwatechnologies/index.json`, package `Kuwa.OData.Client`. Published on every release run with the workflow's own token.
- **Release assets:** the same `.nupkg` and `.snupkg`.
- **nuget.org:** only when `publish_to_nuget` is set, using Trusted Publishing (below).

## Publishing to nuget.org with Trusted Publishing

[Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) means no long-lived API key is stored anywhere. During the run, GitHub issues a short-lived OIDC token, nuget.org checks it against a policy you created, and returns an API key that is valid for one hour. The workflow already requests the token (`id-token: write`) and calls `NuGet/login@v1`.

Set it up once:

1. Sign in at https://www.nuget.org. If the packages should belong to an organization, create it first (your name, then **Organizations**).
2. Open your username menu, then **Trusted Publishing**, and add a policy:
   - **Package owner:** the organization (or yourself)
   - **Repository owner:** `KuwaTechnologies`
   - **Repository:** `Kuwa.OData.Client`
   - **Workflow file:** `release.yml` (file name only)
   - **Environment:** leave empty
   - **Scopes:** allow pushing new packages and versions, glob `Kuwa.OData.Client`
3. In GitHub, open the repository **Settings**, then **Secrets and variables**, **Actions**, **Variables**, and add `NUGET_USER` with your nuget.org **profile name** (not your email).
4. Run the **Release** workflow and tick `publish_to_nuget`. If the release already exists, also tick `recreate_release`.

Notes:

- For a private repository the policy is active for 7 days until the first successful publish records the repository id; publish within that window.
- Each token can be exchanged once and the key lasts one hour, so the login step runs right before the push.
- If the policy owner leaves the organization, the policy goes inactive until they are added back.


## Other workflows

| Workflow | Trigger | Purpose |
|---|---|---|
| `PR Validation` | pull requests | build and test, uploads a TRX report |
| `CI Build & Package` | pushes to `main`/`master` | build, test and pack a CI-versioned package |
| `Wiki` | pushes that change `docs/wiki/**` | syncs this wiki from the repository |

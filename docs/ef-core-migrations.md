# EF Core migrations — how-to

The database schema is defined in C# (`src/JobRadar.Infrastructure/Persistence/`) and changed through
EF Core **migrations**: generated C# files that describe each schema change. Migrations live in
`src/JobRadar.Infrastructure/Persistence/Migrations/` and are committed with the code that needs them.

## One-time setup

`dotnet-ef` is a local tool pinned in `dotnet-tools.json`. After cloning, restore it once:

```powershell
dotnet tool restore
```

## Add a migration after changing the model

Change an entity or a configuration in `Persistence/Configurations/`, then run from the repo root:

```powershell
dotnet ef migrations add <Name> --project src/JobRadar.Infrastructure --output-dir Persistence/Migrations
```

- `<Name>` describes the change in PascalCase, e.g. `AddPostingPollingIndex`.
- No database or Aspire is needed: `DesignTimeDbContextFactory` creates the DbContext with a placeholder
  connection string, and creating a migration never connects.
- Review the generated `<timestamp>_<Name>.cs` before committing. It should contain exactly the change
  you intended. `JobRadarDbContextModelSnapshot.cs` is updated too; commit both.

## See the SQL a migration will run

```powershell
dotnet ef migrations script --project src/JobRadar.Infrastructure
```

Add `--idempotent` to get a script that can run against a database in any state.

## Apply migrations

Migrations will be applied automatically when the system starts through the AppHost
(`dotnet run --project src/JobRadar.AppHost`), so there is no manual `database update` step.
*(Being set up in brief step 2, micro-step 2.8 — this section is updated then.)*

## Undo the last migration (not yet applied anywhere)

```powershell
dotnet ef migrations remove --project src/JobRadar.Infrastructure
```

Only for a migration that has not been committed or applied. Once a migration is shared, fix mistakes
with a new migration instead.

`migrations remove` first connects to the database to check whether the migration was applied. The
design-time connection points at our own `jobradar-postgres` container (port 17300) without a password,
so this check fails. For a migration that was never applied or committed, delete its two files
(`<timestamp>_<Name>.cs` and `<timestamp>_<Name>.Designer.cs`) instead, then regenerate the model snapshot
by adding the next migration, or restore `JobRadarDbContextModelSnapshot.cs` from git.

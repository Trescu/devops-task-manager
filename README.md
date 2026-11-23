# DevOps Task Manager

Egyetemi DevOps projekt: egyszerű ASP.NET Core 8 REST API és webes kezelőfelület, automatizált build, unit tesztek, Docker image build, sandbox deploy és HTTP smoke tesztek.

## Gyorsindítás

Előfeltételek: .NET SDK 8 vagy Docker.

```bash
dotnet run --project src/TaskManager.Api
# http://localhost:5000 helyett a konzol által kiírt URL-t nyisd meg.
```

Docker használatával:

```bash
docker build -t devops-task-manager:local .
docker run --rm -p 8080:8080 -e ASPNETCORE_URLS=http://+:8080 devops-task-manager:local
# http://localhost:8080
```

## Üzleti funkciók

- Feladatok létrehozása, listázása, módosítása, törlése (CRUD)
- Prioritás, státusz, opcionális leírás
- Keresés és állapotszűrés a böngészőben
- REST API: `GET /api/tasks`, `POST /api/tasks`, `PUT /api/tasks/{id}`, `DELETE /api/tasks/{id}`
- `GET /health`: alkalmazás health endpoint

**Adattárolás:** ez egy oktatási sandbox. Az API memóriaalapú tárolót használ; újraindítás vagy serverless példányváltás után a feladatok törlődhetnek. Nem használható éles, tartós adatkezelésre. Az egyes Vercel-példányok külön memóriával rendelkezhetnek.

## CI/CD

`.github/workflows/ci.yml`: minden main push / pull request esetén:

1. `dotnet restore`, `dotnet build`
2. `dotnet test` (xUnit)
3. Docker multi-stage image build
4. Image elindítása GitHub Actions Ubuntu sandboxon
5. HTTP smoke tesztek: health + Create/Read/Update/Delete
6. A tesztelt konténer artifact mentése (Docker tar)

**Felhődeploy állapota:** az első Vercel konténeres build sikeres (READY), de a Vercel GitHub-integráció még nincs engedélyezve ehhez a repositoryhoz. A jelenlegi Vercel deployment kézzel, a repository fájljaiból készült, **nem Git push váltotta ki**. Automatikus Vercel CD-hez a repositoryt a Vercel Git integrációjában hozzá kell rendelni. A Vercel a gyökérkönyvtárban található `Dockerfile` alapján buildel. A `Dockerfile.vercel` alternatív minta, nem az aktív Dockerfile. **Fontos:** a Git-alapú Vercel-deploy külön folyamat, alapból nem vár a GitHub Actions CI eredményére. A CI által ellenőrzött image és a Vercel által felépített image külön buildként készül; automatikus, egymásra váró GitHub Actions → Vercel release-folyamat még nincs beállítva. Éles használathoz release gate szükséges. A projekt célja oktatási sandbox.

## Futtatás és tesztelés lokálisan

```bash
dotnet restore TaskManager.sln
dotnet build TaskManager.sln --configuration Release --no-restore
dotnet test TaskManager.sln --configuration Release --no-build
bash scripts/smoke-test.sh http://localhost:8080
```

## Projekt szerkezete

- `src/TaskManager.Api/`: backend + statikus frontend
- `tests/TaskManager.Tests/`: xUnit egységtesztek
- `scripts/smoke-test.sh`: valós HTTP CRUD teszt
- `Dockerfile`, `Dockerfile.vercel`: konténer build
- `.github/workflows/ci.yml`: automatizálás
- `docs/DOKUMENTACIO.md`: beadandó dokumentáció

## Definition of Done

A CI csak akkor zöld, ha build + unit teszt + konténeres sandbox indítás + HTTP smoke tesztek sikeresek. A Vercel deploy állapotát külön ellenőrizni kell; a zöld CI önmagában nem bizonyítja a felhős telepítés sikerét.

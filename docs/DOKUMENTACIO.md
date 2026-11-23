# DevOps projektfeladat – Részletes műszaki dokumentáció

**Projekt:** DevOps Task Manager  
**Technológiák:** C#, ASP.NET Core 8, xUnit, Docker, GitHub Actions, Vercel Container Runtime  
**Repository:** https://github.com/Trescu/devops-task-manager

## 1. Üzleti szcenárió

Kis csapat egyszerű feladatokat szeretne nyilvántartani. A felhasználó létrehoz, módosít és töröl feladatokat, státuszt és prioritást ad meg. Az üzleti logika tudatosan egyszerű: a beadandó súlypontja az automatizált build–test–deploy lánc.

## 2. Funkcionális követelmények

- CRUD: feladatok létrehozása/listázása/lekérdezése/módosítása/törlése
- Mezők: cím (kötelező, max. 120), leírás (max. 500), státusz (todo / in-progress / done), prioritás (low / medium / high)
- Kliensoldali keresés és szűrés
- Egészségellenőrzés a `GET /health` végponton
- HTTP státuszkódok: 201 létrehozás, 200 sikeres lekérdezés/módosítás, 204 törlés, 400 hibás bemenet, 404 hiányzó erőforrás

## 3. Architektúra

Böngésző → ASP.NET Core statikus webfelület → REST API végpontok → Singleton TaskService → memóriabeli gyűjtemény.

A Vercel Container Runtime HTTP-konténert futtat. Mivel serverless platform, a memória nem perzisztens: egy példány újraindulásakor vagy több példány között a feladatok nem tartósak. A funkciók a CI sandbox folyamatosan futó, egyetlen konténerén ellenőrizhetők megbízhatóan. Ez oktatási prototípus, nem éles feladatkezelő. Tartóssághoz PostgreSQL vagy más külső adatbázis szükséges.

## 4. DevOps CI pipeline

A `.github/workflows/ci.yml` a main branchre érkező push, pull request és manuális indítás esetén:

1. Kód checkout (Git)
2. .NET 8 környezet beállítása
3. `dotnet restore` és `dotnet build --warnaserror`
4. `dotnet test`, xUnit üzleti logikai egységtesztekkel
5. `docker build` multi-stage Dockerfile-ból
6. `docker run`: a létrejött image telepítése GitHub Actions sandbox konténerbe
7. `scripts/smoke-test.sh`: valós API-hívások CRUD teljes folyamaton
8. Docker artifact export és GitHub Actions artifact tárolás (main push esetén)

Sikertelen build/teszt esetén a további lépések nem futnak. A konténeres smoke teszt egy ténylegesen futó artifactot vizsgál. **A GitHub Actions runner ideiglenes, ezért a CI-sandbox megszűnik a job végén.**

## 5. CD felhőbe

A Vercelhez kötött GitHub repository a `main` commitjait automatikusan építi és telepíti. A `Dockerfile.vercel` a felhőben is .NET 8 alapú image-et épít, HTTP-szerverként indítja az alkalmazást, a Vercel által biztosított `PORT` változón figyelve.

**Korlát:** a GitHub Actions CI és a Vercel Git-alapú deploy nem alkot automatikusan, egymásra váró release gate-et. A Vercel build függetlenül indulhat CI-től. Szigorú „csak sikeres teszt után élesíthető” követelmény esetén Vercel Deployment Checks vagy kizárólag CI-ből történő, tokenes telepítés szükséges.

## 6. Tesztstratégia és elfogadási feltételek

| Szint | Vizsgálat | Elfogadási feltétel |
|---|---|---|
| Unit | TaskService létrehozás és validáció | xUnit minden teszt zöld |
| Unit | Feladatmódosítás és törlés | megfelelő értékek és azonosítók |
| Build | Release build | nincs fordítási hiba |
| Docker | image összeállítás | sikeres Docker build |
| Sandbox | HTTP health | GET /health: 200 + healthy |
| Sandbox | teljes CRUD | POST 201, GET 200, PUT 200, DELETE 204, GET 404 |
| Felhő | Vercel deployment | READY és publikus /health elérés |

**Nem állítunk sikeres eredményt addig, amíg a GitHub Actions és a felhődeploy tényleges eredménye nincs ellenőrizve.**

## 7. Biztonság és kockázatok

- Dependenciák buildkor NuGetből és a hivatalos Microsoft konténerképekből
- Nincs hardcoded API-kulcs, nincs felhasználói fiók vagy jogosultsági rendszer
- Public sandbox: az API nem hitelesített; ne tölts fel személyes vagy üzleti adatokat
- Vercel példányváltás esetén memóriabeli adatok elvesznek
- A GitHub Actions csak minimális `contents: read` jogosultságot kér
- Éles rendszerhez auth, perzisztens adatbázis, központi loggyűjtés, rate limit és mentés szükséges

## 8. Reprodukálható kézi ellenőrzés

```bash
git clone https://github.com/Trescu/devops-task-manager.git
cd devops-task-manager
dotnet restore TaskManager.sln
dotnet build TaskManager.sln -c Release --no-restore
dotnet test TaskManager.sln -c Release --no-build
docker build -t task-manager:local .
docker run --rm -p 8080:8080 task-manager:local
# második terminálban:
bash scripts/smoke-test.sh http://localhost:8080
```

## 9. Definition of Done

- A kód verziókövetett repositoryban van.
- Git push után elindul a pipeline.
- A build és a tesztek sikeresek.
- A Docker artifact létrejön és sandboxban elindul.
- A HTTP smoke teszt igazolja az üzleti funkciókat.
- A felhődeploy külön visszaellenőrzése igazolja a publikus elérést.
- Forráskód, konfiguráció, dokumentáció és Elevator Pitch a Neptun-ba leadható.

## 10. Elevator Pitch (vázlat)

„Egy mini feladatkezelő alkalmazást készítettem ASP.NET Core-ban. A projekt hangsúlya a CI/CD folyamat: a GitHub repositoryba érkező push automatikusan fordítást, egységteszteket, Docker buildet és konténeres telepítést indít. A pipeline ezután a futó alkalmazáson HTTP-hívásokkal ellenőrzi a feladatok létrehozását, lekérdezését, módosítását és törlését. A telepített verzió Vercelen is elérhető, a Docker-kép a felhőben automatikusan épül. Ez demonstrálja, hogyan alakítható egy egyszerű alkalmazás reprodukálható, automatizált szállítási folyamattá.”

## 11. Források

- https://learn.microsoft.com/aspnet/core/
- https://docs.github.com/actions
- https://docs.docker.com/build/
- https://vercel.com/docs
- https://vercel.com/changelog/deploy-a-container-from-a-plain-dockerfile

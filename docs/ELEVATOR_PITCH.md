# Elevator Pitch – DevOps Task Manager

**Tervezett felolvasási idő:** körülbelül 1 perc 35 másodperc, normál beszédtempóval.  
**Javasolt formátum:** 4 dia + rövid képernyőfelvétel.

## Felolvasható szöveg

„A projekt célja nem egy bonyolult alkalmazás megépítése volt, hanem annak bemutatása, hogyan automatizálható egy szoftver teljes fejlesztési és telepítési folyamata.

Ennek szemléltetésére egy ASP.NET Core alapú, webes feladatkezelőt készítettem. Az alkalmazásban feladatokat lehet létrehozni, módosítani és törölni, valamint prioritást és állapotot rendelhetünk hozzájuk.

A forráskódot GitHub-repositoryban kezelem. A fő automatizálási eszköz a GitHub Actions: egy kódmódosítás után a rendszer magától lefordítja az alkalmazást, majd kilenc automatizált egységtesztet futtat.

Ha a tesztek sikeresek, elkészül a Docker-konténer. A pipeline ezt el is indítja egy elkülönített sandbox-környezetben, majd valódi HTTP-kérésekkel ellenőrzi az alkalmazás működését. A tesztek nemcsak azt vizsgálják, hogy a program elindul-e, hanem azt is, hogy sikeresen tudunk-e feladatot létrehozni, lekérdezni, módosítani és törölni.

Az ellenőrző futásban mind a kilenc egységteszt és a teljes HTTP smoke teszt sikeres volt. A konténeres alkalmazáshoz Vercel-deploymentet is készítettem.

A megoldás legnagyobb előnye, hogy a build, a tesztelés és a sandbox-telepítés megismételhető, dokumentált folyamatként fut le, kézi lépések nélkül.

Ezzel a projekttel egy egyszerű alkalmazáson keresztül mutatom be a DevOps lényegét: a gyorsabb, ellenőrizhetőbb és megbízhatóbban automatizált szoftverszállítást.”

## Diák és képernyőfelvétel

1. **Üzleti funkció** – az alkalmazás webfelületének bemutatása (20 mp).
2. **Pipeline** – GitHub Actions workflow: restore → build → test → Docker → sandbox → smoke (30 mp).
3. **Bizonyíték** – sikeres 9/9 xUnit és HTTP CRUD smoke eredmény megmutatása (25 mp).
4. **Deploy és összegzés** – Vercel build READY, architektúra és DevOps előnyök (20 mp).

**Megjegyzés:** Amíg a GitHub–Vercel Git-integráció nincs összekötve, a videóban ne állítsd azt, hogy minden Git push automatikus felhődeployt indít. A GitHub Actions sandbox-telepítés viszont ténylegesen automatikus és sikeresen tesztelt.

# dcd. portfolio

Four developers building connected systems for healthcare, hospitality, and retail. ASP.NET Core MVC renders the portfolio from C# models and Razor views. Tailwind CSS styles the views, and GSAP, ScrollTrigger, and Lenis run the animations and smooth scrolling.

## Run locally

Install the .NET 10 SDK and Node.js 22.12 or newer.

```bash
dotnet run
```

Open [the local portfolio](http://localhost:5080/). The .NET build installs the locked npm dependencies on the first run and compiles the frontend into `wwwroot/assets/`. Use `dotnet watch run` while editing.

```bash
dotnet build
dotnet publish -c Release
```

## Edit the portfolio

- `Models/Portfolio.cs` contains the project, service, team, and navigation content.
- `Views/Home/` contains the hero, manifesto, work, services, and team sections. `Views/Shared/` contains the layout, navigation, footer, and shortcuts.
- `Frontend/` contains Tailwind CSS and browser interactions. `wwwroot/images/` contains the project captures and hero photography.

The home page follows the [Blueprint Apps](https://blueprintapps.io/) opening and [Pier Gorelli](https://piergorelli.com/) editorial sections. The hero uses generated editorial photographs and the portfolio's existing product screenshots.

## Verify

See [the verification checklist](docs/verification.md) for the browser smoke checks.

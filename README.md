# dcd. portfolio

Four developers building connected systems for healthcare, hospitality, and retail. ASP.NET Core MVC renders the portfolio from C# models and Razor views. Tailwind CSS styles the views, and GSAP, ScrollTrigger, and Lenis run the animations and smooth scrolling.

## Run locally

Install the .NET 10 SDK, Node.js 22.12 or newer and SQL Server Express LocalDB on Windows. Development uses Windows authentication and a database named `DcdPortfolio`. For another SQL Server instance, set the `ConnectionStrings__Portfolio` environment variable.

To provision the first administrator, set `LocalAdmin:Email` and `LocalAdmin:Password` with .NET User Secrets before the first run. Existing local administrator settings with a salt and password hash also work. Use an unused email address. Bootstrap only provisions the first administrator and never promotes a registered member. Keep credentials outside the repository.

```bash
dotnet run
```

Open [the local portfolio](http://localhost:5080/). Development startup applies EF Core migrations and seeds the portfolio once. Create a member account at `/account/sign-up`, then sign in. Members can edit their account at `/account/profile`. Administrators can manage content, accounts and reports at `/admin`. The build installs locked npm dependencies and compiles the frontend into `wwwroot/assets/`.

```bash
dotnet build
dotnet publish -c Release
```

## Edit the portfolio

- `Models/Portfolio.cs` contains the original portfolio records used to initialize the database. Use the admin screens for changes after initialization.
- `Views/Home/` contains the hero, manifesto, work, services, and team sections. `Views/Shared/` contains the layout, navigation, footer, and shortcuts.
- `Frontend/` contains Tailwind CSS and browser interactions. `wwwroot/images/` contains the project captures and hero photography.

The home page follows the [Blueprint Apps](https://blueprintapps.io/) opening and [Pier Gorelli](https://piergorelli.com/) editorial sections. The hero uses generated editorial photographs and the portfolio's existing product screenshots.

## Verify

See [the verification checklist](docs/verification.md) for the browser smoke checks.

## Database and deployment

The SQL Server schema includes Identity credentials and roles, portfolio content, uploaded images and dated activity records. EF migration history tracks schema changes. Application users and database credentials serve separate purposes.

In SQL Server Object Explorer, open the six content tables (`Home`, `About`, `Works`, `Services`, `Team`, and `Contacts`) to inspect individual fields. Ordered values live in `HomeImages`, `WorkParagraphs`, `WorkTechnologies`, `ServiceTags`, and `TeamTechnologies`. Each child row has a parent key and a zero-based position. Service tags render in that order with their text and a built-in icon. Custom labels use a generic tag icon.

Under **Programmability → Stored Procedures**, the portfolio read, save, delete and owner procedures operate on these tables. Content writes share a transaction with image changes and activity logging. Account status changes, audit history and report totals also use stored procedures. ASP.NET Core Identity handles credentials, sign-in and roles.

For deployment, configure `ConnectionStrings__Portfolio` for the intended SQL Server and supply the initial administrator secrets to the initialization process. After publishing, run the following once per deployment with a database principal allowed to migrate the schema. Start the web application under its normal read/write database principal and serve it over HTTPS.

```sh
dotnet Dcd.Web.dll --initialize-database
dotnet Dcd.Web.dll
```

For future schema changes run `dotnet tool restore`, then `dotnet ef migrations add <Name>` in Development. Review and test the migration against a database copy before deployment. Back up SQL Server and persist the ASP.NET Core Data Protection key ring across restarts and instances.

Password recovery sends a time-limited reset link. Configure `Recovery__PublicOrigin` with the site's trusted HTTPS origin and `Smtp__Host`, `Smtp__From`, and optionally `Smtp__Port` (default 587), `Smtp__User` and `Smtp__Password` for the mail relay.

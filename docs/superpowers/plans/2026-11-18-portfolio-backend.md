# Portfolio backend implementation plan

The IPT 101 assignment requires ASP.NET MVC, Entity Framework and Microsoft SQL Server. Visitors register and sign in before viewing the portfolio. Members can edit their own information. Administrators can inspect and deactivate accounts, view statistics and edit website content.

1. Configure SQL Server, EF Core migrations and ASP.NET Core Identity in `Program.cs` and `Data/`. Initialize an empty database from the current portfolio and provision the first administrator from operator-supplied credentials. Repeat initialization to check it preserves edits.
2. Connect the existing account forms to Identity, add a protected profile editor and password recovery. Check duplicate addresses, incorrect passwords, isolated profile updates, sign-out and member access restrictions.
3. Connect the existing content forms and public Razor pages to the same database records. Keep the eight existing projects, three services and four team profiles. Verify a saved change appears on the signed-in portfolio and survives restart.
4. Replace dashboard fixtures with account records, dated statistics and an activity log. Use stable database identifiers for account management. Verify a deactivated member loses access and exported counts agree with the dashboard.
5. Build and publish, run against a separate LocalDB database, exercise the HTTP flows, and check the portfolio at the breakpoints in `docs/verification.md`. Document configuration and distinguish verified behavior from external delivery or deployment steps.

namespace Dcd.Web.Models;

public sealed record NavLink(string Id, string Number, string Label);
public sealed record Project(string Id, string Name, string Category, string[] Details, string[] Stack,
    string Url, string? Image = null, string? AlternateImage = null, string? Github = null, string TitleSize = "12dvh",
    string? LightImage = null, string? LightAlternateImage = null);
public sealed record Service(string Number, string Title, string Body, string[] Tags, string Cursor);
public sealed record TeamMember(string Id, string Slot, string Name, string Role, string Bio, string[] Stack,
    string? Github, string Status, string SvgShape, string? FullName = null, string? Email = null, bool IsOwner = false);
public sealed record PortfolioPage(NavLink[] Navigation, Project[] Projects, Service[] Services, TeamMember[] Team);

public static class Portfolio
{
    private static readonly NavLink[] Navigation =
    [
        new("about", "01", "About"),
        new("work", "02", "Works"),
        new("services", "03", "Services"),
        new("team", "04", "Team"),
        new("contact", "05", "Contacts"),
    ];

    private static readonly Project[] Projects =
    [
        new("samson-dental-center", "Samson Dental Center", "Healthcare platform",
            ["A care portal for a dental clinic in Baguio City. Patients can choose an appointment slot online and review their treatment history.",
             "The clinical workspace brings scheduling and patient records together so staff can follow the same information from booking through the visit."],
            ["Next.js", "React", "TypeScript", "Tailwind CSS", "Supabase", "PostgreSQL"],
            "https://samsondentalcenter-website.chrbuilds.dev/", "project-samson.png", "project-samson-hover.png", "https://github.com/ChrstprJohn/SamsonDentalCenterWebsite", TitleSize: "10dvh"),
        new("hole-in-hub", "Hole In Hub Golf & Lounge", "Hospitality & reservations",
            ["A venue site for an indoor golf lounge. Visitors can explore simulator bays, compare coaching options, and get to reservations from the same site.",
             "The reservation subsystem supports the lounge’s day-to-day bay bookings. The project links to both the customer site and the reservation code."],
            ["ASP.NET Core MVC", "C#", "MongoDB", "Tailwind CSS", "JavaScript", "GSAP"],
            "https://holeinhub.runasp.net/", "project-hole-hero-desktop.png", "project-hole-programs.png", "https://github.com/marcKevzzz/HoleInHubReservationManagementSubsystem", "10dvh"),
        new("helmet-cartel", "Helmet Cartel", "Retail & inventory",
            ["An online storefront for browsing motorcycle helmets across multiple brands and placing orders.",
             "The operations side connects ordering with stock and sales workflows. The two project previews show the storefront and the management dashboard."],
            ["ASP.NET Web Forms", "C#", "SQL Server", "JavaScript", "SignalR", "Chart.js"],
            "https://github.com/marcKevzzz/HelmetCartelOrderingAndManagementSys", "helmet-storefront.png", "helmet-operations.png", "https://github.com/marcKevzzz/HelmetCartelOrderingAndManagementSys"),
        new("schedsnap", "SchedSnap", "Creative tool",
            ["SchedSnap turns a class timetable into a wallpaper that students can glance at throughout the day.",
             "Pick a design, add classes and meeting times, and export a PNG sized for a phone, tablet, or laptop."],
            ["React", "JavaScript", "Tailwind CSS", "GSAP", "Supabase", "Vite"],
            "https://schedsnap.chrbuilds.dev/", "project-schedsnap-home.png", "project-schedsnap.png", "https://github.com/dcd-inc/SchedSnap"),
        new("dynamic-website", "Dynamic Website", "ASP.NET web application",
            ["A personal portfolio built with ASP.NET Web Forms, presenting projects, skills, experience, and contact details.",
             "A website builder lets the owner update their public profile, hero, technology stack, and portfolio content from a dedicated management workspace."],
            ["ASP.NET Web Forms", "C#", "SQL Server", "JavaScript", "GSAP", "ASP.NET Web API"],
            "http://marckevzzz.runasp.net/", "project-dynamic.png", "project-dynamic-builder.png", "https://github.com/marcKevzzz/Personal-Portfolio"),
        new("tower-of-hanoi", "Tower of Hanoi", "Interactive puzzle",
            ["A browser version of the classic three-peg puzzle. Move the disks one at a time and keep larger disks off smaller ones.",
             "The puzzle makes the rules visible through the changing stacks, so players can plan the next move and see their progress."],
            ["JavaScript", "React", "Framer Motion", "Tailwind CSS", "Vite"],
            "https://tower-of-hanoi-delta.vercel.app/", "project-hanoi-dark.png", "project-hanoi-dark-hover.png", Github: "https://github.com/marcKevzzz/towerOfHanoi", LightImage: "project-hanoi-light.png", LightAlternateImage: "project-hanoi-light-hover.png"),
        new("programming-fundamentals", "Programming Fundamentals", "Programming exercises",
            ["A web collection of introductory programming exercises, organized around a program index.",
             "The site gives learners a single place to browse the exercises and revisit earlier examples while practicing."],
            ["ASP.NET Core MVC", "C#", "Razor", "Bootstrap", "Bootstrap Icons", "jQuery"],
            "http://programmingfundamentals.runasp.net/", "project-programming-dark.png", "project-programming-dark-hover.png", "https://github.com/marcKevzzz/BasicProgrammingLanguage", "9dvh", "project-programming-light.png", "project-programming-light-hover.png"),
        new("cpu-scheduling-calculator", "CPU Scheduling Calculator", "Operating systems tool",
            ["A browser calculator for comparing how CPU scheduling policies decide which process runs next.",
             "Timelines make the execution order visible, while waiting and turnaround times give students a way to compare results."],
            ["JavaScript", "HTML", "CSS", "Bootstrap", "Bootstrap Icons", "jQuery"],
            "https://marckevzzz.github.io/CPU-Scheduling-Calculator/", "project-cpu.png", "project-cpu-hover.png", Github: "https://github.com/marcKevzzz/CPU-Scheduling-Calculator", TitleSize: "9dvh"),
    ];

    private static readonly Service[] Services =
    [
        new("01", "Web development for agencies", "Motion-heavy sites and CMS builds, built closely with your designers and delivered ready for your team to own.", ["Next.js", "GSAP", "React", "TypeScript"], "web dev →"),
        new("02", "AI & automation workflows", "Hands-on training and custom skills for tools like Codex, followed by the workflows your team needs. With an honest list of what not to automate.", ["Codex", "Custom Agents", "Tooling", "Workflows"], "ai workflows →"),
        new("03", "AI in products & systems architecture", "Semantic search and assistants grounded in your content, tested on the questions real users ask, answering with sources and verifiable telemetry.", ["RAG", "Semantic Search", "PostgreSQL", "Real-time Sync"], "systems →"),
    ];

    private static readonly TeamMember[] Team =
    [
        new("marc-kevin", "DEV / 01", "Kevzz", "UI/UX Designer & Interaction Systems", "User experience, interface design, and interaction systems that connect clear workflows with responsive, considered motion.",
            ["Figma", "GSAP", "CSS", "Accessibility"],
            "https://github.com/marcKevzzz",
            "ACTIVE · COMMITTING",
            "reticle", FullName: "Del Mundo, Marc Kevin F.", Email: "delmundo.marckevin.ferolino@gmail.com", IsOwner: true),
        new("alex-rivera", "DEV / 02", "Pixyz", "Frontend Development & Networking", "Responsive frontend development and networking, connecting accessible interfaces with reliable communication between systems.",
            ["React", "JavaScript", "TCP/IP", "Cisco Packet Tracer"],
            "https://github.com/Pixyz-bit",
            "ACTIVE · COMMITTING",
            "waveform"),
        new("christian-reyes", "DEV / 03", "Leo", "Frontend & Data Infrastructure", "Frontend interfaces and data infrastructure, bringing application screens, structured data, and API integrations together.",
            ["React", "TypeScript", "PostgreSQL", "Supabase"],
            "https://github.com/Leokkise",
            "ACTIVE · COMMITTING",
            "matrix"),
        new("rafael-santos", "DEV / 04", "Chrstpr", "DevOps & Systems Deployment", "Automated CI/CD build environments, production containerization, load optimization, and server uptime.",
            ["Docker", "CI/CD Pipelines", "Cloud Hosting", "Performance"],
            "https://github.com/ChrstprJohn",
            "ACTIVE · COMMITTING",
            "cube"),
    ];

    public static PortfolioPage Content { get; } = new(Navigation, Projects, Services, Team);
}

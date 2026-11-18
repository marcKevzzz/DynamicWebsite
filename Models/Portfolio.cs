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
        new("cosmicgaze", "CosmicGaze", "Astronomy explorer",
            ["Find NASA’s picture for your birthday or explore the Moon’s phase on a date of your choice.",
             "Compare two birthday Moon phases and discover how old you would be on other planets."],
            ["React", "Vite", "Tailwind CSS", "Axios", "GSAP", "Astronomy Engine", "Three.js"], "https://cosmicgaze.chrbuilds.dev/#services", "project-cosmicgaze.png", "project-cosmicgaze-explore.png", "https://github.com/ChrstprJohn/CosmicGaze"),
        new("signwise", "SignWise", "Document review",
            ["Upload a document or photograph a page to review the terms before signing.",
             "The review helps readers identify potential red flags and questions to ask."],
            ["React", "Vite", "Tailwind CSS", "Gemini API", "Mammoth", "jsPDF"], "https://signwisely.vercel.app/", "project-signwise.png", "project-signwise-review.png", "https://github.com/ChrstprJohn/SignWise"),
        new("programming-fundamentals", "Programming Fundamentals", "Programming exercises",
            ["A web collection of introductory programming exercises, organized around a program index.",
             "The site gives learners a single place to browse the exercises and revisit earlier examples while practicing."],
            ["ASP.NET Core MVC", "C#", "Razor", "Bootstrap", "Bootstrap Icons", "jQuery"],
            "http://programmingfundamentals.runasp.net/", "project-programming-dark.png", "project-programming-dark-hover.png", "https://github.com/marcKevzzz/BasicProgrammingLanguage", "9dvh", "project-programming-light.png", "project-programming-light-hover.png"),
        new("pixelplaza", "PixelPlaza", "Social pixel world",
            ["Choose a character and explore a shared pixel-art village.",
             "Meet other visitors in the plaza and talk through the in-game chat."],
            ["React", "Phaser", "Vite", "Node.js", "Redis", "Centrifugo"], "https://pixelplaza-live.vercel.app/", "project-pixelplaza.png", "project-pixelplaza-world.png", "https://github.com/ChrstprJohn/PixelPlaza"),
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
        new("arlan-martin", "DEV / 02", "Pixyz", "Frontend Development & Networking", "Responsive frontend development and networking, connecting accessible interfaces with reliable communication between systems.",
            ["React", "JavaScript", "TCP/IP", "Cisco Packet Tracer"],
            "https://github.com/Pixyz-bit",
            "ACTIVE · COMMITTING",
            "waveform", FullName: "Jalop, Arlan Martin N.", Email: "jalop.arlanmartin.naraga@gmail.com"),
        new("sam-leonard", "DEV / 03", "Leo", "Frontend & Data Infrastructure", "Frontend interfaces and data infrastructure, bringing application screens, structured data, and API integrations together.",
            ["React", "TypeScript", "PostgreSQL", "Supabase"],
            "https://github.com/Leokkise",
            "ACTIVE · COMMITTING",
            "matrix", FullName: "Barber, Sam Leonard M.", Email: "barber.samleonard.majaba@gmail.com"),
        new("christopher-john", "DEV / 04", "Chrstpr", "DevOps & Systems Deployment", "Automated CI/CD build environments, production containerization, load optimization, and server uptime.",
            ["Docker", "CI/CD Pipelines", "Cloud Hosting", "Performance"],
            "https://github.com/ChrstprJohn",
            "ACTIVE · COMMITTING",
            "cube", FullName: "Picardo, Christopher John O.", Email: "picardochristopherjohnoleo1@gmail.com"),
    ];

    public static PortfolioPage Content { get; } = new(Navigation, Projects, Services, Team);
}

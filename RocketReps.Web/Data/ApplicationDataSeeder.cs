using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace RocketReps.Web.Data;

public static class ApplicationDataSeeder
{
    private static readonly string[] Roles = ["Admin", "Teacher", "Student"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var riverview = await dbContext.Schools.SingleOrDefaultAsync(school => school.Name == "Riverview STEM Academy");
        if (riverview is null)
        {
            riverview = new School
            {
                Id = Guid.NewGuid(),
                Name = "Riverview STEM Academy",
                Mascot = "Rockets",
            };

            dbContext.Schools.Add(riverview);
        }

        await SeedMathDeckAsync(dbContext, "Addition Launch Pad", "Practice addition facts from 0 through 12.", BuildAdditionCards());
        await SeedMathDeckAsync(dbContext, "Subtraction Orbit", "Practice subtraction facts with answers from 0 through 12.", BuildSubtractionCards());
        await SeedMathDeckAsync(dbContext, "Multiplication Mission", "Practice multiplication facts from 0 through 12.", BuildMultiplicationCards());
        await SeedMathDeckAsync(dbContext, "Division Docking", "Practice division facts connected to the 1 through 12 times tables.", BuildDivisionCards());

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedMathDeckAsync(ApplicationDbContext dbContext, string title, string description, IReadOnlyList<MathFactSeed> facts)
    {
        var deck = await dbContext.Decks.Include(deck => deck.Cards).SingleOrDefaultAsync(deck => deck.IsGlobalStock && deck.Title == title);
        if (deck is null)
        {
            deck = new Deck
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                Subject = "Math",
                GradeBand = "Elementary and Middle School",
                IsGlobalStock = true,
                IsPublished = true,
            };

            dbContext.Decks.Add(deck);
        }

        if (deck.Cards.Count > 0)
        {
            return;
        }

        for (var index = 0; index < facts.Count; index++)
        {
            var fact = facts[index];
            deck.Cards.Add(new Card
            {
                Id = Guid.NewGuid(),
                Front = fact.Question,
                Back = fact.Answer,
                CorrectAnswer = fact.Answer,
                CardType = CardType.MathFact,
                SortOrder = index + 1,
            });
        }
    }

    private static List<MathFactSeed> BuildAdditionCards()
    {
        var facts = new List<MathFactSeed>();
        for (var left = 0; left <= 12; left++)
        {
            for (var right = 0; right <= 12; right++)
            {
                facts.Add(new MathFactSeed($"{left} + {right}", (left + right).ToString()));
            }
        }

        return facts;
    }

    private static List<MathFactSeed> BuildSubtractionCards()
    {
        var facts = new List<MathFactSeed>();
        for (var left = 0; left <= 12; left++)
        {
            for (var right = 0; right <= left; right++)
            {
                facts.Add(new MathFactSeed($"{left} - {right}", (left - right).ToString()));
            }
        }

        return facts;
    }

    private static List<MathFactSeed> BuildMultiplicationCards()
    {
        var facts = new List<MathFactSeed>();
        for (var left = 0; left <= 12; left++)
        {
            for (var right = 0; right <= 12; right++)
            {
                facts.Add(new MathFactSeed($"{left} x {right}", (left * right).ToString()));
            }
        }

        return facts;
    }

    private static List<MathFactSeed> BuildDivisionCards()
    {
        var facts = new List<MathFactSeed>();
        for (var divisor = 1; divisor <= 12; divisor++)
        {
            for (var answer = 1; answer <= 12; answer++)
            {
                facts.Add(new MathFactSeed($"{divisor * answer} / {divisor}", answer.ToString()));
            }
        }

        return facts;
    }

    private sealed record MathFactSeed(string Question, string Answer);
}

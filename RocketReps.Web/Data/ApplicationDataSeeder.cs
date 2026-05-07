using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace RocketReps.Web.Data;

public static class ApplicationDataSeeder
{
    private static readonly string[] Roles = ["Admin", "Teacher", "Student"];

    private const int LargestMathFactNumber = 12;

    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await SeedIdentityRolesAsync(services);

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
        await SeedFocusedMathDecksAsync(dbContext);
        await SeedSpellingDeckAsync(dbContext);
        await SeedCaliforniaFactsDeckAsync(dbContext);

        await dbContext.SaveChangesAsync();

        var demoOptions = services.GetRequiredService<IOptions<DemoOptions>>().Value;
        if (demoOptions.Enabled)
        {
            await DemoDataSeeder.SeedAsync(services);
        }
    }

    public static async Task SeedIdentityRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create identity role '{role}': {string.Join(", ", result.Errors.Select(error => error.Description))}");
                }
            }
        }
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

    private static async Task SeedFocusedMathDecksAsync(ApplicationDbContext dbContext)
    {
        for (var number = 0; number <= LargestMathFactNumber; number++)
        {
            await SeedMathDeckAsync(
                dbContext,
                $"Addition Launch Pad: {number}s",
                $"Practice addition facts with {number} from 0 through {LargestMathFactNumber}.",
                BuildAdditionCards(number));

            await SeedMathDeckAsync(
                dbContext,
                $"Subtraction Orbit: {number}s",
                $"Practice subtraction facts subtracting {number}, with answers from 0 through {LargestMathFactNumber}.",
                BuildSubtractionCards(number));

            await SeedMathDeckAsync(
                dbContext,
                $"Multiplication Mission: {number}s",
                $"Practice multiplication facts with {number} from 0 through {LargestMathFactNumber}.",
                BuildMultiplicationCards(number));
        }

        for (var divisor = 1; divisor <= LargestMathFactNumber; divisor++)
        {
            await SeedMathDeckAsync(
                dbContext,
                $"Division Docking: {divisor}s",
                $"Practice division facts by {divisor}, with answers from 0 through {LargestMathFactNumber}.",
                BuildDivisionCards(divisor));
        }
    }

    private static async Task SeedSpellingDeckAsync(ApplicationDbContext dbContext)
    {
        var words = new[]
        {
            "rocket",
            "planet",
            "launch",
            "orbit",
            "comet",
            "galaxy",
            "student",
            "teacher",
            "number",
            "science",
            "problem",
            "answer",
            "practice",
            "mission",
            "energy",
            "future",
            "explore",
            "engine",
            "window",
            "bright",
        };

        var deck = await dbContext.Decks.Include(deck => deck.Cards).SingleOrDefaultAsync(deck => deck.IsGlobalStock && deck.Title == "Spelling Lift-Off");
        if (deck is null)
        {
            deck = new Deck
            {
                Id = Guid.NewGuid(),
                Title = "Spelling Lift-Off",
                Description = "Hear each word, then type the spelling from memory.",
                Subject = "Spelling",
                GradeBand = "Elementary",
                IsGlobalStock = true,
                IsPublished = true,
            };

            dbContext.Decks.Add(deck);
        }

        if (deck.Cards.Count > 0)
        {
            return;
        }

        for (var index = 0; index < words.Length; index++)
        {
            var word = words[index];
            deck.Cards.Add(new Card
            {
                Id = Guid.NewGuid(),
                Front = "Listen and spell the word.",
                Back = word,
                CorrectAnswer = word,
                CardType = CardType.AudioPrompt,
                SortOrder = index + 1,
            });
        }
    }

    private static async Task SeedCaliforniaFactsDeckAsync(ApplicationDbContext dbContext)
    {
        var facts = new[]
        {
            new MultipleChoiceSeed("What is the capital of California?", "Sacramento", ["Sacramento", "Los Angeles", "San Francisco", "San Diego"]),
            new MultipleChoiceSeed("What is California's nickname?", "The Golden State", ["The Golden State", "The Sunshine State", "The Empire State", "The Garden State"]),
            new MultipleChoiceSeed("Which ocean borders California?", "Pacific Ocean", ["Pacific Ocean", "Atlantic Ocean", "Indian Ocean", "Arctic Ocean"]),
            new MultipleChoiceSeed("What is California's state flower?", "California poppy", ["California poppy", "Rose", "Sunflower", "Bluebonnet"]),
            new MultipleChoiceSeed("What is California's state bird?", "California quail", ["California quail", "Bald eagle", "Roadrunner", "Robin"]),
            new MultipleChoiceSeed("What is California's state animal?", "California grizzly bear", ["California grizzly bear", "Mountain lion", "Gray wolf", "Black bear"]),
            new MultipleChoiceSeed("What is California's state tree?", "Coast redwood", ["Coast redwood", "Palm tree", "Joshua tree", "Maple tree"]),
            new MultipleChoiceSeed("What is the largest city in California?", "Los Angeles", ["Los Angeles", "Sacramento", "San Jose", "Fresno"]),
            new MultipleChoiceSeed("Which famous bridge is in San Francisco?", "Golden Gate Bridge", ["Golden Gate Bridge", "Brooklyn Bridge", "London Bridge", "Sunshine Skyway Bridge"]),
            new MultipleChoiceSeed("Which national park in California is famous for Yosemite Falls and Half Dome?", "Yosemite National Park", ["Yosemite National Park", "Yellowstone National Park", "Grand Canyon National Park", "Everglades National Park"]),
            new MultipleChoiceSeed("Which mountain range runs through eastern California?", "Sierra Nevada", ["Sierra Nevada", "Appalachian Mountains", "Ozark Mountains", "Cascade Range"]),
            new MultipleChoiceSeed("In what year did the California Gold Rush begin?", "1848", ["1848", "1776", "1906", "1969"]),
            new MultipleChoiceSeed("What is California's postal abbreviation?", "CA", ["CA", "CL", "CF", "CO"]),
            new MultipleChoiceSeed("Which country borders California to the south?", "Mexico", ["Mexico", "Canada", "Brazil", "Spain"]),
            new MultipleChoiceSeed("Which California valley is known for growing lots of food?", "Central Valley", ["Central Valley", "Death Valley", "Silicon Valley", "Monument Valley"]),
            new MultipleChoiceSeed("Which desert is partly in southeastern California?", "Mojave Desert", ["Mojave Desert", "Sahara Desert", "Gobi Desert", "Great Victoria Desert"]),
        };

        var deck = await dbContext.Decks.Include(deck => deck.Cards).SingleOrDefaultAsync(deck => deck.IsGlobalStock && deck.Title == "California Facts");
        if (deck is null)
        {
            deck = new Deck
            {
                Id = Guid.NewGuid(),
                Title = "California Facts",
                Description = "Practice quick facts about California with tap-friendly multiple choice questions.",
                Subject = "Social Studies",
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

        for (var index = 0; index < facts.Length; index++)
        {
            var fact = facts[index];
            deck.Cards.Add(new Card
            {
                Id = Guid.NewGuid(),
                Front = fact.Question,
                Back = fact.CorrectAnswer,
                CorrectAnswer = fact.CorrectAnswer,
                ChoicesJson = JsonSerializer.Serialize(fact.Choices),
                CardType = CardType.MultipleChoice,
                SortOrder = index + 1,
            });
        }
    }

    private static List<MathFactSeed> BuildAdditionCards()
    {
        var facts = new List<MathFactSeed>();
        for (var left = 0; left <= LargestMathFactNumber; left++)
        {
            for (var right = 0; right <= LargestMathFactNumber; right++)
            {
                facts.Add(new MathFactSeed($"{left} + {right}", (left + right).ToString()));
            }
        }

        return facts;
    }

    private static List<MathFactSeed> BuildAdditionCards(int addend)
    {
        var facts = new List<MathFactSeed>();
        for (var otherAddend = 0; otherAddend <= LargestMathFactNumber; otherAddend++)
        {
            facts.Add(new MathFactSeed($"{addend} + {otherAddend}", (addend + otherAddend).ToString()));
        }

        return facts;
    }

    private static List<MathFactSeed> BuildSubtractionCards()
    {
        var facts = new List<MathFactSeed>();
        for (var left = 0; left <= LargestMathFactNumber; left++)
        {
            for (var right = 0; right <= left; right++)
            {
                facts.Add(new MathFactSeed($"{left} - {right}", (left - right).ToString()));
            }
        }

        return facts;
    }

    private static List<MathFactSeed> BuildSubtractionCards(int subtrahend)
    {
        var facts = new List<MathFactSeed>();
        for (var answer = 0; answer <= LargestMathFactNumber; answer++)
        {
            facts.Add(new MathFactSeed($"{answer + subtrahend} - {subtrahend}", answer.ToString()));
        }

        return facts;
    }

    private static List<MathFactSeed> BuildMultiplicationCards()
    {
        var facts = new List<MathFactSeed>();
        for (var left = 0; left <= LargestMathFactNumber; left++)
        {
            for (var right = 0; right <= LargestMathFactNumber; right++)
            {
                facts.Add(new MathFactSeed($"{left} x {right}", (left * right).ToString()));
            }
        }

        return facts;
    }

    private static List<MathFactSeed> BuildMultiplicationCards(int factor)
    {
        var facts = new List<MathFactSeed>();
        for (var otherFactor = 0; otherFactor <= LargestMathFactNumber; otherFactor++)
        {
            facts.Add(new MathFactSeed($"{factor} x {otherFactor}", (factor * otherFactor).ToString()));
        }

        return facts;
    }

    private static List<MathFactSeed> BuildDivisionCards()
    {
        var facts = new List<MathFactSeed>();
        for (var divisor = 1; divisor <= LargestMathFactNumber; divisor++)
        {
            for (var answer = 1; answer <= LargestMathFactNumber; answer++)
            {
                facts.Add(new MathFactSeed($"{divisor * answer} / {divisor}", answer.ToString()));
            }
        }

        return facts;
    }

    private static List<MathFactSeed> BuildDivisionCards(int divisor)
    {
        var facts = new List<MathFactSeed>();
        for (var answer = 0; answer <= LargestMathFactNumber; answer++)
        {
            facts.Add(new MathFactSeed($"{divisor * answer} / {divisor}", answer.ToString()));
        }

        return facts;
    }

    private sealed record MathFactSeed(string Question, string Answer);

    private sealed record MultipleChoiceSeed(string Question, string CorrectAnswer, IReadOnlyList<string> Choices);
}

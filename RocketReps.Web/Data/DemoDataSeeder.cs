using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace RocketReps.Web.Data;

public static class DemoDataSeeder
{
    public const string TeacherUserName = "demo.teacher";
    public const string TeacherEmail = "demo.teacher@rocketreps.local";
    public const string StudentUserNamePrefix = "demo.student";

    private const string DemoPassword = "Rocket-Demo-2026!";
    private const int StudentCount = 30;

    private static readonly DemoClassSeed[] DemoClasses =
    [
        new("Mission Control Math", "DEMO01", ["Addition Launch Pad: 1s", "Addition Launch Pad: 2s", "Multiplication Mission: 5s", "Spelling Lift-Off", "California Facts"]),
        new("Rocket Lab Facts", "DEMO02", ["Subtraction Orbit: 3s", "Multiplication Mission: 10s", "Division Docking: 2s", "Spelling Lift-Off", "California Facts"]),
    ];

    public static string StudentUserName(int number) => $"{StudentUserNamePrefix}{number:00}";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var school = await dbContext.Schools.SingleAsync(school => school.Name == "Riverview STEM Academy");
        var teacher = await EnsureUserAsync(
            userManager,
            TeacherUserName,
            "Demo Teacher",
            school.Id,
            "Teacher",
            TeacherEmail);

        for (var classIndex = 0; classIndex < DemoClasses.Length; classIndex++)
        {
            var classSeed = DemoClasses[classIndex];
            var classroom = await EnsureClassroomAsync(dbContext, school.Id, teacher.Id, classSeed);
            await EnsureMembershipAsync(dbContext, classroom.Id, teacher.Id, ClassroomMemberRole.Teacher, teacher.Id);

            foreach (var deckTitle in classSeed.ActiveDeckTitles)
            {
                await EnsureDeckAssignmentAsync(dbContext, classroom.Id, teacher.Id, deckTitle);
            }

            var firstStudentNumber = classIndex * (StudentCount / DemoClasses.Length) + 1;
            var lastStudentNumber = firstStudentNumber + (StudentCount / DemoClasses.Length) - 1;
            for (var number = firstStudentNumber; number <= lastStudentNumber; number++)
            {
                var student = await EnsureUserAsync(
                    userManager,
                    StudentUserName(number),
                    $"Demo Student {number:00}",
                    school.Id,
                    "Student");

                await EnsureMembershipAsync(dbContext, classroom.Id, student.Id, ClassroomMemberRole.Student, teacher.Id);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string displayName,
        Guid schoolId,
        string role,
        string? email = null)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName,
                SchoolId = schoolId,
            };

            var createResult = await userManager.CreateAsync(user, DemoPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create demo user '{userName}': {FormatErrors(createResult)}");
            }
        }
        else if (user.DisplayName != displayName || user.SchoolId != schoolId || user.Email != email || !user.EmailConfirmed)
        {
            user.DisplayName = displayName;
            user.SchoolId = schoolId;
            user.Email = email;
            user.EmailConfirmed = true;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update demo user '{userName}': {FormatErrors(updateResult)}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to add demo user '{userName}' to role '{role}': {FormatErrors(roleResult)}");
            }
        }

        return user;
    }

    private static async Task<Classroom> EnsureClassroomAsync(
        ApplicationDbContext dbContext,
        Guid schoolId,
        string teacherId,
        DemoClassSeed classSeed)
    {
        var classroom = await dbContext.Classrooms.SingleOrDefaultAsync(classroom => classroom.JoinCode == classSeed.JoinCode);
        if (classroom is null)
        {
            classroom = new Classroom
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                CreatedByTeacherId = teacherId,
                Name = classSeed.Name,
                JoinCode = classSeed.JoinCode,
                RequiresTeacherApproval = true,
            };

            dbContext.Classrooms.Add(classroom);
        }
        else
        {
            classroom.SchoolId = schoolId;
            classroom.CreatedByTeacherId = teacherId;
            classroom.Name = classSeed.Name;
        }

        return classroom;
    }

    private static async Task EnsureMembershipAsync(
        ApplicationDbContext dbContext,
        Guid classroomId,
        string userId,
        ClassroomMemberRole role,
        string approvedByTeacherId)
    {
        var membership = await dbContext.ClassroomMemberships.SingleOrDefaultAsync(membership =>
            membership.ClassroomId == classroomId && membership.UserId == userId);

        if (membership is null)
        {
            dbContext.ClassroomMemberships.Add(new ClassroomMembership
            {
                Id = Guid.NewGuid(),
                ClassroomId = classroomId,
                UserId = userId,
                Role = role,
                Status = ClassroomMemberStatus.Approved,
                ApprovedAt = DateTimeOffset.UtcNow,
                ApprovedByTeacherId = approvedByTeacherId,
            });
            return;
        }

        membership.Role = role;
        membership.Status = ClassroomMemberStatus.Approved;
        membership.ApprovedAt ??= DateTimeOffset.UtcNow;
        membership.ApprovedByTeacherId = approvedByTeacherId;
    }

    private static async Task EnsureDeckAssignmentAsync(
        ApplicationDbContext dbContext,
        Guid classroomId,
        string teacherId,
        string deckTitle)
    {
        var deckId = await dbContext.Decks
            .Where(deck => deck.IsGlobalStock && deck.Title == deckTitle)
            .Select(deck => deck.Id)
            .SingleAsync();

        var assignment = await dbContext.DeckAssignments.SingleOrDefaultAsync(assignment =>
            assignment.ClassroomId == classroomId && assignment.DeckId == deckId);

        if (assignment is null)
        {
            dbContext.DeckAssignments.Add(new DeckAssignment
            {
                Id = Guid.NewGuid(),
                ClassroomId = classroomId,
                DeckId = deckId,
                AssignedByTeacherId = teacherId,
                IsActive = true,
                IsOpenStudyAllowed = true,
            });
            return;
        }

        assignment.AssignedByTeacherId = teacherId;
        assignment.IsActive = true;
        assignment.IsOpenStudyAllowed = true;
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(error => error.Description));

    private sealed record DemoClassSeed(string Name, string JoinCode, string[] ActiveDeckTitles);
}

using Microsoft.AspNetCore.Identity;

namespace RocketReps.Web.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public Guid? SchoolId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public School? School { get; set; }
    public ICollection<ClassroomMembership> ClassroomMemberships { get; } = [];
    public ICollection<StudentCardProgress> CardProgress { get; } = [];
    public ICollection<ReviewLog> ReviewLogs { get; } = [];
}

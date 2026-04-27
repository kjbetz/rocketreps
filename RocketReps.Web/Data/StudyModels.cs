namespace RocketReps.Web.Data;

public enum ClassroomMemberRole
{
    Student = 0,
    Teacher = 1,
}

public enum ClassroomMemberStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
}

public enum CardType
{
    Flashcard = 0,
    MultipleChoice = 1,
    MathFact = 2,
    ImagePrompt = 3,
    AudioPrompt = 4,
}

public enum ReviewRating
{
    Again = 0,
    Good = 1,
}

public sealed class School
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Mascot { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Classroom> Classrooms { get; } = [];
    public ICollection<Deck> Decks { get; } = [];
}

public sealed class Classroom
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string? CreatedByTeacherId { get; set; }
    public required string Name { get; set; }
    public required string JoinCode { get; set; }
    public bool RequiresTeacherApproval { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    public School? School { get; set; }
    public ApplicationUser? CreatedByTeacher { get; set; }
    public ICollection<ClassroomMembership> Memberships { get; } = [];
    public ICollection<DeckAssignment> Assignments { get; } = [];
}

public sealed class ClassroomMembership
{
    public Guid Id { get; set; }
    public Guid ClassroomId { get; set; }
    public required string UserId { get; set; }
    public ClassroomMemberRole Role { get; set; }
    public ClassroomMemberStatus Status { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? ApprovedByTeacherId { get; set; }

    public Classroom? Classroom { get; set; }
    public ApplicationUser? User { get; set; }
    public ApplicationUser? ApprovedByTeacher { get; set; }
}

public sealed class Deck
{
    public Guid Id { get; set; }
    public Guid? SchoolId { get; set; }
    public string? OwnerTeacherId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Subject { get; set; }
    public required string GradeBand { get; set; }
    public bool IsGlobalStock { get; set; }
    public bool IsPublished { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public School? School { get; set; }
    public ApplicationUser? OwnerTeacher { get; set; }
    public ICollection<Card> Cards { get; } = [];
    public ICollection<DeckAssignment> Assignments { get; } = [];
}

public sealed class Card
{
    public Guid Id { get; set; }
    public Guid DeckId { get; set; }
    public required string Front { get; set; }
    public required string Back { get; set; }
    public CardType CardType { get; set; }
    public int SortOrder { get; set; }
    public string? ChoicesJson { get; set; }
    public string? CorrectAnswer { get; set; }

    public Deck? Deck { get; set; }
    public ICollection<StudentCardProgress> StudentProgress { get; } = [];
    public ICollection<ReviewLog> Reviews { get; } = [];
}

public sealed class DeckAssignment
{
    public Guid Id { get; set; }
    public Guid ClassroomId { get; set; }
    public Guid DeckId { get; set; }
    public required string AssignedByTeacherId { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public bool IsOpenStudyAllowed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Classroom? Classroom { get; set; }
    public Deck? Deck { get; set; }
    public ApplicationUser? AssignedByTeacher { get; set; }
    public ICollection<ReviewLog> Reviews { get; } = [];
}

public sealed class StudentCardProgress
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public Guid CardId { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public int ReviewCount { get; set; }
    public int LapseCount { get; set; }
    public double? Stability { get; set; }
    public double? Difficulty { get; set; }
    public ReviewRating? LastRating { get; set; }
    public DateTimeOffset? LastReviewedAt { get; set; }

    public ApplicationUser? User { get; set; }
    public Card? Card { get; set; }
}

public sealed class ReviewLog
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public Guid CardId { get; set; }
    public Guid? DeckAssignmentId { get; set; }
    public DateTimeOffset ReviewedAt { get; set; }
    public bool WasCorrect { get; set; }
    public ReviewRating Rating { get; set; }
    public TimeSpan? ScheduledInterval { get; set; }
    public DateTimeOffset NextDueAt { get; set; }

    public ApplicationUser? User { get; set; }
    public Card? Card { get; set; }
    public DeckAssignment? DeckAssignment { get; set; }
}

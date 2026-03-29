using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Trampline.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recommendations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AuthorRole = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Lvl = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    TotpSecret = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsTotpEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSuperAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Activity = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: true),
                    Socials = table.Column<List<string>>(type: "text[]", nullable: false),
                    Photos = table.Column<List<string>>(type: "text[]", nullable: false),
                    Videos = table.Column<List<string>>(type: "text[]", nullable: false),
                    VerificationLevel = table.Column<int>(type: "integer", nullable: false),
                    VerifiedName = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    inn = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employees_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    agent = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Patronymic = table.Column<string>(type: "text", nullable: false),
                    About = table.Column<string>(type: "text", nullable: true),
                    Photo = table.Column<string>(type: "text", nullable: true),
                    Resume = table.Column<string>(type: "text", nullable: true),
                    Skills = table.Column<List<string>>(type: "text[]", nullable: false),
                    Repos = table.Column<List<string>>(type: "text[]", nullable: false),
                    admission = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    course = table.Column<int>(type: "integer", nullable: true),
                    graduation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    university = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    GeoLon = table.Column<double>(type: "double precision", nullable: false),
                    GeoLat = table.Column<double>(type: "double precision", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Format = table.Column<string>(type: "text", nullable: false),
                    SalaryFrom = table.Column<decimal>(type: "numeric", nullable: true),
                    SalaryTo = table.Column<decimal>(type: "numeric", nullable: true),
                    Photos = table.Column<List<string>>(type: "text[]", nullable: false),
                    Videos = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_events_employees_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    GeoLon = table.Column<double>(type: "double precision", nullable: false),
                    GeoLat = table.Column<double>(type: "double precision", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Photos = table.Column<List<string>>(type: "text[]", nullable: false),
                    Videos = table.Column<List<string>>(type: "text[]", nullable: false),
                    Format = table.Column<string>(type: "text", nullable: false),
                    SalaryFrom = table.Column<decimal>(type: "numeric", nullable: true),
                    SalaryTo = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_jobs_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mentorships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    GeoLon = table.Column<double>(type: "double precision", nullable: false),
                    GeoLat = table.Column<double>(type: "double precision", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: true),
                    Duration = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Format = table.Column<string>(type: "text", nullable: false),
                    SalaryFrom = table.Column<decimal>(type: "numeric", nullable: true),
                    SalaryTo = table.Column<decimal>(type: "numeric", nullable: true),
                    Photos = table.Column<List<string>>(type: "text[]", nullable: false),
                    Videos = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentorships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mentorships_employees_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eventApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CoverLetter = table.Column<string>(type: "text", nullable: true),
                    IsReadByEmployer = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_eventApplications_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventApplications_workers_WorkerProfileId",
                        column: x => x.WorkerProfileId,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventTag",
                columns: table => new
                {
                    EventsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTag", x => new { x.EventsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_EventTag_events_EventsId",
                        column: x => x.EventsId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTag_tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "jobApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CoverLetter = table.Column<string>(type: "text", nullable: true),
                    IsReadByEmployer = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_jobApplications_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_jobApplications_workers_WorkerProfileId",
                        column: x => x.WorkerProfileId,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobTag",
                columns: table => new
                {
                    JobsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTag", x => new { x.JobsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_JobTag_jobs_JobsId",
                        column: x => x.JobsId,
                        principalTable: "jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobTag_tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mentorshipApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CoverLetter = table.Column<string>(type: "text", nullable: true),
                    IsReadByEmployer = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentorshipApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mentorshipApplications_mentorships_MentorshipId",
                        column: x => x.MentorshipId,
                        principalTable: "mentorships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mentorshipApplications_workers_WorkerProfileId",
                        column: x => x.WorkerProfileId,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MentorshipTag",
                columns: table => new
                {
                    MentorshipsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipTag", x => new { x.MentorshipsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_MentorshipTag_mentorships_MentorshipsId",
                        column: x => x.MentorshipsId,
                        principalTable: "mentorships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorshipTag_tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "tags",
                columns: new[] { "Id", "Category", "Lvl", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "tech", 0, "C#" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "tech", 0, "Java" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "tech", 0, "Python" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "tech", 0, "JavaScript" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "tech", 0, "TypeScript" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "tech", 0, "Go" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "tech", 0, "Rust" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "tech", 0, "C++" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "tech", 0, "PHP" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "tech", 0, "Kotlin" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "tech", 0, "Swift" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "tech", 0, "Ruby" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "tech", 0, "Dart" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "tech", 0, "Scala" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "tech", 0, "SQL" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "tech", 0, ".NET" },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "tech", 0, "Spring" },
                    { new Guid("00000000-0000-0000-0000-000000000018"), "tech", 0, "React" },
                    { new Guid("00000000-0000-0000-0000-000000000019"), "tech", 0, "Vue.js" },
                    { new Guid("00000000-0000-0000-0000-000000000020"), "tech", 0, "Svelte" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "tech", 0, "Angular" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "tech", 0, "Node.js" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "tech", 0, "Django" },
                    { new Guid("00000000-0000-0000-0000-000000000024"), "tech", 0, "FastAPI" },
                    { new Guid("00000000-0000-0000-0000-000000000025"), "tech", 0, "Flutter" },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "tech", 0, "Docker" },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "tech", 0, "Kubernetes" },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "tech", 0, "PostgreSQL" },
                    { new Guid("00000000-0000-0000-0000-000000000029"), "tech", 0, "MongoDB" },
                    { new Guid("00000000-0000-0000-0000-000000000030"), "tech", 0, "Redis" },
                    { new Guid("00000000-0000-0000-0000-000000000031"), "tech", 0, "Git" },
                    { new Guid("00000000-0000-0000-0000-000000000032"), "tech", 0, "Linux" },
                    { new Guid("00000000-0000-0000-0000-000000000033"), "tech", 0, "CI/CD" },
                    { new Guid("00000000-0000-0000-0000-000000000034"), "tech", 0, "REST API" },
                    { new Guid("00000000-0000-0000-0000-000000000035"), "tech", 0, "GraphQL" },
                    { new Guid("00000000-0000-0000-0000-000000000036"), "tech", 0, "gRPC" },
                    { new Guid("00000000-0000-0000-0000-000000000037"), "tech", 0, "RabbitMQ" },
                    { new Guid("00000000-0000-0000-0000-000000000038"), "tech", 0, "Kafka" },
                    { new Guid("00000000-0000-0000-0000-000000000039"), "tech", 0, "Elasticsearch" },
                    { new Guid("00000000-0000-0000-0000-000000000040"), "tech", 0, "AWS" },
                    { new Guid("00000000-0000-0000-0000-000000000041"), "tech", 0, "Azure" },
                    { new Guid("00000000-0000-0000-0000-000000000042"), "tech", 0, "Nginx" },
                    { new Guid("00000000-0000-0000-0000-000000000043"), "tech", 0, "Backend" },
                    { new Guid("00000000-0000-0000-0000-000000000044"), "tech", 0, "Frontend" },
                    { new Guid("00000000-0000-0000-0000-000000000045"), "tech", 0, "Fullstack" },
                    { new Guid("00000000-0000-0000-0000-000000000046"), "tech", 0, "Mobile" },
                    { new Guid("00000000-0000-0000-0000-000000000047"), "tech", 0, "DevOps" },
                    { new Guid("00000000-0000-0000-0000-000000000048"), "tech", 0, "Data Science" },
                    { new Guid("00000000-0000-0000-0000-000000000049"), "tech", 0, "Machine Learning" },
                    { new Guid("00000000-0000-0000-0000-000000000050"), "tech", 0, "QA" },
                    { new Guid("00000000-0000-0000-0000-000000000051"), "tech", 0, "GameDev" },
                    { new Guid("00000000-0000-0000-0000-000000000052"), "tech", 0, "Embedded" },
                    { new Guid("00000000-0000-0000-0000-000000000053"), "tech", 0, "Blockchain" },
                    { new Guid("00000000-0000-0000-0000-000000000054"), "tech", 0, "Cybersecurity" },
                    { new Guid("00000000-0000-0000-0000-000000000055"), "tech", 0, "1С" },
                    { new Guid("00000000-0000-0000-0000-000000000056"), "design", 0, "UI/UX" },
                    { new Guid("00000000-0000-0000-0000-000000000057"), "design", 0, "Web-дизайн" },
                    { new Guid("00000000-0000-0000-0000-000000000058"), "design", 0, "Графический дизайн" },
                    { new Guid("00000000-0000-0000-0000-000000000059"), "design", 0, "Figma" },
                    { new Guid("00000000-0000-0000-0000-000000000060"), "design", 0, "Adobe Photoshop" },
                    { new Guid("00000000-0000-0000-0000-000000000061"), "design", 0, "Adobe Illustrator" },
                    { new Guid("00000000-0000-0000-0000-000000000062"), "design", 0, "Motion-дизайн" },
                    { new Guid("00000000-0000-0000-0000-000000000063"), "design", 0, "3D-моделирование" },
                    { new Guid("00000000-0000-0000-0000-000000000064"), "design", 0, "Прототипирование" },
                    { new Guid("00000000-0000-0000-0000-000000000065"), "design", 0, "Брендинг" },
                    { new Guid("00000000-0000-0000-0000-000000000066"), "marketing", 0, "SMM" },
                    { new Guid("00000000-0000-0000-0000-000000000067"), "marketing", 0, "SEO" },
                    { new Guid("00000000-0000-0000-0000-000000000068"), "marketing", 0, "Контент-маркетинг" },
                    { new Guid("00000000-0000-0000-0000-000000000069"), "marketing", 0, "Email-маркетинг" },
                    { new Guid("00000000-0000-0000-0000-000000000070"), "marketing", 0, "Таргетированная реклама" },
                    { new Guid("00000000-0000-0000-0000-000000000071"), "marketing", 0, "Контекстная реклама" },
                    { new Guid("00000000-0000-0000-0000-000000000072"), "marketing", 0, "Копирайтинг" },
                    { new Guid("00000000-0000-0000-0000-000000000073"), "marketing", 0, "PR" },
                    { new Guid("00000000-0000-0000-0000-000000000074"), "marketing", 0, "Аналитика" },
                    { new Guid("00000000-0000-0000-0000-000000000075"), "marketing", 0, "Яндекс.Метрика" },
                    { new Guid("00000000-0000-0000-0000-000000000076"), "management", 0, "Управление проектами" },
                    { new Guid("00000000-0000-0000-0000-000000000077"), "management", 0, "Agile" },
                    { new Guid("00000000-0000-0000-0000-000000000078"), "management", 0, "Scrum" },
                    { new Guid("00000000-0000-0000-0000-000000000079"), "management", 0, "Product Management" },
                    { new Guid("00000000-0000-0000-0000-000000000080"), "management", 0, "Бизнес-анализ" },
                    { new Guid("00000000-0000-0000-0000-000000000081"), "management", 0, "Jira" },
                    { new Guid("00000000-0000-0000-0000-000000000082"), "management", 0, "Управление командой" },
                    { new Guid("00000000-0000-0000-0000-000000000083"), "management", 0, "Стратегическое планирование" },
                    { new Guid("00000000-0000-0000-0000-000000000084"), "finance", 0, "Бухгалтерия" },
                    { new Guid("00000000-0000-0000-0000-000000000085"), "finance", 0, "Финансовый анализ" },
                    { new Guid("00000000-0000-0000-0000-000000000086"), "finance", 0, "Налогообложение" },
                    { new Guid("00000000-0000-0000-0000-000000000087"), "finance", 0, "Аудит" },
                    { new Guid("00000000-0000-0000-0000-000000000088"), "finance", 0, "1С:Бухгалтерия" },
                    { new Guid("00000000-0000-0000-0000-000000000089"), "finance", 0, "Excel" },
                    { new Guid("00000000-0000-0000-0000-000000000090"), "legal", 0, "Юриспруденция" },
                    { new Guid("00000000-0000-0000-0000-000000000091"), "legal", 0, "Трудовое право" },
                    { new Guid("00000000-0000-0000-0000-000000000092"), "legal", 0, "Договорная работа" },
                    { new Guid("00000000-0000-0000-0000-000000000093"), "education", 0, "Преподавание" },
                    { new Guid("00000000-0000-0000-0000-000000000094"), "education", 0, "Репетиторство" },
                    { new Guid("00000000-0000-0000-0000-000000000095"), "education", 0, "Разработка курсов" },
                    { new Guid("00000000-0000-0000-0000-000000000096"), "education", 0, "Менторство" },
                    { new Guid("00000000-0000-0000-0000-000000000097"), "education", 0, "Научные исследования" },
                    { new Guid("00000000-0000-0000-0000-000000000098"), "event", 0, "Хакатон" },
                    { new Guid("00000000-0000-0000-0000-000000000099"), "event", 0, "Конференция" },
                    { new Guid("00000000-0000-0000-0000-000000000100"), "event", 0, "Митап" },
                    { new Guid("00000000-0000-0000-0000-000000000101"), "event", 0, "Воркшоп" },
                    { new Guid("00000000-0000-0000-0000-000000000102"), "event", 0, "Вебинар" },
                    { new Guid("00000000-0000-0000-0000-000000000103"), "event", 0, "Стажировка" },
                    { new Guid("00000000-0000-0000-0000-000000000104"), "event", 0, "Олимпиада" },
                    { new Guid("00000000-0000-0000-0000-000000000105"), "event", 0, "Карьерный день" },
                    { new Guid("00000000-0000-0000-0000-000000000106"), "event", 0, "Нетворкинг" },
                    { new Guid("00000000-0000-0000-0000-000000000107"), "event", 0, "Мастер-класс" },
                    { new Guid("00000000-0000-0000-0000-000000000108"), "event", 0, "Лекция" },
                    { new Guid("00000000-0000-0000-0000-000000000109"), "event", 0, "Конкурс" },
                    { new Guid("00000000-0000-0000-0000-000000000110"), "event", 0, "Выставка" },
                    { new Guid("00000000-0000-0000-0000-000000000111"), "event", 0, "Форум" },
                    { new Guid("00000000-0000-0000-0000-000000000112"), "event", 0, "День открытых дверей" },
                    { new Guid("00000000-0000-0000-0000-000000000113"), "event", 0, "Буткемп" },
                    { new Guid("00000000-0000-0000-0000-000000000114"), "soft", 0, "Коммуникации" },
                    { new Guid("00000000-0000-0000-0000-000000000115"), "soft", 0, "Работа в команде" },
                    { new Guid("00000000-0000-0000-0000-000000000116"), "soft", 0, "Лидерство" },
                    { new Guid("00000000-0000-0000-0000-000000000117"), "soft", 0, "Тайм-менеджмент" },
                    { new Guid("00000000-0000-0000-0000-000000000118"), "soft", 0, "Критическое мышление" },
                    { new Guid("00000000-0000-0000-0000-000000000119"), "soft", 0, "Презентации" },
                    { new Guid("00000000-0000-0000-0000-000000000120"), "soft", 0, "Переговоры" },
                    { new Guid("00000000-0000-0000-0000-000000000121"), "soft", 0, "Английский язык" },
                    { new Guid("00000000-0000-0000-0000-000000000122"), "employment", 0, "Полная занятость" },
                    { new Guid("00000000-0000-0000-0000-000000000123"), "employment", 0, "Частичная занятость" },
                    { new Guid("00000000-0000-0000-0000-000000000124"), "employment", 0, "Проектная работа" },
                    { new Guid("00000000-0000-0000-0000-000000000125"), "employment", 0, "Фриланс" },
                    { new Guid("00000000-0000-0000-0000-000000000126"), "employment", 0, "Временная работа" },
                    { new Guid("00000000-0000-0000-0000-000000000127"), "level", 0, "Intern" },
                    { new Guid("00000000-0000-0000-0000-000000000128"), "level", 1, "Junior" },
                    { new Guid("00000000-0000-0000-0000-000000000129"), "level", 2, "Middle" },
                    { new Guid("00000000-0000-0000-0000-000000000130"), "level", 3, "Senior" },
                    { new Guid("00000000-0000-0000-0000-000000000131"), "level", 4, "Lead" },
                    { new Guid("00000000-0000-0000-0000-000000000132"), "level", 5, "Architect" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "Avatar", "DeletedAt", "Email", "IsPrivate", "IsSuperAdmin", "Nickname", "PasswordHash", "Phone", "Role", "TotpSecret" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), null, null, "admin@gmail.com", false, true, "Администратор", "3rljPMTphm1R5ozPxwb7ig==:W5meEqj/maKtTM0RxeTVlcs61YIPpPnj6yZu3Z0Eh2o=", null, "Admin", null });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Action",
                table: "audit_logs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CreatedAt",
                table: "audit_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityType",
                table: "audit_logs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_ReceiverId",
                table: "contacts",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_RequesterId",
                table: "contacts",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_RequesterId_ReceiverId",
                table: "contacts",
                columns: new[] { "RequesterId", "ReceiverId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_UserId",
                table: "employees",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eventApplications_EventId",
                table: "eventApplications",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_eventApplications_WorkerProfileId",
                table: "eventApplications",
                column: "WorkerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_eventApplications_WorkerProfileId_EventId",
                table: "eventApplications",
                columns: new[] { "WorkerProfileId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_CreatedAt",
                table: "events",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_events_IsActive",
                table: "events",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_events_ProfileId",
                table: "events",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_events_UserId",
                table: "events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTag_TagsId",
                table: "EventTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_UserId_TargetId_Type",
                table: "favorites",
                columns: new[] { "UserId", "TargetId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_jobApplications_JobId",
                table: "jobApplications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_jobApplications_WorkerProfileId_JobId",
                table: "jobApplications",
                columns: new[] { "WorkerProfileId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_jobs_CreatedAt",
                table: "jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_EmployeeId",
                table: "jobs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_IsActive",
                table: "jobs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_UserId",
                table: "jobs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTag_TagsId",
                table: "JobTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_mentorshipApplications_MentorshipId",
                table: "mentorshipApplications",
                column: "MentorshipId");

            migrationBuilder.CreateIndex(
                name: "IX_mentorshipApplications_WorkerProfileId",
                table: "mentorshipApplications",
                column: "WorkerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_mentorshipApplications_WorkerProfileId_MentorshipId",
                table: "mentorshipApplications",
                columns: new[] { "WorkerProfileId", "MentorshipId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mentorships_CreatedAt",
                table: "mentorships",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_mentorships_IsActive",
                table: "mentorships",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_mentorships_ProfileId",
                table: "mentorships",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_mentorships_UserId",
                table: "mentorships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipTag_TagsId",
                table: "MentorshipTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId",
                table: "notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId_IsRead",
                table: "notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_UserId",
                table: "reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserId",
                table: "sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_Name_Category_Lvl",
                table: "tags",
                columns: new[] { "Name", "Category", "Lvl" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Phone",
                table: "users",
                column: "Phone",
                unique: true,
                filter: "\"Phone\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_workers_UserId",
                table: "workers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "contacts");

            migrationBuilder.DropTable(
                name: "eventApplications");

            migrationBuilder.DropTable(
                name: "EventTag");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "jobApplications");

            migrationBuilder.DropTable(
                name: "JobTag");

            migrationBuilder.DropTable(
                name: "mentorshipApplications");

            migrationBuilder.DropTable(
                name: "MentorshipTag");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "recommendations");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "workers");

            migrationBuilder.DropTable(
                name: "mentorships");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trampline.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameJoinTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventTag_events_EventsId",
                table: "EventTag");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTag_tags_TagsId",
                table: "EventTag");

            migrationBuilder.DropForeignKey(
                name: "FK_JobTag_jobs_JobsId",
                table: "JobTag");

            migrationBuilder.DropForeignKey(
                name: "FK_JobTag_tags_TagsId",
                table: "JobTag");

            migrationBuilder.DropForeignKey(
                name: "FK_MentorshipTag_mentorships_MentorshipsId",
                table: "MentorshipTag");

            migrationBuilder.DropForeignKey(
                name: "FK_MentorshipTag_tags_TagsId",
                table: "MentorshipTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MentorshipTag",
                table: "MentorshipTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobTag",
                table: "JobTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventTag",
                table: "EventTag");

            migrationBuilder.RenameTable(
                name: "MentorshipTag",
                newName: "mentorshipTags");

            migrationBuilder.RenameTable(
                name: "JobTag",
                newName: "jobTags");

            migrationBuilder.RenameTable(
                name: "EventTag",
                newName: "eventTags");

            migrationBuilder.RenameIndex(
                name: "IX_MentorshipTag_TagsId",
                table: "mentorshipTags",
                newName: "IX_mentorshipTags_TagsId");

            migrationBuilder.RenameIndex(
                name: "IX_JobTag_TagsId",
                table: "jobTags",
                newName: "IX_jobTags_TagsId");

            migrationBuilder.RenameIndex(
                name: "IX_EventTag_TagsId",
                table: "eventTags",
                newName: "IX_eventTags_TagsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mentorshipTags",
                table: "mentorshipTags",
                columns: new[] { "MentorshipsId", "TagsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_jobTags",
                table: "jobTags",
                columns: new[] { "JobsId", "TagsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_eventTags",
                table: "eventTags",
                columns: new[] { "EventsId", "TagsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_eventTags_events_EventsId",
                table: "eventTags",
                column: "EventsId",
                principalTable: "events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_eventTags_tags_TagsId",
                table: "eventTags",
                column: "TagsId",
                principalTable: "tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_jobTags_jobs_JobsId",
                table: "jobTags",
                column: "JobsId",
                principalTable: "jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_jobTags_tags_TagsId",
                table: "jobTags",
                column: "TagsId",
                principalTable: "tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mentorshipTags_mentorships_MentorshipsId",
                table: "mentorshipTags",
                column: "MentorshipsId",
                principalTable: "mentorships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mentorshipTags_tags_TagsId",
                table: "mentorshipTags",
                column: "TagsId",
                principalTable: "tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_eventTags_events_EventsId",
                table: "eventTags");

            migrationBuilder.DropForeignKey(
                name: "FK_eventTags_tags_TagsId",
                table: "eventTags");

            migrationBuilder.DropForeignKey(
                name: "FK_jobTags_jobs_JobsId",
                table: "jobTags");

            migrationBuilder.DropForeignKey(
                name: "FK_jobTags_tags_TagsId",
                table: "jobTags");

            migrationBuilder.DropForeignKey(
                name: "FK_mentorshipTags_mentorships_MentorshipsId",
                table: "mentorshipTags");

            migrationBuilder.DropForeignKey(
                name: "FK_mentorshipTags_tags_TagsId",
                table: "mentorshipTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mentorshipTags",
                table: "mentorshipTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_jobTags",
                table: "jobTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_eventTags",
                table: "eventTags");

            migrationBuilder.RenameTable(
                name: "mentorshipTags",
                newName: "MentorshipTag");

            migrationBuilder.RenameTable(
                name: "jobTags",
                newName: "JobTag");

            migrationBuilder.RenameTable(
                name: "eventTags",
                newName: "EventTag");

            migrationBuilder.RenameIndex(
                name: "IX_mentorshipTags_TagsId",
                table: "MentorshipTag",
                newName: "IX_MentorshipTag_TagsId");

            migrationBuilder.RenameIndex(
                name: "IX_jobTags_TagsId",
                table: "JobTag",
                newName: "IX_JobTag_TagsId");

            migrationBuilder.RenameIndex(
                name: "IX_eventTags_TagsId",
                table: "EventTag",
                newName: "IX_EventTag_TagsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MentorshipTag",
                table: "MentorshipTag",
                columns: new[] { "MentorshipsId", "TagsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobTag",
                table: "JobTag",
                columns: new[] { "JobsId", "TagsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventTag",
                table: "EventTag",
                columns: new[] { "EventsId", "TagsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EventTag_events_EventsId",
                table: "EventTag",
                column: "EventsId",
                principalTable: "events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTag_tags_TagsId",
                table: "EventTag",
                column: "TagsId",
                principalTable: "tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobTag_jobs_JobsId",
                table: "JobTag",
                column: "JobsId",
                principalTable: "jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobTag_tags_TagsId",
                table: "JobTag",
                column: "TagsId",
                principalTable: "tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MentorshipTag_mentorships_MentorshipsId",
                table: "MentorshipTag",
                column: "MentorshipsId",
                principalTable: "mentorships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MentorshipTag_tags_TagsId",
                table: "MentorshipTag",
                column: "TagsId",
                principalTable: "tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

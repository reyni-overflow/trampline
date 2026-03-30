using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trampline.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE INDEX ix_jobs_fts_ru ON jobs
                USING gin(to_tsvector('russian', coalesce("Title",'') || ' ' || coalesce("Description",'')));

                CREATE INDEX ix_jobs_fts_simple ON jobs
                USING gin(to_tsvector('simple', coalesce("Title",'') || ' ' || coalesce("Description",'')));

                CREATE INDEX ix_events_fts_ru ON events
                USING gin(to_tsvector('russian', coalesce("Title",'') || ' ' || coalesce("Description",'')));

                CREATE INDEX ix_events_fts_simple ON events
                USING gin(to_tsvector('simple', coalesce("Title",'') || ' ' || coalesce("Description",'')));

                CREATE INDEX ix_mentorships_fts_ru ON mentorships
                USING gin(to_tsvector('russian', coalesce("Title",'') || ' ' || coalesce("Description",'')));

                CREATE INDEX ix_mentorships_fts_simple ON mentorships
                USING gin(to_tsvector('simple', coalesce("Title",'') || ' ' || coalesce("Description",'')));
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS ix_jobs_fts_ru;
                DROP INDEX IF EXISTS ix_jobs_fts_simple;
                DROP INDEX IF EXISTS ix_events_fts_ru;
                DROP INDEX IF EXISTS ix_events_fts_simple;
                DROP INDEX IF EXISTS ix_mentorships_fts_ru;
                DROP INDEX IF EXISTS ix_mentorships_fts_simple;
            """);
        }
    }
}

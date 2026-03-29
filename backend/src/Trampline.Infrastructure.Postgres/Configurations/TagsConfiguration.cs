using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Employee;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class TagsConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasIndex(x => new { x.Name, x.Category, x.Lvl })
            .IsUnique();

        builder.ToTable("tags");

        builder.HasData(SeedTags());
    }

    private static Tag[] SeedTags()
    {
        var id = 0;
        Tag Make(string name, string category, int lvl = 0) => new()
        {
            Id = new Guid($"00000000-0000-0000-0000-{(++id):D12}"),
            Name = name,
            Category = category,
            Lvl = lvl
        };

        return
        [
            Make("C#", "tech"), Make("Java", "tech"), Make("Python", "tech"),
            Make("JavaScript", "tech"), Make("TypeScript", "tech"), Make("Go", "tech"),
            Make("Rust", "tech"), Make("C++", "tech"), Make("PHP", "tech"),
            Make("Kotlin", "tech"), Make("Swift", "tech"), Make("Ruby", "tech"),
            Make("Dart", "tech"), Make("Scala", "tech"), Make("SQL", "tech"),

            Make(".NET", "tech"), Make("Spring", "tech"), Make("React", "tech"),
            Make("Vue.js", "tech"), Make("Svelte", "tech"), Make("Angular", "tech"),
            Make("Node.js", "tech"), Make("Django", "tech"), Make("FastAPI", "tech"),
            Make("Flutter", "tech"), Make("Docker", "tech"), Make("Kubernetes", "tech"),
            Make("PostgreSQL", "tech"), Make("MongoDB", "tech"), Make("Redis", "tech"),
            Make("Git", "tech"), Make("Linux", "tech"), Make("CI/CD", "tech"),
            Make("REST API", "tech"), Make("GraphQL", "tech"), Make("gRPC", "tech"),
            Make("RabbitMQ", "tech"), Make("Kafka", "tech"), Make("Elasticsearch", "tech"),
            Make("AWS", "tech"), Make("Azure", "tech"), Make("Nginx", "tech"),

            Make("Backend", "tech"), Make("Frontend", "tech"), Make("Fullstack", "tech"),
            Make("Mobile", "tech"), Make("DevOps", "tech"), Make("Data Science", "tech"),
            Make("Machine Learning", "tech"), Make("QA", "tech"), Make("GameDev", "tech"),
            Make("Embedded", "tech"), Make("Blockchain", "tech"), Make("Cybersecurity", "tech"),
            Make("1С", "tech"),

            Make("UI/UX", "design"), Make("Web-дизайн", "design"),
            Make("Графический дизайн", "design"), Make("Figma", "design"),
            Make("Adobe Photoshop", "design"), Make("Adobe Illustrator", "design"),
            Make("Motion-дизайн", "design"), Make("3D-моделирование", "design"),
            Make("Прототипирование", "design"), Make("Брендинг", "design"),

            Make("SMM", "marketing"), Make("SEO", "marketing"),
            Make("Контент-маркетинг", "marketing"), Make("Email-маркетинг", "marketing"),
            Make("Таргетированная реклама", "marketing"), Make("Контекстная реклама", "marketing"),
            Make("Копирайтинг", "marketing"), Make("PR", "marketing"),
            Make("Аналитика", "marketing"), Make("Яндекс.Метрика", "marketing"),

            Make("Управление проектами", "management"), Make("Agile", "management"),
            Make("Scrum", "management"), Make("Product Management", "management"),
            Make("Бизнес-анализ", "management"), Make("Jira", "management"),
            Make("Управление командой", "management"), Make("Стратегическое планирование", "management"),

            Make("Бухгалтерия", "finance"), Make("Финансовый анализ", "finance"),
            Make("Налогообложение", "finance"), Make("Аудит", "finance"),
            Make("1С:Бухгалтерия", "finance"), Make("Excel", "finance"),
            Make("Юриспруденция", "legal"), Make("Трудовое право", "legal"),
            Make("Договорная работа", "legal"),

            Make("Преподавание", "education"), Make("Репетиторство", "education"),
            Make("Разработка курсов", "education"), Make("Менторство", "education"),
            Make("Научные исследования", "education"),

            Make("Хакатон", "event"), Make("Конференция", "event"),
            Make("Митап", "event"), Make("Воркшоп", "event"),
            Make("Вебинар", "event"), Make("Стажировка", "event"),
            Make("Олимпиада", "event"), Make("Карьерный день", "event"),
            Make("Нетворкинг", "event"), Make("Мастер-класс", "event"),
            Make("Лекция", "event"), Make("Конкурс", "event"),
            Make("Выставка", "event"), Make("Форум", "event"),
            Make("День открытых дверей", "event"), Make("Буткемп", "event"),

            Make("Коммуникации", "soft"), Make("Работа в команде", "soft"),
            Make("Лидерство", "soft"), Make("Тайм-менеджмент", "soft"),
            Make("Критическое мышление", "soft"), Make("Презентации", "soft"),
            Make("Переговоры", "soft"), Make("Английский язык", "soft"),

            Make("Полная занятость", "employment"), Make("Частичная занятость", "employment"),
            Make("Проектная работа", "employment"), Make("Фриланс", "employment"),
            Make("Временная работа", "employment"),

            Make("Intern", "level", 0), Make("Junior", "level", 1),
            Make("Middle", "level", 2), Make("Senior", "level", 3),
            Make("Lead", "level", 4), Make("Architect", "level", 5),
        ];
    }
}
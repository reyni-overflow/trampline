interface ProfileData {
    name: string;
    lastName: string;
    patronymic: string;
    about: string | null;
    photo: string | null;
    skills: string[];
    repos: string[];
    university: string;
    course: string;
    email: string;
}

export function exportProfileToPdf(profile: ProfileData) {
    const fullName = [profile.lastName, profile.name, profile.patronymic].filter(Boolean).join(' ');

    const skillsHtml = profile.skills.length
        ? `<div class="section">
            <h2>Навыки</h2>
            <div class="skills">${profile.skills.map((s) => `<span class="skill">${esc(s)}</span>`).join('')}</div>
        </div>`
        : '';

    const aboutHtml = profile.about
        ? `<div class="section">
            <h2>О себе</h2>
            <p>${esc(profile.about)}</p>
        </div>`
        : '';

    const eduHtml = profile.university
        ? `<div class="section">
            <h2>Образование</h2>
            <p>${esc(profile.university)}${profile.course ? `, ${esc(profile.course)} курс` : ''}</p>
        </div>`
        : '';

    const reposHtml = profile.repos.length
        ? `<div class="section">
            <h2>Репозитории</h2>
            <ul>${profile.repos.map((r) => `<li><a href="${esc(r)}">${esc(r)}</a></li>`).join('')}</ul>
        </div>`
        : '';

    const contactHtml = profile.email
        ? `<div class="section">
            <h2>Контакты</h2>
            <p>${esc(profile.email)}</p>
        </div>`
        : '';

    const html = `<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <title>Резюме — ${esc(fullName)}</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            color: #1a1a1a;
            padding: 2.5rem;
            line-height: 1.6;
            max-width: 800px;
            margin: 0 auto;
        }
        .header {
            display: flex;
            align-items: center;
            gap: 1.5rem;
            padding-bottom: 1.5rem;
            border-bottom: 2px solid #e5e5e5;
            margin-bottom: 1.5rem;
        }
        .header img {
            width: 90px;
            height: 90px;
            border-radius: 50%;
            object-fit: cover;
        }
        .header h1 {
            font-size: 1.75rem;
            font-weight: 700;
        }
        .section {
            margin-bottom: 1.25rem;
        }
        .section h2 {
            font-size: 1rem;
            font-weight: 600;
            color: #555;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            margin-bottom: 0.5rem;
            padding-bottom: 0.25rem;
            border-bottom: 1px solid #eee;
        }
        .section p {
            font-size: 0.95rem;
            white-space: pre-wrap;
        }
        .skills {
            display: flex;
            flex-wrap: wrap;
            gap: 0.4rem;
        }
        .skill {
            display: inline-block;
            padding: 0.2rem 0.65rem;
            background: #f0f0f0;
            border-radius: 4px;
            font-size: 0.85rem;
        }
        ul {
            padding-left: 1.25rem;
        }
        li {
            font-size: 0.95rem;
            margin-bottom: 0.2rem;
        }
        a { color: #2563eb; text-decoration: none; }
        a:hover { text-decoration: underline; }
        @media print {
            body { padding: 1rem; }
            a { color: #1a1a1a; }
            a::after { content: " (" attr(href) ")"; font-size: 0.8rem; color: #666; }
        }
    </style>
</head>
<body>
    <div class="header">
        ${profile.photo ? `<img src="${esc(profile.photo)}" alt="">` : ''}
        <h1>${esc(fullName)}</h1>
    </div>
    ${aboutHtml}
    ${skillsHtml}
    ${eduHtml}
    ${reposHtml}
    ${contactHtml}
    <script>window.onload = function() { window.print(); }</script>
</body>
</html>`;

    const w = window.open('', '_blank');
    if (w) {
        w.document.write(html);
        w.document.close();
    }
}

function esc(str: string): string {
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

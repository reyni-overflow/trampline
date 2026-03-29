namespace Trampline.Core.Models;

public enum ApplicationStatus
{
    Pending,     // Ждёт рассмотрения
    Viewed,      // Просмотрен работодателем
    Rejected,    // Отклонён
    Invited,     // Приглашён на собеседование
    InProgress,  // Собеседование
    Hired,       // Нанят
    Withdrawn    // Отозван соискателем
}
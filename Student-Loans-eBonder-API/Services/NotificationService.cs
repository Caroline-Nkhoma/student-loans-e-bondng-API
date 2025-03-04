using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentLoanseBonderAPI.DTOs;
using StudentLoanseBonderAPI.Entities;

namespace StudentLoanseBonderAPI.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public NotificationService(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    // Fetch all notifications
    public async Task<List<NotificationReadDTO>> FindAll()
    {
        var notifications = _dbContext.Notifications.ToList();

        // Convert entities to DTOs
        return notifications.Select(n => new NotificationReadDTO
        {
            Id = n.Id,
            Title = n.Title,
            Description = n.Description,
            CreatedAt = n.CreatedAt,
            IsRead = n.IsRead
        }).ToList();
    }

    public async Task<NotificationReadDTO?> FindOne(int id)
    {
        var notification = _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id);

        if (notification == null)
        {
            return null;
        }

        return _mapper.Map<NotificationReadDTO>(notification);
    }

    // Create a new notification
    public async Task<bool> Create(NotificationCreateDTO dto)
    {
        var newNotification = new Notification
        {
            Title = dto.Title,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await _dbContext.Notifications.AddAsync(newNotification);

        // Return the created notification as a DTO
        return true;
    }

    // Delete a notification
    public async Task<bool> DeleteNotificationAsync(int id)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id);

        if (notification == null) return false;

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}



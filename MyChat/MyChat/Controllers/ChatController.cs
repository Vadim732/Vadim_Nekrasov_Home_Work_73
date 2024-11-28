using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;

namespace MyChat.Controllers;

public class ChatController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly MyChatContext _context;

    public ChatController(UserManager<User> userManager, MyChatContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public IActionResult Index()
    {
        var messages = _context.Messages
            .Include(m => m.User)
            .OrderByDescending(m => m.DateOfDispatch)
            .Take(30)
            .ToList()
            .OrderBy(m => m.DateOfDispatch) 
            .ToList();

        return View(messages);
    }


    [HttpPost]
    public async Task<IActionResult> Create(Message message)
    {
        if (string.IsNullOrWhiteSpace(message.Inscription))
        {
            return BadRequest(new { error = "Сообщение не может быть пустым." });
        }

        var creator = await _userManager.GetUserAsync(User);
        if (creator == null)
        {
            return Unauthorized(new { error = "Не удалось определить пользователя." });
        }

        message.UserId = creator.Id;
        message.DateOfDispatch = DateTime.UtcNow;
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Json(new
        {
            avatar = creator.Avatar,
            dateOfDispatch = message.DateOfDispatch.ToString("dd.MM.yyyy HH:mm:ss"),
            userName = creator.UserName,
            userid = creator.Id,
            inscription = message.Inscription
        });
    }

    [HttpGet]
    public IActionResult GetLatestMessages(DateTime lastMessageTime)
    {
        var messages = _context.Messages
            .Where(m => m.DateOfDispatch > lastMessageTime)
            .Include(m => m.User)
            .OrderBy(m => m.DateOfDispatch)
            .Take(30)
            .ToList();

        var response = messages.Select(m => new
        {
            dateOfDispatch = m.DateOfDispatch.ToString("dd.MM.yyyy HH:mm:ss"),
            userName = m.User.UserName,
            inscription = m.Inscription
        });

        return Json(response);
    }

}
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
        List<Message> messages = _context.Messages.Include(m => m.User).ToList();
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
            dateOfDispatch = message.DateOfDispatch.ToString("yyyy-MM-dd HH:mm:ss"),
            userName = creator.UserName,
            inscription = message.Inscription
        });
    }
}
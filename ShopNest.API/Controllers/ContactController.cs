using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.API.DTOs;
using ShopNest.API.Repositories;
using ShopNest.API.Services;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactController : ControllerBase
{
    private readonly EmailService _emailService;
    private readonly UserRepository _userRepository;

    public ContactController(
        EmailService emailService,
        UserRepository userRepository)
    {
        _emailService = emailService;
        _userRepository = userRepository;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ContactDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Subject) ||
                string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { message = "Subject and message are required." });

            // Get user details
            var user = await _userRepository.GetByIdAsync(GetUserId());
            if (user == null)
                return NotFound(new { message = "User not found." });

            await _emailService.SendContactEmailAsync(
                user.Email,
                $"{user.FirstName} {user.LastName}",
                dto);

            return Ok(new { message = "Message sent successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
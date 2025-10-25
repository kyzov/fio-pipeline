using FIOpipeline.ApiService.Models;
using FIOpipeline.Core.Providers;
using FIOpipeline.Domain;
using FIOpipeline.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

namespace FIOpipeline.ApiService.Controllers;

[ApiController]
[Route("api/person")]
public class PersonController : ControllerBase
{
    private readonly IPersonProvider _personProvider;

    public PersonController(IPersonProvider validator)
    {
        _personProvider = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PersonDto dto)
    {
        if (dto == null)
            return BadRequest("DTO is required");

        // Преобразование string в enum
        if (!Enum.TryParse<Sex>(dto.Sex, out var sex))
            return BadRequest("Invalid sex value");

        var domainPerson = new Person
        {
            LastName = dto.LastName,
            FirstName = dto.FirstName,
            SecondName = dto.SecondName,
            BirthdayDate = dto.BirthdayDate,
            Sex = sex,
            Address = new Address { Value = dto.Address.Value },
            Phone = new Phone { Value = dto.Phone.Value },
            Email = new Email { Value = dto.Email.Value }
        };

        var (success, errors) = await _personProvider.ValidatePerson(domainPerson);

        if (!success)
            return BadRequest(new
            {
                Message = "Ошибки валидации",
                Errors = errors
            });

        return Ok(new { Message = "Пользователь успешно создан и сохранен в БД" });
    }
}


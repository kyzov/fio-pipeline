using FIOpipeline.ApiService.Models;
using FIOpipeline.Core.Providers;
using FIOpipeline.Domain;
using FIOpipeline.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

namespace FIOpipeline.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly IPersonProvider _personProvider;

    public PersonController(IPersonProvider validator)
    {
        _personProvider = validator;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] PersonDto dto)
    {
        if (dto == null)
            return BadRequest("DTO is required");

        if (!Enum.TryParse<Sex>(dto.Sex, out var sex))
            return BadRequest("Invalid sex value");

        var domainPerson = new Person
        {
            LastName = dto.LastName,
            FirstName = dto.FirstName,
            SecondName = dto.SecondName,
            BirthdayDate = dto.BirthdayDate,
            Sex = sex,
            Addresses = dto.Addresses.Select(a => new Address { Value = a.Value }).ToList(),
            Phones = dto.Phones.Select(p => new Phone { Value = p.Value }).ToList(),
            Emails = dto.Emails.Select(e => new Email { Value = e.Value }).ToList()
        };

        var (success, errors, personId) = await _personProvider.ValidatePerson(domainPerson);

        if (!success)
        {
            return BadRequest(new
            {
                Message = errors.Any(e => e.Contains("дубликат")) ?
                         "Найдены возможные дубликаты" : "Ошибки валидации",
                Errors = errors
            });
        }

        return Ok(new
        {
            Message = errors,
            PersonId = personId
        });
    }
}
using FIOpipeline.ApiService.Models;
using FIOpipeline.Domain;
using FIOpipeline.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

namespace FIOpipeline.ApiService.Controllers;

[ApiController]
[Route("api/person")]
public class PersonController : ControllerBase
{
    private readonly IPersonProvider _validator;

    public PersonController(IPersonProvider validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public IActionResult Create([FromBody] PersonDto dto)
    {
        //todo: parse
        var domainPerson = new Person
        {
            LastName = dto.LastName,
            FirstName = dto.FirstName,
            SecondName = dto.SecondName,
            BirthdayDate = dto.BirthdayDate,
            Sex = dto.Sex,
            Address = new Address { Value = dto.Address.Value },
            Phone = new Phone { Value = dto.Phone.Value },
            Email = new Email { Value = dto.Email.Value }
        };

        var errors = _validator.Validate(domainPerson);
        if (errors.Any())
            return BadRequest(errors);

        return Ok();
    }
}


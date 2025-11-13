using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportSystem.Domain.Entities;
using SupportSystem.Infrastructure.Data;

namespace SupportSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly SupportSystemDbContext _context;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(SupportSystemDbContext context, ILogger<CustomersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        var customers = await _context.Customers.ToListAsync();
        return Ok(customers);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            CreatedAt = DateTime.UtcNow,
            DataProcessingConsent = request.DataProcessingConsent,
            ConsentDate = request.DataProcessingConsent ? DateTime.UtcNow : null
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    /// <summary>
    /// Request data deletion (LGPD compliance)
    /// </summary>
    [HttpPost("{id}/request-deletion")]
    public async Task<ActionResult> RequestDataDeletion(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound();

        customer.DataDeletionRequested = true;
        customer.DataDeletionRequestDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Data deletion request recorded" });
    }
}

public class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public bool DataProcessingConsent { get; set; }
}

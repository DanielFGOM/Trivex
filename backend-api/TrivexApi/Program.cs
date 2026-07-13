using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<TrivexDbContext>(options =>
    options.UseSqlite("Data Source=trivex.db"));

// Permitir que el frontend (React) se pueda comunicar con esta API
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("PermitirFrontend");
app.UseHttpsRedirection();

// GET: obtener todos los tickets
app.MapGet("/api/tickets", async (TrivexDbContext db) =>
{
    return await db.Tickets.ToListAsync();
})
.WithName("GetTickets");

// GET: obtener un ticket por su id
app.MapGet("/api/tickets/{id}", async (int id, TrivexDbContext db) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    return ticket is not null ? Results.Ok(ticket) : Results.NotFound();
})
.WithName("GetTicketById");

// POST: crear un nuevo ticket
app.MapPost("/api/tickets", async (Ticket nuevoTicket, TrivexDbContext db) =>
{
    db.Tickets.Add(nuevoTicket);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tickets/{nuevoTicket.Id}", nuevoTicket);
})
.WithName("CreateTicket");

// DELETE: borrar un ticket
app.MapDelete("/api/tickets/{id}", async (int id, TrivexDbContext db) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket is null) return Results.NotFound();
    db.Tickets.Remove(ticket);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteTicket");

app.Run();

// Este es el "molde" de datos de un Ticket
public class Ticket
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Urgencia { get; set; } = "";
    public string Estado { get; set; } = "";
}
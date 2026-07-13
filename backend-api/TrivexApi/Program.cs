using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<TrivexDbContext>(options =>
    options.UseSqlite("Data Source=trivex.db"));
    builder.Services.AddHttpClient();

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
// POST: crear un nuevo ticket (ahora con clasificación automática por IA)
app.MapPost("/api/tickets", async (Ticket nuevoTicket, TrivexDbContext db, IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();

    try
    {
        // Le preguntamos al microservicio de Python cómo clasificar este ticket
        var solicitud = new { descripcion = nuevoTicket.Descripcion };

        var respuestaIA = await httpClient.PostAsJsonAsync(
            "http://127.0.0.1:8000/analizar",
            solicitud
        );

        if (respuestaIA.IsSuccessStatusCode)
        {
            var resultado = await respuestaIA.Content.ReadFromJsonAsync<ClasificacionIA>();

            if (resultado is not null)
            {
                nuevoTicket.Categoria = resultado.Categoria;
                nuevoTicket.Urgencia = resultado.Urgencia;
            }
        }
    }
    catch (Exception ex)
    {
        // Si el microservicio de Python no responde, seguimos sin clasificar (no rompemos todo el sistema)
        Console.WriteLine($"No se pudo clasificar con IA: {ex.Message}");
    }

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
// Esta es la "forma" de la respuesta que manda el microservicio de Python
public class ClasificacionIA
{
    public string Categoria { get; set; } = "";
    public string Urgencia { get; set; } = "";
}
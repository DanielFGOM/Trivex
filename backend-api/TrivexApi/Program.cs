var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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

// Lista de tickets en memoria (por ahora, luego la cambiamos por una base de datos real)
var tickets = new List<Ticket>
{
    new Ticket(1, "Se me cobró de más este mes", "Pagos", "Alta", "Pendiente"),
    new Ticket(2, "No puedo iniciar sesión en la app", "Acceso", "Alta", "Pendiente"),
    new Ticket(3, "Sugerencia: cambiar el color del botón", "Sugerencia", "Baja", "Pendiente"),
};

// GET: obtener todos los tickets
app.MapGet("/api/tickets", () =>
{
    return tickets;
})
.WithName("GetTickets");

// GET: obtener un ticket por su id
app.MapGet("/api/tickets/{id}", (int id) =>
{
    var ticket = tickets.FirstOrDefault(t => t.Id == id);
    return ticket is not null ? Results.Ok(ticket) : Results.NotFound();
})
.WithName("GetTicketById");

// POST: crear un nuevo ticket
app.MapPost("/api/tickets", (Ticket nuevoTicket) =>
{
    tickets.Add(nuevoTicket);
    return Results.Created($"/api/tickets/{nuevoTicket.Id}", nuevoTicket);
})
.WithName("CreateTicket");

// DELETE: borrar un ticket
app.MapDelete("/api/tickets/{id}", (int id) =>
{
    var ticket = tickets.FirstOrDefault(t => t.Id == id);
    if (ticket is null) return Results.NotFound();
    tickets.Remove(ticket);
    return Results.NoContent();
})
.WithName("DeleteTicket");

app.Run();

// Este es el "molde" de datos de un Ticket
record Ticket(int Id, string Descripcion, string Categoria, string Urgencia, string Estado);
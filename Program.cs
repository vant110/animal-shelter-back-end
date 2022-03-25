using Microsoft.EntityFrameworkCore;
using vant110.AnimalShelter.Data;
using vant110.AnimalShelter.Data.Models;

var builder = WebApplication.CreateBuilder(args);
{
    string connection = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<animalshelterContext>(options => options.UseSqlServer(connection));
}
var app = builder.Build();

app.UseFileServer();

app.MapGet("/api/animals/{species?}", async (string ? species, animalshelterContext db, HttpContext context) =>
{
    byte? speciesId = species switch
    {
        "others" => 0,
        "cats" => 1,
        "dogs" => 2,
        null => null,
        _ => 255,
    };
    if (speciesId == 255) return Results.BadRequest();

    IQueryable<Animal> animals = db.Animals;
    if (speciesId != null)
    {
        animals = animals.Where(a => a.SpeciesId == speciesId);
    }
    if (context.Request.Query.ContainsKey("vaccination"))
    {
        animals = animals.Where(a => a.VaccinationStatus == true);
    }
    if (context.Request.Query.ContainsKey("sterilization"))
    {
        animals = animals.Where(a => a.SterilizationStatus == true);
    }
    if (context.Request.Query.ContainsKey("chip"))
    {
        animals = animals.Where(a => a.ChipStatus == true);
    }
    animals = (string)context.Request.Query["sorting"] switch
    {
        "younger" => animals.OrderByDescending(a => a.BirthYear),
        "older" => animals.OrderBy(a => a.BirthYear),
        "earlier" => animals.OrderBy(a => a.ArrivalDate),
        _ => animals.OrderByDescending(a => a.ArrivalDate),
    };
    return Results.Json(await animals
        .Select(a => new
        {
            a.AnimalId,
            a.Name,
            a.ImageName
        })
        .ToListAsync());
});

app.MapGet("/api/articles", async (animalshelterContext db, HttpContext context) =>
{
    const int n = 10;
    if (!int.TryParse(context.Request.Query["page"], out var page)) return Results.BadRequest();
    if (page < 1) return Results.BadRequest();

    return Results.Json(await db.Articles
        .OrderByDescending(a => a.ArticleId)
        .Select(a => new
        {
            a.ArticleId,
            a.Title,
            a.Description
        })
        .Skip((page - 1) * n)
        .Take(n)
        .ToListAsync());
});

app.MapPost("/api/animals", async (IWebHostEnvironment environment, animalshelterContext db, HttpContext context) =>
{
    var form = context.Request.Form;

    try
    {
        Animal animal = new()
        {
            Name = form["name"],
            BirthYear = short.Parse(form["birthYear"]),
            ArrivalDate = DateTime.Parse(form["arrivalDate"]),
            VaccinationStatus = form.ContainsKey("vaccinationStatus"),
            SterilizationStatus = form.ContainsKey("sterilizationStatus"),
            ChipStatus = form.ContainsKey("chipStatus"),
            ImageName = "",
            Description = form["description"],
            SpeciesId = byte.Parse(form["speciesId"])
        };
        await db.Animals.AddAsync(animal);
        await db.SaveChangesAsync();
        animal.ImageName = $"{animal.AnimalId}.jpg";
        await db.SaveChangesAsync();

        var formFile = form.Files["image"];
        if (formFile is null || formFile.Length == 0)
        {
            db.Animals.Remove(animal);
            await db.SaveChangesAsync();
            return Results.Json(new { code = -2, message = "Выберите изображение." });
        }
        var path = @$"{environment.WebRootPath}\images\animals\{animal.ImageName}";
        using Stream fileStream = new FileStream(path, FileMode.Create);
        await formFile.CopyToAsync(fileStream);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(message: $"{ex.Message}\n\t{ex.StackTrace}");
        return Results.Json(new { code = -1, message = ex.Message });
    }
    return Results.Json(new { code = 0, message = "Питомец добавлен." });
});

app.Run();

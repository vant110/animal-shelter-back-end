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
    return Results.Json((string)context.Request.Query["sorting"] switch
    {
        "younger" => await db.Animals
            .Where(a => (speciesId == null || a.SpeciesId == speciesId)
                & (!context.Request.Query.ContainsKey("vaccination") || a.VaccinationStatus)
                & (!context.Request.Query.ContainsKey("chip") || a.SterilizationStatus)
                & (!context.Request.Query.ContainsKey("sterilization") || a.ChipStatus))
            .OrderByDescending(a => a.BirthYear)
            .Select(a => new
            {
                a.AnimalId,
                a.Name,
                a.ImageName
            })
            .ToListAsync(),
        "older" => await db.Animals
            .Where(a => (speciesId == null || a.SpeciesId == speciesId)
                & (!context.Request.Query.ContainsKey("vaccination") || a.VaccinationStatus)
                & (!context.Request.Query.ContainsKey("chip") || a.SterilizationStatus)
                & (!context.Request.Query.ContainsKey("sterilization") || a.ChipStatus))
            .OrderBy(a => a.BirthYear)
            .Select(a => new
            {
                a.AnimalId,
                a.Name,
                a.ImageName
            })
            .ToListAsync(),
        "earlier" => await db.Animals
            .Where(a => (speciesId == null || a.SpeciesId == speciesId)
                & (!context.Request.Query.ContainsKey("vaccination") || a.VaccinationStatus)
                & (!context.Request.Query.ContainsKey("chip") || a.SterilizationStatus)
                & (!context.Request.Query.ContainsKey("sterilization") || a.ChipStatus))
            .OrderBy(a => a.ArrivalDate)
            .Select(a => new
            {
                a.AnimalId,
                a.Name,
                a.ImageName
            })
            .ToListAsync(),
        _ => await db.Animals
            .Where(a => (speciesId == null || a.SpeciesId == speciesId)
                & (!context.Request.Query.ContainsKey("vaccination") || a.VaccinationStatus)
                & (!context.Request.Query.ContainsKey("chip") || a.SterilizationStatus)
                & (!context.Request.Query.ContainsKey("sterilization") || a.ChipStatus))
            .OrderByDescending(a => a.ArrivalDate)
            .Select(a => new
            {
                a.AnimalId,
                a.Name,
                a.ImageName
            })
            .ToListAsync(),
    });
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
        .ToListAsync()
    );
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
            return Results.Json(new { code = -2, message = "�������� �����������." });
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
    return Results.Json(new { code = 0, message = "������� ��������." });
});

app.Run();

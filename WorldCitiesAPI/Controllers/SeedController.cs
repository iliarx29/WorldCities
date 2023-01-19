using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Runtime;
using WorldCitiesAPI.Data;
using WorldCitiesAPI.Data.Models;

namespace WorldCitiesAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public SeedController(ApplicationDbContext context, IWebHostEnvironment _env)
    {
        _context = context;
        this._env = _env;
    }

    [HttpGet("import")]
    public async Task<ActionResult> Import()
    {
        var path = Path.Combine(_env.ContentRootPath, "Data/Source/worldcities.xlsx");

        using var stream = System.IO.File.OpenRead(path);
        using var excelPackage = new ExcelPackage(stream);

        var worksheet = excelPackage.Workbook.Worksheets[0];
        var nEndRow = worksheet.Dimension.End.Row;

        var numbOfCountriesAdded = 0;
        var numbOfCitiesAdded = 0;

        var countriesByName = _context.Countries.AsNoTracking().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        for (int nRow = 2; nRow <= nEndRow; nRow++)
        {
            var row = worksheet.Cells[nRow, 1, nRow, worksheet.Dimension.End.Column];

            var countryName = row[nRow, 5].GetValue<string>();
            var iso2 = row[nRow, 6].GetValue<string>();
            var iso3 = row[nRow, 7].GetValue<string>();

            if (countriesByName.ContainsKey(countryName))
                continue;

            var country = new Country
            {
                Name = countryName,
                ISO2 = iso2,
                ISO3 = iso3
            };

            await _context.Countries.AddAsync(country);
            countriesByName.Add(countryName, country);
            numbOfCountriesAdded++;
        }

        if (numbOfCountriesAdded > 0)
            await _context.SaveChangesAsync();

        var cities = _context.Cities.AsNoTracking().ToDictionary(x => (Name: x.Name, Lat: x.Lat, Lon: x.Lon, CountryId: x.CountryId));

        for (int nRow = 2; nRow <= nEndRow; nRow++)
        {
            var row = worksheet.Cells[nRow, 1, nRow, worksheet.Dimension.End.Column];

            var name = row[nRow, 1].GetValue<string>();
            var nameAscii = row[nRow, 2].GetValue<string>();
            var lat = row[nRow, 3].GetValue<decimal>();
            var lon = row[nRow, 4].GetValue<decimal>();
            var countryName = row[nRow, 5].GetValue<string>();

            var countryId = countriesByName[countryName].Id;

            if (cities.ContainsKey((Name: name, Lat: lat, Lon: lon, CountryId: countryId)))
                continue;

            var city = new City
            {
                Name = name,
                Lat = lat,
                Lon = lon,
                CountryId = countryId
            };

            _context.Cities.Add(city);
            numbOfCitiesAdded++;
        }

        if (numbOfCitiesAdded > 0)
            await _context.SaveChangesAsync();

        return new JsonResult(new
        {
            Cities = numbOfCitiesAdded,
            Countries = numbOfCountriesAdded
        });
    }

}

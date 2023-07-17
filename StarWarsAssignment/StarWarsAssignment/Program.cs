
using System.Text.Json;
using System.Text.Json.Serialization;

//Hi Krystyna, this is my current code so far! I know i still have a long way
//to go with refactoring etc but I'm just trying to get the functionality working well first
//Any advice would be great! The issue occurs when I take the user input. It just closes the program after 1 key press
//When you debug and step into the the GetUserInput() function, it works if the input is correct

var baseAddress = "https://swapi.dev/api/";
var requestUri = "planets";

StarWarsDataApp starWarsDataApp = 
    new StarWarsDataApp(new ApiDataReader());
starWarsDataApp.Run(baseAddress, requestUri);


Console.ReadKey();


//ToDo List
//query api using link to return json of 10 planets
//print list of planets name, diam, surface water, population
//user selects property, which return max and min with planet name
//exception handling, DI, SRP
//what type should the data be organised into? structs, records etc

public class StarWarsDataApp
{
    private readonly IApiDataReader _apiDataReader;
    private readonly List<Planet> planets = new List<Planet>();

    public StarWarsDataApp(IApiDataReader apiDataReader)
    {
        _apiDataReader = apiDataReader;
    }

    public async void Run(string baseAddress, string requestUri)
    {
        var result = await _apiDataReader.Read(baseAddress, requestUri);
        var root = JsonSerializer.Deserialize<Root>(result);
        PopulatePlanets(root);
        PrintPlanetData();
        PromptUser();
        var input = GetUserInput();
        PrintResult(input);

    }

    private void PopulatePlanets(Root? root)
    {
        foreach (var result in root.results)
        {
            planets.Add(new Planet(result.name, result.diameter, 
                result.surface_water, result.population));
        }
    }

    private void PrintPlanetData() //Should be turned into a table. Use ToString() padding for proper spacing
    {
        foreach (var planet in planets)
        {
            Console.WriteLine($"Name: {planet.Name}, " +
                $"Diameter: {planet.Diameter},  Surface water: " +
                $"{planet.SurfaceWater}, Population: {planet.Population}");
        }
    }

    private void PrintResult(string input)
    {
        var orderedPlanets = planets
            .Where(planet => planet.Population != null)
            .OrderBy(planet => planet.Population); //needs to be based on specific user selection later
        var max = orderedPlanets.Last();
        var min = orderedPlanets.First();

        Print($"Max population is {max.Population} (planet: {max.Name})");
        Print($"Min population is {min.Population} (planet: {min.Name})");
    }

    private void PromptUser()
    {
        Print("Select the statistics you are interested in: ");
        Print("population" + Environment.NewLine + "diameter" 
            + Environment.NewLine + "surface water");
    }

    private void Print(string message)
    {
        Console.WriteLine(message);
    }

    private string GetUserInput()
    {
        bool shallStop = false;
        string input;
        do
        {
            input = Console.ReadLine();
            if (Validate(input))
            {
                shallStop = true;
            }
        } while (!shallStop);

        return input;
    }

    private bool Validate(string input)
    {
        switch (input.ToLower())
        {
            case "population":
            case "diameter":
            case "surface water":
                return true;
            case null:
                throw new ArgumentNullException(nameof(input));
            default:
                {
                    Print("You have given an invalid input");
                    return false;
                }
        }
    }
}

public class Planet //Most likely better as struct or record. Get() method also seems like a bad choice in constructor.
                    //Probably better to be in correct format before the class object is created
{
    public string Name { get; init; }
    public Nullable<double> Diameter { get; init; }
    public Nullable<double> SurfaceWater { get; init; }
    public Nullable<double> Population { get; init; }

    public Planet(string name, string diameter, 
        string surfaceWater, string population)
    {
        Name = name;
        Diameter = GetValue(diameter);
        SurfaceWater = GetValue(surfaceWater);
        Population = GetValue(population);
    }

    public double? GetValue(string value)
    {
        if (value == "unknown")
        {
            return null;
        }
        return double.Parse(value);
    }
}

public interface IApiDataReader
{
    Task<string> Read(string baseAddress, string requestUri);
}

public class ApiDataReader : IApiDataReader
{
    public async Task<string> Read(string baseAddress, string requestUri)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(baseAddress);
        HttpResponseMessage response = await client.GetAsync(
            requestUri);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}

public record Result(
        [property: JsonPropertyName("name")] string name,
        [property: JsonPropertyName("rotation_period")] string rotation_period,
        [property: JsonPropertyName("orbital_period")] string orbital_period,
        [property: JsonPropertyName("diameter")] string diameter,
        [property: JsonPropertyName("climate")] string climate,
        [property: JsonPropertyName("gravity")] string gravity,
        [property: JsonPropertyName("terrain")] string terrain,
        [property: JsonPropertyName("surface_water")] string surface_water,
        [property: JsonPropertyName("population")] string population,
        [property: JsonPropertyName("residents")] IReadOnlyList<string> residents,
        [property: JsonPropertyName("films")] IReadOnlyList<string> films,
        [property: JsonPropertyName("created")] DateTime created,
        [property: JsonPropertyName("edited")] DateTime edited,
        [property: JsonPropertyName("url")] string url
    );

public record Root(
    [property: JsonPropertyName("count")] int count,
    [property: JsonPropertyName("next")] string next,
    [property: JsonPropertyName("previous")] object previous,
    [property: JsonPropertyName("results")] IReadOnlyList<Result> results
);

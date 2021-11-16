using System.Text;
using System.Text.Json;

Console.WriteLine("This script will fetch all your owned repositories from Github (public AND private).");
Console.WriteLine("Press 'y' to confirm deletion. Press any other key to skip to next repo.");
Console.WriteLine("Re-run the tool if you have more than 30 repos, because of pagination.\n\n");

using (var client = new HttpClient())
{
    string username = "robinlarondelle";
    string token = ""; // <-- your personal access token here
    byte[] byteArray = Encoding.ASCII.GetBytes($"{username}:{token}");

    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    client.DefaultRequestHeaders.Add("User-Agent", "Github Repository Deletion Tool");

    var stringTask = client.GetStreamAsync("https://api.github.com/user/repos?affiliation=owner");

    try
    {
        var response = await stringTask;
        var repos = await JsonSerializer.DeserializeAsync<List<Repo>>(response);

        if (repos?.Count > 0)
        {
            foreach (var repo in repos)
            {
                Console.WriteLine($"Delete {repo.name}? [y/N]");
                var input = Console.ReadLine()?.ToLower();

                if (input == "y")
                {
                    Console.WriteLine($"Are you sure you want to delete {repo.name}? This can not be undone! [y/N]");
                    input = Console.ReadLine()?.ToLower();

                    if (input == "y")
                    {
                        Console.WriteLine($"Deleting {repo.name}");
                        var url = $"https://api.github.com/repos/{username}/{repo.name}";
                        var deleteTask = client.DeleteAsync(url);
                        var msg = await deleteTask;
                        var deleteResponse = await msg.Content.ReadAsStringAsync();
                        Console.WriteLine(deleteResponse ?? $"Deleted {repo.name}\n");
                    }
                }
            }
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine(ex.Message);
    }
}

public class Repo
{
    public string name { get; set; } // keep lower case for mapping
}
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;

namespace UOToolBox.Controllers;

[ApiController]
[Route("[controller]")] 
public class JourneyController: ControllerBase
{
    private readonly ILogger<JourneyController> _logger;
    
    public JourneyController(ILogger<JourneyController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet]
    public IEnumerable<Journey> Get()
    {
        var path = "/Users/forrrest/ClassicUOLauncher-osx-x64-release/ClassicUO/Data/Client/JournalLogs/2021_12_23_23_44_21_journal.txt";
        Console.WriteLine($"Reading file: {path}");
        var text = ReadFile(path);
        return new[] {new Journey(text)};
    }

    private string ReadFile(string path)
    {
        try
        {
            using var sr = new StreamReader(path);
            return sr.ReadToEnd();
        }
        catch (IOException e)
        {
            return "The file could not be read:\n" + e.Message; 
        }
    }
}
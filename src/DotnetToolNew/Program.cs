var name = args.First();

var currentDirectory = Environment.CurrentDirectory;

var solutionDirectory = Path.Combine(currentDirectory, name);

var projectDirectory = Path.Combine(solutionDirectory, "src", name);

Directory.CreateDirectory(solutionDirectory);

Directory.CreateDirectory(Path.Combine(solutionDirectory, "src"));

Directory.CreateDirectory(projectDirectory);

var csProjFilePath = Path.Combine(projectDirectory, $"{name}.csproj");

await DotnetAsync("new sln", solutionDirectory);

await DotnetAsync("new console", projectDirectory);

File.WriteAllText(Path.Combine(projectDirectory, "update.bat"), new StringBuilder()
    .AppendJoin(Environment.NewLine, new[] {
        $"dotnet tool uninstall -g {name}",
        "dotnet pack",
        $"dotnet tool install --global --add-source ./nupkg {name}"
    })
    .ToString());

var doc = XElement.Load(csProjFilePath);

var element = doc.FirstNode as XElement;

element.Add(new XElement("PackageOutputPath", "./nupkg"));

element.Add(new XElement("ToolCommandName", name.ToLower()));

element.Add(new XElement("PackAsTool", true));

element.Add(new XElement("Version", "1.0.0"));

doc.Save(csProjFilePath);

await DotnetAsync($"sln {name}.sln add {Path.Combine(projectDirectory, $"{name}.csproj")}", solutionDirectory);

await CommandAsync($"start {Path.Combine(solutionDirectory, name)}.sln", solutionDirectory);

async Task DotnetAsync(string arguments, string workingDirectory)
    => await StartAsync("dotnet.exe", arguments, workingDirectory);

async Task CommandAsync(string arguments, string workingDirectory)
    => await StartAsync("cmd.exe", $"/C {arguments}", workingDirectory);

async Task StartAsync(string filename, string arguments, string workingDirectory)
{
    Process process = new()
    {
        StartInfo = new()
        {
            WindowStyle = ProcessWindowStyle.Normal,
            FileName = filename,
            Arguments = arguments,
            WorkingDirectory = workingDirectory
        }
    };

    process.Start();

    await process.WaitForExitAsync();
}
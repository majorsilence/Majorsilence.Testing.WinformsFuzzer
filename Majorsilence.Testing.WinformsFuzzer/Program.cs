using Majorsilence.Testing.WinformsFuzzer;


// This console applicatin uses flaUI to launch a Winforms application and fuzz it
// It will launch the application, find all controls and interact with them randomly
// It will log all interactions to the console
// It will also take screenshots of the application and save them to a folder
// It will run for a specified amount of time and then exit
// It will also handle any exceptions that occur during the fuzzing process and log them to the console
// It will also handle any popups that may appear during the fuzzing process and close them
// It will also handle any modal dialogs that may appear during the fuzzing process and close them
// It will also handle any message boxes that may appear during the fuzzing process and close them
// It will also handle any file dialogs that may appear during the fuzzing process and close them
// It will also handle any print dialogs that may appear during the fuzzing process and close them
// It will also handle any save dialogs that may appear during the fuzzing process and close them
// It will also handle any open dialogs that may appear during the fuzzing process and close them
// It will also handle any color dialogs that may appear during the fuzzing process and close them
// It will also handle any font dialogs that may appear during the fuzzing process and close them
// It will also handle any custom dialogs that may appear during the fuzzing process and close them
// It will handle child windows and controls as well
// It will also handle MDI windows and controls and child windows as well
// It will also handle tab controls and tab pages as well
// It will also handle tree views and tree nodes as well
// and so on
// It will also handle list views and list items as well
// It will also handle any other controls that may appear during the fuzzing process and interact with them randomly
// It will also handle any other windows that may appear during the fuzzing process and interact with
// them randomly

var fuzzer = new WinformsFuzzer();

string appPath = string.Empty;
string screenshotPath = String.Empty;
TimeSpan duration = TimeSpan.Zero;
for (int i = 0; i < args.Length; i++)
{
    var arg = args[i];
    if (arg.Equals("--app", StringComparison.OrdinalIgnoreCase))
    {
        appPath = args[i + 1];
    }
    else if (arg.Equals("--screenshot", StringComparison.OrdinalIgnoreCase))
    {
        screenshotPath = args[i + 1];
    }
    else if (arg.Equals("--duration", StringComparison.OrdinalIgnoreCase))
    {
        if (int.TryParse(args[i + 1], out int minutes))
        {
            duration = TimeSpan.FromMinutes(minutes);
        }
        else
        {
            duration = TimeSpan.FromMinutes(10); // default to 10 minutes
        }
    }
}

if (string.IsNullOrEmpty(appPath))
{
    Console.WriteLine("Please provide the path to the application to fuzz using --app <path>");
    return;
}

if (string.IsNullOrEmpty(screenshotPath))
{
    Console.WriteLine("Please provide the path to the screenshot");
    return;
}

if (duration == TimeSpan.Zero)
{
    Console.WriteLine("Please provide the duration of the screenshot");
    return;
}

await fuzzer.LaunchAndFuzz(appPath, duration,
    screenshotPath);
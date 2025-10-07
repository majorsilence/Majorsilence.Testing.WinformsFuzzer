namespace Majorsilence.Testing.WinformsFuzzer;

using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System.Diagnostics;

public class WinformsFuzzer
{
    public async Task LaunchAndFuzz(string applicationPath, TimeSpan duration, string screenshotFolder)
    {
        if (!Directory.Exists(screenshotFolder))
        {
            Directory.CreateDirectory(screenshotFolder);
        }

        using var automation = new UIA3Automation();
        var app = Application.Launch(applicationPath);

        try
        {
            var mainWindow = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
            if (mainWindow == null)
            {
                throw new InvalidOperationException("Could not find main window");
            }

            var stopwatch = Stopwatch.StartNew();
            var random = new Random();
            var screenshotCounter = 0;

            while (stopwatch.Elapsed < duration)
            {
                try
                {
                    // Get all top-level windows (main + child)
                    var windows = app.GetAllTopLevelWindows(automation);

                    foreach (var window in windows)
                    {
                        // Take screenshot periodically
                        if (screenshotCounter % 10 == 0)
                        {
                            var screenshotPath =
                                Path.Combine(screenshotFolder, $"screenshot_{screenshotCounter:D4}.png");
                            window.Capture().Save(screenshotPath);
                        }

                        screenshotCounter++;

                        // Get all interactive elements
                        var interactableElements = GetInteractableElements(window);

                        if (interactableElements.Any())
                        {
                            var element = interactableElements[random.Next(interactableElements.Count)];
                            await FuzzElement(element, random);
                        }

                        // If not the main window, close after fuzzing
                        if (window.Properties.AutomationId.ValueOrDefault !=
                            mainWindow.Properties.AutomationId.ValueOrDefault)
                        {
                            window.Close();
                        }
                    }

                    // Random delay between actions
                    await Task.Delay(random.Next(100, 500));
                }
                catch (Exception ex)
                {
                    // Log exception but continue fuzzing
                    Console.WriteLine($"Fuzzing error: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
        }
        finally
        {
            app.Close();
            app.Dispose();
        }
    }

    private static Dictionary<string, List<AutomationElement>> windowElementCache = new();

    private List<AutomationElement> GetInteractableElements(Window window)
    {
        string id = window.Properties.AutomationId.ValueOrDefault ?? window.Properties.Name.ValueOrDefault ?? "main";
        if (windowElementCache.ContainsKey(id))
        {
            return windowElementCache[id];
        }

        var elements = new List<AutomationElement>();

        // Find buttons
        elements.AddRange(window.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
            .Where(e => e.IsEnabled && e.IsOffscreen == false));

        // Find text boxes
        elements.AddRange(window.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit))
            .Where(e => e.IsEnabled && e.IsOffscreen == false));

        // Find checkboxes
        elements.AddRange(window.FindAllDescendants(cf => cf.ByControlType(ControlType.CheckBox))
            .Where(e => e.IsEnabled && e.IsOffscreen == false));

        // Find combo boxes
        elements.AddRange(window.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox))
            .Where(e => e.IsEnabled && e.IsOffscreen == false));

        // Find list items
        elements.AddRange(window.FindAllDescendants(cf => cf.ByControlType(ControlType.ListItem))
            .Where(e => e.IsEnabled && e.IsOffscreen == false));

        // Find DataGridView items
        elements.AddRange(window.FindAllDescendants(cf => cf.ByControlType(ControlType.DataGrid))
            .Where(e => e.IsEnabled && e.IsOffscreen == false));

        windowElementCache[id] = elements;

        return elements;
    }

    private async Task FuzzElement(AutomationElement element, Random random)
    {
        try
        {
            switch (element.ControlType)
            {
                case ControlType.Button:
                    element.AsButton().Invoke();
                    break;

                case ControlType.Edit:
                    var textBox = element.AsTextBox();
                    var randomText = GenerateRandomText(random);
                    textBox.Text = randomText;
                    break;

                case ControlType.CheckBox:
                    element.AsCheckBox().Toggle();
                    break;

                case ControlType.ComboBox:
                    var comboBox = element.AsComboBox();
                    if (comboBox.Items.Length > 0)
                    {
                        var randomIndex = random.Next(comboBox.Items.Length);
                        comboBox.Select(randomIndex);
                    }

                    break;

                case ControlType.ListItem:
                    element.Click();
                    break;
                case ControlType.DataGrid:
                    var dataGrid = element.AsDataGridView();
                    if (dataGrid.Rows.Length > 0)
                    {
                        var randomRowIndex = random.Next(dataGrid.Rows.Length);
                        var row = dataGrid.Rows[randomRowIndex];
                        row.Click();
                        // Optionally, edit a cell if editable
                        if (row.Cells.Length > 0)
                        {
                            var randomCellIndex = random.Next(row.Cells.Length);
                            var cell = row.Cells[randomCellIndex];
                        }
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fuzzing element {element.ControlType}: {ex.Message}");
        }
    }

    private string GenerateRandomText(Random random)
    {
        // pick a random word from one of the Faker built in resources
        var rand = random.Next(1, 9);

        switch (rand)
        {
            case 1:
                return Faker.Name.FullName();
            case 2:
                return Faker.Address.StreetAddress();
            case 3:
                return Faker.Company.Name();
            case 4:
                return Faker.Internet.Email();
            case 5:
                return Faker.Lorem.Sentence();
            case 6:
                return Faker.Phone.Number();
            case 8:
                return Faker.RandomNumber.Next(1, 1000).ToString();
            default:
                return "Test";
        }
    }
}
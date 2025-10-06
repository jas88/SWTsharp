using SWTSharp;

namespace SWTSharp.Sample;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("SWTSharp Sample Application");
        Console.WriteLine($"Platform: {Display.Platform}");
        Console.WriteLine();

        try
        {
            // Create display and shell
            var display = Display.Default;
            var shell = new Shell(display);
            shell.Text = "SWTSharp Demo";
            shell.SetSize(400, 300);

            // Create a label
            var label = new Label(shell, SWT.NONE);
            label.Text = "Welcome to SWTSharp!";
            label.SetBounds(10, 10, 200, 25);

            // Create a text input
            var textInput = new Text(shell, SWT.BORDER | SWT.SINGLE);
            textInput.SetBounds(10, 50, 250, 25);
            textInput.TextContent = "Enter text here...";

            // Create buttons
            var btnPush = new Button(shell, SWT.PUSH);
            btnPush.Text = "Click Me";
            btnPush.SetBounds(10, 90, 100, 30);
            btnPush.Click += (sender, e) =>
            {
                Console.WriteLine($"Button clicked! Text input: {textInput.TextContent}");
            };

            var btnCheck = new Button(shell, SWT.CHECK);
            btnCheck.Text = "Check Box";
            btnCheck.SetBounds(10, 130, 120, 25);
            btnCheck.Click += (sender, e) =>
            {
                if (sender is Button button)
                {
                    Console.WriteLine($"Check box: {button.Selection}");
                }
            };

            var btnRadio1 = new Button(shell, SWT.RADIO);
            btnRadio1.Text = "Option 1";
            btnRadio1.SetBounds(10, 170, 100, 25);
            btnRadio1.Selection = true;

            var btnRadio2 = new Button(shell, SWT.RADIO);
            btnRadio2.Text = "Option 2";
            btnRadio2.SetBounds(120, 170, 100, 25);

            // Create multi-line text
            var textMulti = new Text(shell, SWT.BORDER | SWT.MULTI | SWT.WRAP);
            textMulti.SetBounds(10, 210, 250, 60);
            textMulti.TextContent = "Multi-line text area.\nYou can type multiple lines here.";

            // Open the shell
            Console.WriteLine("Opening window...");
            shell.Open();

            // Note: Event loop is not fully implemented yet in this version
            // In a complete implementation, this would be:
            // while (!shell.IsDisposed)
            // {
            //     if (!display.ReadAndDispatch())
            //         display.Sleep();
            // }

            Console.WriteLine("\nWindow created successfully!");
            Console.WriteLine("Note: Full event loop support is coming in the next version.");
            Console.WriteLine("Press Enter to close...");
            Console.ReadLine();

            // Clean up
            shell.Dispose();
            display.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex is PlatformNotSupportedException || ex is NotImplementedException)
            {
                Console.WriteLine("\nThis platform may not be fully supported yet.");
                Console.WriteLine("Windows support is implemented. macOS and Linux support coming soon.");
            }
        }
    }
}

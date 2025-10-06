using System;
using SWTSharp;

namespace SWTSharp.Tests
{
    /// <summary>
    /// Simple test for the List widget implementation on macOS.
    /// This verifies that the List widget can be created and manipulated.
    /// </summary>
    public class ListTest
    {
        public static void Main(string[] args)
        {
            // Only run on macOS
            if (!OperatingSystem.IsMacOS())
            {
                Console.WriteLine("This test only runs on macOS");
                return;
            }

            Console.WriteLine("Testing List Widget Implementation...");

            // Create display and shell
            var display = new Display();
            var shell = new Shell(display, SWT.SHELL_TRIM);
            shell.Text = "List Widget Test";
            shell.SetSize(400, 300);

            // Create a List widget with SINGLE selection
            var singleList = new List(shell, SWT.SINGLE | SWT.BORDER);
            singleList.SetBounds(10, 10, 180, 250);

            // Add items
            singleList.Add("Apple");
            singleList.Add("Banana");
            singleList.Add("Cherry");
            singleList.Add("Date");
            singleList.Add("Elderberry");
            singleList.Add("Fig");
            singleList.Add("Grape");

            // Create a List widget with MULTI selection
            var multiList = new List(shell, SWT.MULTI | SWT.BORDER);
            multiList.SetBounds(210, 10, 180, 250);

            // Add items using index
            multiList.Add("Red", 0);
            multiList.Add("Green", 1);
            multiList.Add("Blue", 2);
            multiList.Add("Yellow", 3);
            multiList.Add("Orange", 4);
            multiList.Add("Purple", 5);
            multiList.Add("Pink", 6);

            // Test selection
            singleList.Select(2); // Select "Cherry"
            multiList.SelectionIndices = new int[] { 1, 3, 5 }; // Select Green, Yellow, Purple

            // Print selections
            Console.WriteLine($"Single List Selection: {singleList.SelectionIndex}");
            Console.WriteLine($"Single List Item: {singleList.GetItem(singleList.SelectionIndex)}");

            Console.WriteLine($"Multi List Selections: {string.Join(", ", multiList.SelectionIndices)}");
            Console.WriteLine($"Multi List Items: {string.Join(", ", multiList.Selection)}");

            // Test item count
            Console.WriteLine($"Single List Item Count: {singleList.ItemCount}");
            Console.WriteLine($"Multi List Item Count: {multiList.ItemCount}");

            // Test top index
            singleList.SetTopIndex(3);
            Console.WriteLine($"Single List Top Index: {singleList.GetTopIndex()}");

            // Show shell
            shell.Open();

            Console.WriteLine("List widget created successfully!");
            Console.WriteLine("The window should display two list widgets with items.");
            Console.WriteLine("Press Ctrl+C to exit...");

            // Event loop
            while (!shell.IsDisposed)
            {
                if (!display.ReadAndDispatch())
                {
                    display.Sleep();
                }
            }

            display.Dispose();
            Console.WriteLine("Test completed.");
        }
    }
}

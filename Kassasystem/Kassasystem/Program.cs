using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
    public bool PricePerKilo { get; set; }

    public override string ToString()
    {
        return $"{ProductId} - {ProductName} - {Price} kr/{(PricePerKilo ? "kilo" : "st")}";
    }
}

    public class Kassasystem
    {
    private List<Product> products;
    private List<string> receipt;
    private int receiptNumber;

    public Kassasystem()
    {
        products = new List<Product>
        {
            new Product { ProductId = 300, ProductName = "Bananas", Price = 12.5m, PricePerKilo = true },
            new Product { ProductId = 301, ProductName = "Coffee", Price = 35.5m, PricePerKilo = false },
        };
        LoadProductsFromFile();
        LoadReceiptNumber();
        receipt = new List<string>();
    }

    private Product ParseProduct(string line)
    {
        string[] parts = line.Split(' ');

        if (int.TryParse(parts[0], out int productId) &&
            decimal.TryParse(parts[2], out decimal price) &&
            parts.Length == 4)
        {
            string productName = parts[1];
            bool pricePerKilo = parts[3].ToLower() == "kilo";

            return new Product
            {
                ProductId = productId,
                ProductName = productName,
                Price = price,
                PricePerKilo = pricePerKilo
            };
        }

        return null; // or throw an exception, depending on your error handling strategy
    }


    private void LoadProductsFromFile()
    {
        try
        {
            string[] lines = File.ReadAllLines("products.txt");

            foreach (var line in lines)
            {
                if (ParseProduct(line) is Product product)
                {
                    products.Add(product);
                }
            }
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error loading products: {e.Message}");
        }
    }

    private void LoadReceiptNumber()
    {
        try
        {
            if (int.TryParse(File.ReadAllText("receiptNumber.txt"), out int savedReceiptNumber))
            {
                receiptNumber = savedReceiptNumber;
            }
            else
            {
                receiptNumber = 1;
            }
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error loading receipt number: {e.Message}");
            receiptNumber = 1;
        }
    }

    public void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("Välkommen till kassasystemet!");
        Console.WriteLine("1. Ny kund");
        Console.WriteLine("0. Avsluta");
        Console.Write("Välj en option (1/0): ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                StartNewTransaction();
                break;
            case "0":
                SaveReceiptNumber();
                return; // Avsluta programmet
            default:
                Console.WriteLine("Ogiltig inmatning. Försök igen.");
                break;
        }
    }

    private void StartNewTransaction()
    {
        receipt.Clear();
        Console.Clear();
        Console.WriteLine($"Kassa\nKVITTO {DateTime.Now:yyyy-MM-dd HH:mm}");

        while (true)
        {
            DisplayProducts();
            Console.WriteLine("Kommando:");
            Console.WriteLine("<productid> <antal>");
            Console.WriteLine("Pay");
            Console.Write("Kommando: ");

            string command = Console.ReadLine();

            if (command.Equals("Pay", StringComparison.OrdinalIgnoreCase))
            {
                ProcessPayment();
                return;
            }
            else
            {
                ProcessCommand(command);
            }
        }
    }



    private void DisplayProducts()
    {
        Console.WriteLine("Produkter:");
        foreach (var product in products)
        {
            Console.WriteLine($"{product}");
        }
    }



    private void ProcessCommand(string command)
    {
        string[] parts = command.Split(' ');

        if (parts.Length == 2 && int.TryParse(parts[0], out int productId) && int.TryParse(parts[1], out int quantity))
        {
            AddProduct(productId, quantity);
        }
        else
        {
            Console.WriteLine("Ogiltig inmatning. Försök igen.");
        }
    }



    private void AddProduct(int productId, int quantity)
    {
        var product = products.Find(p => p.ProductId == productId);

        if (product != null)
        {
            decimal totalCost = product.PricePerKilo ? product.Price * quantity : product.Price * quantity;
            string item = $"{product.ProductId} {quantity} {(product.PricePerKilo ? "kilo" : "st")} - {totalCost} kr";
            receipt.Add(item);
            Console.WriteLine($"Lagt till i kvitto: {item}");
        }
        else
        {
            Console.WriteLine("Produkt hittades inte. Försök igen.");
        }
    }

    
    private void ProcessPayment()
    {
        if (receipt != null)
        {
            DisplayReceipt();
            SaveReceiptToFile(receipt);
            receipt.Clear();
            Console.WriteLine("Betalning slutförd. Tack!");
        }
        else
        {
            Console.WriteLine("Inget kvitto att behandla.");
        }
    }

    private void DisplayReceipt()
    {
        Console.WriteLine("\nKvitto:");
        foreach (var item in receipt)
        {
            Console.WriteLine(item);
        }
    }

    private void SaveReceiptToFile(List<string> receipt)
    {
        if (receipt != null && receipt.Count > 0)
        {
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(projectDirectory, "Receipts");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"RECEIPT_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string fullPath = Path.Combine(folderPath, fileName);

            // Use a separator with a unique identifier to distinguish between different receipts
            string separator = $"-------------------- {DateTime.Now:yyyy-MM-dd HH:mm:ss} --------------------";
            File.AppendAllLines(fullPath, new[] { separator });
            File.AppendAllLines(fullPath, receipt);
            Console.WriteLine($"Kvitto sparat till fil: {fullPath}{Environment.NewLine}");
        }
        else
        {
            Console.WriteLine("Inget kvitto att spara.");
        }
    }

    private void SaveReceiptNumber()
    {
        try
        {
            File.WriteAllText("receiptNumber.txt", receiptNumber.ToString());
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error saving receipt number: {e.Message}");
        }
    }

    static void Main(string[] args)
    {
        Kassasystem kassasystem = new Kassasystem();
        kassasystem.DisplayMenu();
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class BudgetManager
{
    static string expenseFilePath = "expenses.txt";
    static string userFilePath = "user.txt";

    static void Main(string[] args)
    {
        User user = LoadUserFromFile();
        List<Expense> expenses = LoadExpensesFromFile();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Budget Manager");
            Console.WriteLine("=================");
            Console.WriteLine("1. Set Monthly Budget");
            Console.WriteLine("2. Add a New Expense");
            Console.WriteLine("3. View All Expenses");
            Console.WriteLine("4. View Remaining Budget");
            Console.WriteLine("5. Edit or Delete an Expense");
            Console.WriteLine("6. View Data Summary");
            Console.WriteLine("7. Exit");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    user.MonthlyBudget = SetBudget();
                    SaveUserToFile(user);
                    break;
                case "2":
                    AddExpense(expenses);
                    SaveExpensesToFile(expenses);
                    break;
                case "3":
                    ViewExpenses(expenses);
                    break;
                case "4":
                    ViewRemainingBudget(user, expenses);
                    break;
                case "5":
                    EditOrDeleteExpense(expenses);
                    SaveExpensesToFile(expenses);
                    break;
                case "6":
                    ViewDataSummary(user, expenses);
                    break;
                case "7":
                    return;
                default:
                    Console.WriteLine("Invalid option. Press any key to try again.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static decimal SetBudget()
    {
        Console.Write("Enter your monthly budget amount: ");
        if (decimal.TryParse(Console.ReadLine(), out decimal budget))
        {
            Console.WriteLine("Monthly budget set successfully!");
            Console.ReadKey();
            return budget;
        }
        else
        {
            Console.WriteLine("Invalid input. Press any key to return to the menu.");
            Console.ReadKey();
            return 0;
        }
    }

    static void AddExpense(List<Expense> expenses)
    {
        Console.Write("Enter expense category: ");
        string category = Console.ReadLine();

        Console.Write("Enter expense amount: ");
        if (decimal.TryParse(Console.ReadLine(), out decimal amount))
        {
            Console.Write("Enter expense date (yyyy-MM-dd): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime date))
            {
                expenses.Add(new Expense { Category = category, Amount = amount, Date = date });
                Console.WriteLine("Expense added successfully!");
            }
            else
            {
                Console.WriteLine("Invalid date. Expense not added.");
            }
        }
        else
        {
            Console.WriteLine("Invalid amount. Expense not added.");
        }
        Console.ReadKey();
    }

    static void ViewExpenses(List<Expense> expenses)
    {
        Console.WriteLine("Your Expenses:");
        Console.WriteLine("==============");
        foreach (var expense in expenses)
        {
            Console.WriteLine($"{expense.Date:yyyy-MM-dd} | {expense.Category} | {expense.Amount:C}");
        }
        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey();
    }

    static void ViewRemainingBudget(User user, List<Expense> expenses)
    {
        decimal totalExpenses = expenses.Sum(e => e.Amount);
        decimal remainingBudget = user.MonthlyBudget - totalExpenses;

        Console.WriteLine($"Remaining Budget: {remainingBudget:C}");

        if (user.MonthlyBudget > 0 && remainingBudget / user.MonthlyBudget < 0.2m)
        {
            Console.WriteLine("Warning: Your remaining budget is below 20%!");
        }

        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey();
    }

    static void EditOrDeleteExpense(List<Expense> expenses)
    {
        Console.WriteLine("Your Expenses:");
        for (int i = 0; i < expenses.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {expenses[i].Date:yyyy-MM-dd} | {expenses[i].Category} | {expenses[i].Amount:C}");
        }

        Console.Write("Enter the number of the expense to edit or delete: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= expenses.Count)
        {
            Console.WriteLine("1. Edit Expense");
            Console.WriteLine("2. Delete Expense");
            Console.Write("Choose an option: ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Enter new category: ");
                expenses[index - 1].Category = Console.ReadLine();

                Console.Write("Enter new amount: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                {
                    expenses[index - 1].Amount = amount;
                    Console.Write("Enter new date (yyyy-MM-dd): ");
                    if (DateTime.TryParse(Console.ReadLine(), out DateTime date))
                    {
                        expenses[index - 1].Date = date;
                        Console.WriteLine("Expense updated successfully!");
                    }
                }
            }
            else if (choice == "2")
            {
                expenses.RemoveAt(index - 1);
                Console.WriteLine("Expense deleted successfully!");
            }
        }
        else
        {
            Console.WriteLine("Invalid selection.");
        }
        Console.ReadKey();
    }

    static void ViewDataSummary(User user, List<Expense> expenses)
    {
        Console.WriteLine("Data Summary:");
        Console.WriteLine("=============");

        var categoryTotals = expenses
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) });

        foreach (var category in categoryTotals)
        {
            Console.WriteLine($"{category.Category}: {category.Total:C}");
        }

        decimal totalExpenses = expenses.Sum(e => e.Amount);
        decimal remainingBudget = user.MonthlyBudget - totalExpenses;

        Console.WriteLine($"Total Expenses: {totalExpenses:C}");
        Console.WriteLine($"Remaining Budget: {remainingBudget:C}");

        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey();
    }

    static List<Expense> LoadExpensesFromFile()
    {
        List<Expense> expenses = new List<Expense>();

        if (File.Exists(expenseFilePath))
        {
            string[] lines = File.ReadAllLines(expenseFilePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 3 &&
                    decimal.TryParse(parts[1], out decimal amount) &&
                    DateTime.TryParse(parts[2], out DateTime date))
                {
                    expenses.Add(new Expense { Category = parts[0], Amount = amount, Date = date });
                }
            }
        }

        return expenses;
    }

    static void SaveExpensesToFile(List<Expense> expenses)
    {
        List<string> lines = expenses.Select(e => $"{e.Category},{e.Amount},{e.Date:yyyy-MM-dd}").ToList();
        File.WriteAllLines(expenseFilePath, lines);
    }

    static User LoadUserFromFile()
    {
        if (File.Exists(userFilePath))
        {
            string[] parts = File.ReadAllText(userFilePath).Split(',');
            if (parts.Length == 2 && decimal.TryParse(parts[1], out decimal budget))
            {
                return new User { Name = parts[0], MonthlyBudget = budget };
            }
        }

        return new User { Name = "Default User", MonthlyBudget = 0 };
    }

    static void SaveUserToFile(User user)
    {
        File.WriteAllText(userFilePath, $"{user.Name},{user.MonthlyBudget}");
    }
}

class User
{
    public string Name { get; set; }
    public decimal MonthlyBudget { get; set; }
}

class Expense
{
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

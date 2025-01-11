using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Spectre.Console;

public interface IGame
{
    void Play();
}

public class Hangman : IGame
{
    private readonly string OrdAttGissa;
    private readonly HashSet<char> Gissaord;
    private int AntalGissning;
    private readonly List<string> GissningsHistoria;
    private readonly IGameHistoryService SpelHistora;

    public Hangman(string word, int attempts, IGameHistoryService gameHistoryService)
    {
        OrdAttGissa = word.ToUpper();
        Gissaord = new HashSet<char>();
        AntalGissning = attempts;
        GissningsHistoria = new List<string>();
        this.SpelHistora = gameHistoryService;
    }

    public void Play()
    {
        while (AntalGissning > 0 && !IsWordGuessed())
        {
            DisplayGameStatus();

            char guessedLetter = GetUserGuess();

            if (guessedLetter == '\0') continue;

            ProcessGuess(guessedLetter);
        }

        EndGame();
    }

    private void DisplayGameStatus()
    {
        Console.Clear();
        DisplayWord();
        Console.WriteLine($"\nRemaining attempts: {AntalGissning}");
    }

    private char GetUserGuess()
    {
        Console.Write("Gissa boksktav: ");
        string input = Console.ReadLine()?.ToUpper();

        if (string.IsNullOrEmpty(input) || !char.TryParse(input, out char Gissaord) || input.Length > 1)
        {
            Console.WriteLine("Fel Bokstav Försök igen! ;)");
            Console.ReadKey();
            return '\0'; // Invalid input, continue the loop
        }

        return Gissaord;
    }

    private void ProcessGuess(char guessedLetter)
    {
        if (Gissaord.Contains(guessedLetter))
        {
            Console.WriteLine("Du har redan gissat denna bosktaven. Försök igen.");
        }
        else
        {
            Gissaord.Add(guessedLetter);
            GissningsHistoria.Add(guessedLetter.ToString());

            if (OrdAttGissa.Contains(guessedLetter))
            {
                Console.WriteLine($"Bra jobbat Rätt Bokstav'{guessedLetter}'Denna bosktaven finns med.");
                AnsiConsole.Markup("[red]Bra jobbat! Rätt Bokstav '{Gissaord}' Denna bokstav finns med.[/]");


            }
            else
            {
                Console.WriteLine($"The letter '{guessedLetter}' is not in the word.");
                AntalGissning--;
            }
        }

        Console.ReadKey();
    }

    private void DisplayWord()
    {
        foreach (var letter in OrdAttGissa)
        {
            if (Gissaord.Contains(letter))
            {
                AnsiConsole.Markup("[green]" + letter + " [/]");
            }
            else
            {
                AnsiConsole.Markup("[red]_ [/]");
            }
        }
        Console.WriteLine();
    }

    private bool IsWordGuessed()
    {
        return OrdAttGissa.All(letter => Gissaord.Contains(letter));
    }

    private void EndGame()
    {
        Console.Clear();
        if (IsWordGuessed())
        {
            Console.WriteLine($"Congratulations! You've guessed the word: {OrdAttGissa}");
        }
        else
        {
            Console.WriteLine($"Game Over. The word was: {OrdAttGissa}");
        }

        SpelHistora.SaveHistory(OrdAttGissa, GissningsHistoria, AntalGissning);
    }
}

public interface IGameHistoryService
{
    void SaveHistory(string wordToGuess, List<string> guessHistory, int remainingAttempts);
}

public class GameHistoryService : IGameHistoryService
{
    public void SaveHistory(string wordToGuess, List<string> guessHistory, int remainingAttempts)
    {
        var history = new { Word = wordToGuess, Guesses = guessHistory, AttemptsLeft = remainingAttempts };
        string json = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText("game_history.json", json);
        Console.WriteLine("Game history saved to game_history.json.");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        AnsiConsole.Markup("[bold yellow]Welcome to Hangman![/]\n");
        Console.Write("Enter a word to guess (hidden from player): ");

        string wordToGuess = Console.ReadLine();
        Console.Clear();

        IGameHistoryService gameHistoryService = new GameHistoryService();
        Hangman game = new Hangman(wordToGuess, 6, gameHistoryService);
        game.Play();

        AnsiConsole.Markup("[bold green]Thanks for playing Hangman![/]\n");
    }
}

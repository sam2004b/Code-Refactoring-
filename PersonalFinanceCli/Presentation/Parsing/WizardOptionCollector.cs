using System.Text.RegularExpressions;

namespace PersonalFinanceCli.Presentation.Parsing;

public sealed class WizardOptionCollector
{
    private static readonly Regex StrictDateRegex = 
        new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);

    public WizardOptions Collect(IReadOnlyList<string> tokens, int startIndex)
    {
        string? cardRaw = null;
        DateOnly? date = null;
        string? note = null;

        var currentIndex = startIndex;
        while (currentIndex < tokens.Count)
        {
            var option = tokens[currentIndex];
            if (option == "--card")
            {
                currentIndex++;
                cardRaw = currentIndex < tokens.Count ? tokens[currentIndex] : null;
                if (string.IsNullOrWhiteSpace(cardRaw))
                {
                    return new WizardOptions(null, null, null, "Invalid --card value.");
                }
            }
            else if (option == "--date")
            {
                currentIndex++;
                var rawDate = currentIndex < tokens.Count ? tokens[currentIndex] : null;
                if (string.IsNullOrWhiteSpace(rawDate) || !StrictDateRegex.IsMatch(rawDate) || !DateOnly.TryParse(rawDate, out var parsedDate))
                {
                    return new WizardOptions(null, null, null, "Invalid --date value. Use strict YYYY-MM-DD.");
                }

                date = parsedDate;
            }
            else if (option == "--note")
            {
                currentIndex++;
                if (currentIndex >= tokens.Count)
                {
                    return new WizardOptions(null, null, null, "Invalid --note value.");
                }

                var rawNote = tokens[currentIndex];
                if (!rawNote.Contains(' '))
                {
                    return new WizardOptions(null, null, null, "Wizard requires quoted note for --note.");
                }

                note = rawNote;
            }
            else
            {
                return new WizardOptions(null, null, null, $"Unknown option {option}.");
            }

            currentIndex++;
        }

        return new WizardOptions(cardRaw, date, note, null);
    }
}

public readonly record struct WizardOptions(
    string? CardRaw,
     DateOnly? Date,
     string? Note,
     string? Error
);

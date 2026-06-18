using System;
using System.Text.RegularExpressions;
public class Test {
    public static void Main() {
        string input = "???";
        bool match = Regex.IsMatch(input, "^[\p{IsArabic}a-zA-Z0-9\s\.\-]+$");
        Console.WriteLine(match);
    }
}

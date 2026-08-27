using System;

public static class EloSystemScr
{
    private const double MinimumRating = 100.0;

    private static int GetKFactor(double rating, int matchesPlayed)
    {
        if(matchesPlayed < 15) return 40;
        if(rating < 1800) return 25;
        return 15;
    }

    public static (int newRatingA, int newRatingB) CalculateNewRatings(
        double ratingA, double ratingB, 
        int matchesA, int matchesB, 
        int goalsA, int goalsB)
    {
        double scoreA = goalsA > goalsB ? 1.0 : 0.0;
        double scoreB = 1.0 - scoreA;

        double expectedA = 1.0 / (1.0 + Math.Pow(10.0, (ratingB - ratingA) / 400.0));
        double expectedB = 1.0 - expectedA;

        int goalDifference = Math.Abs(goalsA - goalsB);
        double marginOfVictoryMultiplier = 1.0 + (Math.Sqrt(goalDifference) - 1.0) * 0.5;

        int kA = GetKFactor(ratingA, matchesA);
        int kB = GetKFactor(ratingB, matchesB);

        double deltaA = kA * (scoreA - expectedA) * marginOfVictoryMultiplier;
        double deltaB = kB * (scoreB - expectedB) * marginOfVictoryMultiplier;

        double finalA = Math.Max(MinimumRating, ratingA + deltaA);
        double finalB = Math.Max(MinimumRating, ratingB + deltaB);

        int newRatingA = (int)Math.Round(finalA, MidpointRounding.AwayFromZero);
        int newRatingB = (int)Math.Round(finalB, MidpointRounding.AwayFromZero);

        return (newRatingA, newRatingB);
    }
}
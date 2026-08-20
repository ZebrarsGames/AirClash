using System;

public static class EloSystemScr
{
    private const double MinimumRating = 100.0;

    private static int GetKFactor(double rating, int matchesPlayed)
    {
        if (matchesPlayed < 20) return 40;
        if (rating >= 2400) return 10;
        return 20;
    }

    public static (int newRatingA, int newRatingB) CalculateNewRatings(
        double ratingA, double ratingB, 
        int matchesA, int matchesB, 
        int goalsA, int goalsB)
    {
        double scoreA = 1.0;
        double scoreB = 0.0;

        if (goalsA < goalsB)
        {
            scoreA = 0.0;
            scoreB = 1.0;
        }

        double expectedA = 1.0 / (1.0 + Math.Pow(10.0, (ratingB - ratingA) / 400.0));
        double expectedB = 1.0 - expectedA;

        int goalDifference = Math.Abs(goalsA - goalsB);
        double marginOfVictoryMultiplier = Math.Log(goalDifference + 1) * 1.5; 

        int kA = GetKFactor(ratingA, matchesA);
        int kB = GetKFactor(ratingB, matchesB);

        double deltaA = kA * (scoreA - expectedA) * marginOfVictoryMultiplier;
        double deltaB = kB * (scoreB - expectedB) * marginOfVictoryMultiplier;

        double finalA = ratingA + deltaA;
        double finalB = ratingB + deltaB;

        if (finalA < MinimumRating) finalA = MinimumRating;
        if (finalB < MinimumRating) finalB = MinimumRating;

        int newRatingA = (int)Math.Round(finalA, MidpointRounding.AwayFromZero);
        int newRatingB = (int)Math.Round(finalB, MidpointRounding.AwayFromZero);

        return (newRatingA, newRatingB);
    }
}
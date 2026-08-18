using System;

public static class EloSystemScr
{
    private static int GetKFactor(double rating, int matchesPlayed)
    {
        if(matchesPlayed < 20) return 40;
        if(rating >= 2400) return 10;
        return 20;
    }

    public static (int newRatingA, int newRatingB) CalculateNewRatings(
        double ratingA, double ratingB, int matchesA, int matchesB, double scoreA)
    {
        double expectedA = 1.0 / (1.0 + Math.Pow(10.0, (ratingB - ratingA) / 400.0));

        double scoreB = 1.0 - scoreA; 

        int kA = GetKFactor(ratingA, matchesA);
        int kB = GetKFactor(ratingB, matchesB);
        int matchK = (kA + kB) / 2;

        int deltaA = (int)Math.Round(matchK * (scoreA - expectedA));
        int deltaB = -deltaA;

        int newRatingA = (int)Math.Round(ratingA) + deltaA;
        int newRatingB = (int)Math.Round(ratingB) + deltaB;

        if(newRatingA < 100)
        {
            int realLossA = 100 - (int)Math.Round(ratingA);
            newRatingA = 100;
            newRatingB = (int)Math.Round(ratingB) - realLossA;
        }
        else if(newRatingB < 100)
        {
            int realLossB = 100 - (int)Math.Round(ratingB);
            newRatingB = 100;
            newRatingA = (int)Math.Round(ratingA) - realLossB;
        }

        return (newRatingA, newRatingB);
    }
}
public static class GameLives
{
    public const int MaxLives = 3;
    public static int Lives = MaxLives;

    public static void Refill() => Lives = MaxLives;
}
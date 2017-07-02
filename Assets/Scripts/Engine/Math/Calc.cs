public static class Calc
{
    public static float Mod(float a, float b)
    {
        return (a % b + b) % b;
    }

    public static int Mod(int a, int b)
    {
        return (a % b + b) % b;
    }
}
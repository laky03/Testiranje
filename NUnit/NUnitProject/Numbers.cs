namespace NUnitProject;

public static class Numbers
{
    private const int offset = 3;

    public static bool IsPrime(int number)
    {
        if (number < 2)
        {
            return false;
        }

        for (int i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0)
            {
                return false;
            }
        }

        return true;
    }

    public static int NthPrimeNumber(int n)
    {
        CancellationToken token = new();
        return FindNPrimeNumbers(n, token).ElementAt(n - 1);
    }

    public static IEnumerable<int> FindNPrimeNumbers(int n, CancellationToken token)
    {
        int X = 0, i = 2;
        bool flag;

        while (X < n && !token.IsCancellationRequested)
        {
            flag = true;

            for (int j = 2; j <= Math.Sqrt(i); j++)
            {
                if (i % j == 0)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                yield return i;
                X++;
            }
            i++;
        }

        if (token.IsCancellationRequested)
        {
            throw new TimeoutException("Korisnik je zatražio prekid izvršenja.");
        }
    }

    public static IEnumerable<int> FindPrimeNumbersLowerThanN(int n)
    {
        if (n < 2)
        {
            throw new ArgumentException("Upper Limit be must greater than or equal to 2.");
        }

        yield return 2;

        if (n == 2)
        {
            yield break;
        }

        var bits = new BitArray(ToIndex(n) + 1, defaultValue: true);
        var upperSqrtIndex = ToIndex((int)Math.Sqrt(n));

        for (var i = 0; i <= upperSqrtIndex; i++)
        {
            if (!bits[i])
            {
                continue;
            }

            var number = ToNumber(i);

            yield return number;

            for (var j = ToIndex(number * number); (j > i) && (j < bits.Length); j += number)
            {
                bits[j] = false;
            }
        }

        for (var i = upperSqrtIndex + 1; i < bits.Length; i++)
        {
            if (bits[i])
            {
                yield return ToNumber(i);
            }
        }
    }

    static int ToNumber(int index)
    {
        return (2 * index) + offset;
    }
    static int ToIndex(int number)
    {
        return (number - offset) / 2;
    }
}

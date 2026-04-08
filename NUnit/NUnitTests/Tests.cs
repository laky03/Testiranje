using NUnitProject;

namespace NUnitTests;

[TestFixture]
public class Tests
{
    [OneTimeSetUp]
    public void Init()
    {
        Console.WriteLine("Pre svih testova.");
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        Console.WriteLine("Posle svih testova.");
    }
    
    [SetUp]
    public void Setup()
    {
        Console.WriteLine("Pre svakog testa.");
    }

    [TearDown]
    public void TearDown()
    {
        Console.WriteLine("Posle svakog testa.");
    }

    [TestCase(1, Description = "Provera prostog broja", TestName = "IsPrimeTest")]
    public void Test1(int broj)
    {
        Assert.That(!Numbers.IsPrime(broj));
    }

    [Ignore("Ignorisan test.")]
    [TestCase(2, TestName = "IsPrimeTest2")]
    [TestCase(3, TestName = "IsPrimeTest3")]
    [TestCase(5, TestName = "IsPrimeTest4")]
    [TestCase(7, TestName = "IsPrimeTest5")]
    public void Test2(int broj)
    {
        Assert.That(Numbers.IsPrime(broj));
    }

    [TestCase(4, ExpectedResult = 7, TestName = "NThPrime")]
    public int Test3(int n)
    {
        return Numbers.NthPrimeNumber(n);
    }

    [Repeat(3)]
    [TestCase(TestName = "ArgumentException")]
    //[Platform(Exclude = "Win")]
    public void Test4()
    {
        Assert.Throws(typeof(ArgumentException), new TestDelegate(() =>
        {
            Numbers.FindPrimeNumbersLowerThanN(0).ToList();
        }));
    }

    [Test]
    [CancelAfter(1000)] // Koristi CancellationToken
    // [Timeout(5000)] obsolete
    public void Test5(CancellationToken token)
    {
        Assert.Multiple(() =>
        {
            List<int> list = new();

            Assert.Throws<TimeoutException>(() =>
            {
                list = Numbers.FindNPrimeNumbers(int.MaxValue, token).ToList();
            });

            Assert.That(list, Is.Empty);
        });
    }

    [Test]
    [Retry(5)]          // Failed test ponovo se pokreće maksimalno 5 puta
    [CancelAfter(1000)] // Koristi CancellationToken
    // [Timeout(5000)] obsolete
    public async Task Test5Drugi(CancellationToken token)
    {
        List<int> result = new();

        await Task.Run(() =>
        {
            Assert.Throws<TimeoutException>(() =>
            {
                result = Numbers.FindNPrimeNumbers(int.MaxValue, token).ToList();
            });
        }, token);

        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public void Test6(
        [Values(1, 2, 3)] int broj,
        [Random(2, 10, 2, Distinct = true)] int nasumicno,
        [Range(1, 2, 0.5)] double doubleBroj)
    {
        Assert.Multiple(() =>
        {
            Assert.That(broj, Is.LessThanOrEqualTo(3));
            Assert.That(nasumicno, Is.GreaterThanOrEqualTo(0));
            Assert.That(doubleBroj, Is.Not.NaN);
        });
    }

    [Test]
    public void Test()
    {
        List<int> lista = Enumerable
            .Range(100, 102)
            .OrderByDescending(p => p)
            .ToList();

        Assert.That(lista, Is.Not.Ordered);
    }
}

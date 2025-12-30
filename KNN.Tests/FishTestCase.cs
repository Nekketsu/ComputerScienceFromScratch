using System.Globalization;

namespace KNN.Tests;

public class FishTestCase
{
    private string dataFile = Path.Combine("datasets", "fish", "fish.csv");

    public FishTestCase()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Fact]
    public void TestNearest()
    {
        var k = 3;
        var fishKnn = new KNN<Fish>(dataFile);
        var testFish = new Fish("", 0.0f, 30.0f, 32.5f, 38.0f, 12.0f, 5.0f);

        var nearestFish = fishKnn.Nearest(k, testFish);
        
        Assert.Equal(3, nearestFish.Length);

        Fish[] expectedFish =
        [
            new Fish("Bream", 340.0f, 29.5f, 32.0f, 37.3f, 13.9129f, 5.0728f),
            new Fish("Bream", 500.0f, 29.1f, 31.5f, 36.4f, 13.7592f, 4.368f),
            new Fish("Bream", 700.0f, 30.4f, 33.0f, 38.3f, 14.8604f, 5.2854f)
        ];

        Assert.Equivalent(expectedFish, nearestFish);
    }

    [Fact]
    public void TestClassify()
    {
        var k = 5;
        var fishKnn = new KNN<Fish>(dataFile);
        var testFish = new Fish("", 0.0f, 20.0f, 23.5f, 24.0f, 10.0f, 4.0f);

        var classifyFish = fishKnn.Classify(k, testFish);

        Assert.Equal("Parkki", classifyFish);
    }
}

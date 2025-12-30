using Xunit.Abstractions;

namespace KNN.Tests;

public class DigitsTestCase(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private string dataFile = Path.Combine("datasets", "digits", "digits.csv");
    private string testFile = Path.Combine("datasets", "digits", "digits_test.csv");

    [Fact]
    public void TestDigitsTestSet()
    {
        var k = 1;
        var digitsKnn = new KNN<Digit>(dataFile, false);
        var testDataPoints = File.ReadLines(testFile)
            .Select(row => Digit.FromStringData(row.Split(',')))
            .ToArray();

        var correctClassifications = 0;
        foreach (var testDataPoint in testDataPoints)
        {
            var predictedDigit = digitsKnn.Classify(k, testDataPoint);
            if (predictedDigit == testDataPoint.Kind)
            {
                correctClassifications++;
            }
        }
        var correctPercentage = (double)correctClassifications / testDataPoints.Length * 100;
        output.WriteLine($"Correct Classifications: {correctClassifications} of {testDataPoints.Length} or {correctPercentage}%");

        Assert.True(correctPercentage > 97.0);
    }
}

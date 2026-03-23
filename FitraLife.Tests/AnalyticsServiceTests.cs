using FitraLife.Models;
using FitraLife.Services;

namespace FitraLife.Tests;

[TestClass]
public class AnalyticsServiceTests
{
    private readonly AnalyticsService _service = new();

    [TestMethod]
    public void PredictGoalDate_ReturnsMessage_WhenLogsAreNull()
    {
        var result = _service.PredictGoalDate(null!, 80, 75, 1700);

        Assert.AreEqual("Not enough data to make a prediction. Log your activity for a few days!", result.Message);
    }

    [TestMethod]
    public void PredictGoalDate_ReturnsMessage_WhenLogsAreEmpty()
    {
        var result = _service.PredictGoalDate(new List<FitnessLog>(), 80, 75, 1700);

        Assert.AreEqual("Not enough data to make a prediction. Log your activity for a few days!", result.Message);
    }

    [TestMethod]
    public void PredictGoalDate_ReturnsMessage_WhenTargetWeightIsInvalid()
    {
        var logs = new List<FitnessLog>
        {
            new() { CaloriesEaten = 2000, CaloriesBurned = 200 }
        };

        var result = _service.PredictGoalDate(logs, 80, 0, 1700);

        Assert.AreEqual("Please set a target weight in your profile to see predictions.", result.Message);
    }

    [TestMethod]
    public void PredictGoalDate_ReturnsNotOnTrack_WhenTryingToLoseButInSurplus()
    {
        var logs = new List<FitnessLog>
        {
            new() { CaloriesEaten = 2500, CaloriesBurned = 200 }
        };

        var result = _service.PredictGoalDate(logs, 80, 75, 1700);

        Assert.IsFalse(result.IsOnTrack);
        StringAssert.Contains(result.Message, "aiming to lose weight");
        StringAssert.Contains(result.Message, "projected to GAIN");
    }

    [TestMethod]
    public void PredictGoalDate_ReturnsNotOnTrack_WhenTryingToGainButInDeficit()
    {
        var logs = new List<FitnessLog>
        {
            new() { CaloriesEaten = 1500, CaloriesBurned = 300 }
        };

        var result = _service.PredictGoalDate(logs, 70, 75, 1700);

        Assert.IsFalse(result.IsOnTrack);
        StringAssert.Contains(result.Message, "aiming to gain weight");
        StringAssert.Contains(result.Message, "projected to LOSE");
    }

    [TestMethod]
    public void PredictGoalDate_ReturnsMaintenanceMessage_WhenDailyChangeIsNearZero()
    {
        var logs = new List<FitnessLog>
        {
            new() { CaloriesEaten = 2000, CaloriesBurned = 200 }
        };

        var result = _service.PredictGoalDate(logs, 80, 80, 1800);

        Assert.IsTrue(result.IsOnTrack);
        Assert.AreEqual("You are currently maintaining your weight perfectly.", result.Message);
    }

    [TestMethod]
    public void PredictGoalDate_ReturnsLongTermMessage_WhenPredictionExceedsTwoYears()
    {
        var logs = new List<FitnessLog>
        {
            new() { CaloriesEaten = 1892, CaloriesBurned = 200 }
        };

        var result = _service.PredictGoalDate(logs, 90, 80, 1700);

        Assert.IsTrue(result.IsOnTrack);
        StringAssert.Contains(result.Message, "more than 2 years");
    }

    [TestMethod]
    public void PredictGoalDate_ReturnsPredictedDate_WhenOnTrack()
    {
        var logs = new List<FitnessLog>
        {
            new() { CaloriesEaten = 1800, CaloriesBurned = 200 }
        };

        var result = _service.PredictGoalDate(logs, 80, 79, 1800);

        Assert.IsTrue(result.IsOnTrack);
        Assert.IsNotNull(result.PredictedDate);
        StringAssert.Contains(result.Message, "On track to reach 79kg by");
    }
}

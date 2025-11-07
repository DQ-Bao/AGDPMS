using AGDPMS.Shared.Services;

namespace AGDPMS.Shared.Tests;

public class ICutOptimizationServiceTest
{
    public static TheoryData<ProfileCutTestCase> GetProfileCutTestData()
    {
        double L = 6000;
        return
        [
            new() { Code = "C3202", L = L, Lengths = [393.5, 431.5, 910, 1318, 2076.5], Demands = [4, 4, 2, 8, 2], ExpectedBarsUsed = 4 },
            new() { Code = "C3209", L = L, Lengths = [2500.0, 2650.0], Demands = [2.0, 2.0], ExpectedBarsUsed = 2 },
            new()
            {
                Code = "C3295",
                L = L,
                Lengths = [
                    241.5, 279.5, 407, 511.2, 511.3,
                    549.2, 549.3, 758, 1203.6, 1962.1,
                    2123.6, 2263.6, 2400, 2587.6
                ],
                Demands = [
                    4, 4, 4, 2, 2,
                    2, 2, 2, 8, 2,
                    8, 4, 2, 2
                ],
                ExpectedBarsUsed = 10
            }
        ];
    }

    [Theory]
    [MemberData(nameof(GetProfileCutTestData))]
    public void Solve_ShouldReturnExpectedPatternQuantitySum(ProfileCutTestCase testCase)
    {
        var result = ICutOptimizationService.Solve(testCase.L, testCase.Lengths, testCase.Demands);
        Assert.Equal(testCase.ExpectedBarsUsed, result.pattern_quantity.Sum(), precision: 0);
    }
}

[Serializable]
public class ProfileCutTestCase
{
    public required string Code { get; init; }
    public double L { get; init; }
    public required double[] Lengths { get; init; }
    public required double[] Demands { get; init; }
    public double ExpectedBarsUsed { get; init; }
}

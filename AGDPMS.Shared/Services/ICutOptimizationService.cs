using Google.OrTools.LinearSolver;

namespace AGDPMS.Shared.Services;

public interface ICutOptimizationService
{
    static Solution Solve(double L, double[] lengths, double[] demands)
    {
        Solution solution = new Solution();
        solution.lengths = lengths;
        solution.demands = demands;
        solution.stock_len = [L];
        int n = lengths.Length;

        // === Initial Patterns (Greedy) ===
        List<double[]> patterns = GenerateInitialPattern(L, lengths, demands);

        bool continueGen = true;
        int iter = 0;
        double[] finalX = null;
        double finalObj = 0.0;

        // === Column generation loop ===
        while (continueGen)
        {
            iter++;

            // --- Solve LP Master ---
            double[] duals = SolveMaster(patterns, demands, out double[] x, out double obj);
            finalX = x;
            finalObj = obj;

            // --- Solve Subproblem (Knapsack) ---
            double[] newPattern = SolveKnapsack(L, lengths, duals, out double patternValue);
            double reducedCost = 1.0 - patternValue;

            if (reducedCost >= -1e-6)
            {
                continueGen = false;
            }
            else
            {
                patterns.Add(newPattern);
            }
        }

        // === Solve Final Integer Master with Waste Minimization ===
        SolveFinalIntegerMaster(patterns, demands, lengths, L, out finalX, out finalObj);

        // === Report Results ===
        for (int j = 0; j < patterns.Count; j++)
        {
            if (finalX[j] > 1e-6)
            {
                solution.patterns.Add(patterns[j]);
            }
        }
        double totalWaste = 0;
        double[] wastes = new double[solution.patterns.Count];
        double[] used = new double[solution.patterns.Count];
        for (int j = 0; j < solution.patterns.Count; j++)
        {
            used[j] = patterns[j].Select((v, i) => v * lengths[i]).Sum();
            wastes[j] = L - used[j];
            totalWaste += wastes[j] * finalX[j];
        }
        solution.total_waste = totalWaste;
        solution.wastes = wastes;
        solution.used = used;
        solution.pattern_quantity = new double[solution.patterns.Count];
        for (int i = 0, j = 0; i < finalX.Length; i++)
        {
            if (finalX[i] > 1e-6)
            {
                solution.pattern_quantity[j] = finalX[i];
                j++;
            }
        }

        return solution;
    }

    // === Greedy pattern generator ===
    static List<double[]> GenerateInitialPattern(double L, double[] lengths, double[] demand)
    {
        int n = lengths.Length;
        var patterns = new List<double[]>();

        // Sort items by decreasing length (greedy fit)
        var order = Enumerable.Range(0, n).OrderByDescending(i => lengths[i]).ToArray();
        double[] remainingDemand = (double[])demand.Clone();

        while (remainingDemand.Sum() > 1e-6)
        {
            double remaining = L;
            double[] pattern = new double[n];

            foreach (int i in order)
            {
                // Take as many as fit, up to remaining demand
                int canFit = (int)Math.Floor(remaining / lengths[i]);
                int use = (int)Math.Min(canFit, remainingDemand[i]);
                if (use > 0)
                {
                    pattern[i] = use;
                    remaining -= use * lengths[i];
                    remainingDemand[i] -= use;
                }
            }

            if (pattern.Sum() == 0)
            {
                // If no item fits, stop (shouldn’t happen normally)
                break;
            }

            patterns.Add(pattern);
        }

        return patterns;
    }

    // === LP Master Problem ===
    static double[] SolveMaster(
        List<double[]> patterns,
        double[] demand,
        out double[] x,
        out double objective
    )
    {
        int m = demand.Length;
        int n = patterns.Count;
        Solver solver = Solver.CreateSolver("GLOP");

        Variable[] vars = new Variable[n];
        for (int j = 0; j < n; j++)
            vars[j] = solver.MakeNumVar(0.0, double.PositiveInfinity, $"x{j}");

        Google.OrTools.LinearSolver.Constraint[] cons = new Google.OrTools.LinearSolver.Constraint[
            m
        ];
        for (int i = 0; i < m; i++)
        {
            cons[i] = solver.MakeConstraint(demand[i], demand[i], $"demand_{i}");
            for (int j = 0; j < n; j++)
                cons[i].SetCoefficient(vars[j], patterns[j][i]);
        }

        Objective obj = solver.Objective();
        foreach (var v in vars)
            obj.SetCoefficient(v, 1.0);
        obj.SetMinimization();

        Solver.ResultStatus status = solver.Solve();
        if (status != Solver.ResultStatus.OPTIMAL)
            throw new Exception("Master problem not optimal!");

        x = vars.Select(v => v.SolutionValue()).ToArray();
        objective = obj.Value();

        double[] duals = new double[m];
        for (int i = 0; i < m; i++)
            duals[i] = cons[i].DualValue();
        return duals;
    }

    // === Integer Final Master with Waste ===
    static void SolveFinalIntegerMaster(
        List<double[]> patterns,
        double[] demand,
        double[] lengths,
        double L,
        out double[] x,
        out double objective
    )
    {
        int m = demand.Length;
        int n = patterns.Count;
        Solver solver = Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");

        Variable[] vars = new Variable[n];
        for (int j = 0; j < n; j++)
            vars[j] = solver.MakeIntVar(0.0, double.PositiveInfinity, $"x{j}");

        // Demand constraints
        for (int i = 0; i < m; i++)
        {
            var cons = solver.MakeConstraint(demand[i], demand[i], $"demand_{i}");
            for (int j = 0; j < n; j++)
                cons.SetCoefficient(vars[j], patterns[j][i]);
        }

        // Compute waste per pattern
        double[] patternWaste = new double[n];
        for (int j = 0; j < n; j++)
        {
            double used = patterns[j].Select((v, i) => v * lengths[i]).Sum();
            patternWaste[j] = L - used;
        }

        // Objective: minimize rolls + epsilon * waste
        Objective obj = solver.Objective();
        for (int j = 0; j < n; j++)
            obj.SetCoefficient(vars[j], 1.0 + 1e-6 * patternWaste[j]);
        obj.SetMinimization();

        Solver.ResultStatus status = solver.Solve();
        if (status != Solver.ResultStatus.OPTIMAL && status != Solver.ResultStatus.FEASIBLE)
            throw new Exception("Integer master problem infeasible!");

        x = vars.Select(v => v.SolutionValue()).ToArray();
        objective = Math.Round(vars.Sum(v => v.SolutionValue())); // rolls used
    }

    // === Knapsack Subproblem ===
    static double[] SolveKnapsack(double L, double[] lengths, double[] dual, out double bestValue)
    {
        int n = lengths.Length;
        int capacity = (int)Math.Round(L);
        double[] dp = new double[capacity + 1];
        int[,] choice = new int[n, capacity + 1];

        for (int i = 0; i < n; i++)
        {
            int itemLen = (int)Math.Round(lengths[i]);
            for (int w = itemLen; w <= capacity; w++)
            {
                double val = dp[w - itemLen] + dual[i];
                if (val > dp[w])
                {
                    dp[w] = val;
                    choice[i, w] = 1;
                }
            }
        }

        double[] pattern = new double[n];
        int rem = capacity;
        for (int i = n - 1; i >= 0; i--)
        {
            int itemLen = (int)Math.Round(lengths[i]);
            while (rem >= itemLen && choice[i, rem] == 1)
            {
                pattern[i]++;
                rem -= itemLen;
            }
        }

        bestValue = dp[capacity];
        return pattern;
    }

    //static Solution SolveMultipleStockSize(double[] stock_lens, int[] stock_limits, double[] lengths, double[] demands)

    //public static CuttingStockSolution SolveMultipleStockSize(IReadOnlyList<RawMaterial> raws, IReadOnlyList<DemandItem> demands)
    public static Solution SolveMultipleStockSize(double[] stock_lens, int[] stock_limits, double[] lengths, double[] demands)
    {
        List<RawMaterial> raws = new List<RawMaterial>();
        List<DemandItem> dmd = new List<DemandItem>();
        var limited = raws.Where(r => r.MaxCount < int.MaxValue).ToList();
        var unlimited = raws.Where(r => r.MaxCount == int.MaxValue).ToList();

        var phase1 = Phase1(limited, dmd);
        //var phase1 = Phase1_Knapsack(limited, demands);

        // Remaining demand
        var remaining = dmd.Select((d, i) =>
        {
            int produced = phase1.Patterns.Sum(p => p.ItemCounts[i] * p.TimesUsed);
            return new DemandItem
            {
                Length = d.Length,
                Quantity = Math.Max(0, d.Quantity - produced)
            };
        }).ToList();

        var phase2 = remaining.Any(d => d.Quantity > 0)
            ? Phase2(unlimited, remaining)
            : new CuttingStockSolution();

        var final = new CuttingStockSolution();
        final.Patterns.AddRange(phase1.Patterns);
        final.Patterns.AddRange(phase2.Patterns);

        return Convert(final);
    }

    public static CuttingStockSolution Phase1(
    IReadOnlyList<RawMaterial> limitedStocks,
    IReadOnlyList<DemandItem> demands)
    {
        //best: 1e6
        const double BIG = 1e6;

        var patterns = new List<Pattern>();

        // -------------------------------------------------
        // Initial patterns: single-item fills
        // -------------------------------------------------
        for (int r = 0; r < limitedStocks.Count; r++)
        {
            double L = limitedStocks[r].Length;

            for (int i = 0; i < demands.Count; i++)
            {
                int maxCuts = Math.Min(
                    (int)(L / demands[i].Length),
                    demands[i].Quantity);

                if (maxCuts <= 0) continue;

                var p = new Pattern
                {
                    RawIndex = r,
                    RawLength = L,
                    Counts = new int[demands.Count]
                };
                p.Counts[i] = maxCuts;
                patterns.Add(p);
            }
        }

        // -------------------------------------------------
        // Column generation
        // -------------------------------------------------
        while (true)
        {
            // ========== MASTER LP ==========
            Solver master = Solver.CreateSolver("GLOP");

            var X = new Variable[patterns.Count];
            for (int j = 0; j < patterns.Count; j++)
                X[j] = master.MakeNumVar(0, double.PositiveInfinity, $"X[{j}]");

            // Demand constraints (≤)
            var demandCons = new Constraint[demands.Count];
            for (int i = 0; i < demands.Count; i++)
            {
                demandCons[i] = master.MakeConstraint(0, demands[i].Quantity);

                for (int j = 0; j < patterns.Count; j++)
                    demandCons[i].SetCoefficient(X[j], patterns[j].Counts[i]);
            }

            // Limited stock constraints (≤)
            for (int r = 0; r < limitedStocks.Count; r++)
            {
                var c = master.MakeConstraint(0, limitedStocks[r].MaxCount);

                for (int j = 0; j < patterns.Count; j++)
                    if (patterns[j].RawIndex == r)
                        c.SetCoefficient(X[j], 1);
            }

            // Objective
            var masterObj = master.Objective();
            for (int j = 0; j < patterns.Count; j++)
            {
                double used = 0;
                int pieces = 0;

                for (int i = 0; i < demands.Count; i++)
                {
                    used += patterns[j].Counts[i] * demands[i].Length;
                    pieces += patterns[j].Counts[i];
                }

                double waste = patterns[j].RawLength - used;
                masterObj.SetCoefficient(X[j], waste - BIG * pieces);
            }
            masterObj.SetMinimization();

            if (master.Solve() != Solver.ResultStatus.OPTIMAL)
                break;

            // Duals (ONLY from demand constraints)
            double[] duals = demandCons.Select(c => c.DualValue()).ToArray();

            // ========== SUBPROBLEM ==========
            Solver sub = Solver.CreateSolver("CBC");

            var subVars = new Variable[demands.Count];
            for (int i = 0; i < demands.Count; i++)
            {
                subVars[i] = sub.MakeIntVar(0, demands[i].Quantity, $"Y[{i}]");
            }

            var useRaw = new Variable[limitedStocks.Count];
            for (int r = 0; r < limitedStocks.Count; r++)
            {
                useRaw[r] = sub.MakeBoolVar($"UseRaw[{r}]");
            }

            // choose exactly one raw
            var oneRaw = sub.MakeConstraint(1, 1);
            for (int r = 0; r < limitedStocks.Count; r++)
            {
                oneRaw.SetCoefficient(useRaw[r], 1);
            }

            // length constraint
            var len = sub.MakeConstraint(double.NegativeInfinity, 0);
            for (int i = 0; i < demands.Count; i++)
            {
                len.SetCoefficient(subVars[i], demands[i].Length);
            }

            for (int r = 0; r < limitedStocks.Count; r++)
            {
                len.SetCoefficient(useRaw[r], -limitedStocks[r].Length);
            }

            // reduced cost objective
            var subObj = sub.Objective();
            for (int i = 0; i < demands.Count; i++)
            {
                //subObj.SetCoefficient(Y[i], -(BIG + duals[i]));
                subObj.SetCoefficient(subVars[i], -(demands[i].Length + BIG + duals[i]));
            }

            for (int r = 0; r < limitedStocks.Count; r++)
            {
                subObj.SetCoefficient(useRaw[r], limitedStocks[r].Length);
            }

            subObj.SetMinimization();
            sub.Solve();

            if (subObj.Value() >= -1e-6) { break; }

            int rawIndex = Enumerable.Range(0, limitedStocks.Count)
                .First(r => useRaw[r].SolutionValue() > 0.5);

            var newPattern = new Pattern
            {
                RawIndex = rawIndex,
                RawLength = limitedStocks[rawIndex].Length,
                Counts = subVars.Select(v => (int)Math.Round(v.SolutionValue())).ToArray()
            };

            if (!patterns.Any(p => SamePattern(p, newPattern)))
            {
                patterns.Add(newPattern);
            }
            else
            {
                break;
            }
        }

        // -------------------------------------------------
        // Final integer master
        // -------------------------------------------------
        Solver final = Solver.CreateSolver("CBC");

        var XF = new Variable[patterns.Count];
        for (int j = 0; j < patterns.Count; j++)
        {
            XF[j] = final.MakeIntVar(0, double.PositiveInfinity, $"X[{j}]");
        }

        for (int i = 0; i < demands.Count; i++)
        {
            var c = final.MakeConstraint(0, demands[i].Quantity);
            for (int j = 0; j < patterns.Count; j++)
                c.SetCoefficient(XF[j], patterns[j].Counts[i]);
        }

        for (int r = 0; r < limitedStocks.Count; r++)
        {
            var c = final.MakeConstraint(0, limitedStocks[r].MaxCount);
            for (int j = 0; j < patterns.Count; j++)
                if (patterns[j].RawIndex == r)
                    c.SetCoefficient(XF[j], 1);
        }

        var finalObj = final.Objective();
        for (int j = 0; j < patterns.Count; j++)
        {
            double used = 0;
            int pieces = 0;

            for (int i = 0; i < demands.Count; i++)
            {
                used += patterns[j].Counts[i] * demands[i].Length;
                pieces += patterns[j].Counts[i];
            }
            double waste = patterns[j].RawLength - used;
            finalObj.SetCoefficient(XF[j], waste - BIG * pieces);
        }
        finalObj.SetMinimization();

        final.Solve();

        var sol = new CuttingStockSolution();
        for (int j = 0; j < patterns.Count; j++)
        {
            int t = (int)XF[j].SolutionValue();
            if (t > 0)
            {
                sol.Patterns.Add(new CuttingPatternResult
                {
                    RawLength = patterns[j].RawLength,
                    TimesUsed = t,
                    ItemCounts = patterns[j].Counts.ToArray()
                });
            }
        }

        return sol;
    }

    public static CuttingStockSolution Phase2(
    IReadOnlyList<RawMaterial> unlimitedStocks,
    IReadOnlyList<DemandItem> demands)
    {
        var solution = new CuttingStockSolution();
        var remaining = demands.Select(d => new DemandItem
        {
            Length = d.Length,
            Quantity = d.Quantity
        }).ToList();


        while (remaining.Any(d => d.Quantity > 0))
        {
            CuttingPatternResult bestPattern = null;
            int maxFilled = -1;

            // Try each unlimited stock
            foreach (var stock in unlimitedStocks)
            {
                // Greedy knapsack for this stock
                var patternCounts = new int[remaining.Count];
                double usedLength = 0;

                // Fill items largest first (optional heuristic)
                var sortedIndices = remaining
                    .Select((d, i) => (d.Length, i))
                    .OrderByDescending(x => x.Length)
                    .Select(x => x.i)
                    .ToArray();

                foreach (var i in sortedIndices)
                {
                    if (remaining[i].Quantity <= 0) continue;

                    int maxQty = (int)((stock.Length - usedLength) / remaining[i].Length);
                    int take = Math.Min(maxQty, remaining[i].Quantity);

                    if (take > 0)
                    {
                        patternCounts[i] = take;
                        usedLength += take * remaining[i].Length;
                    }
                }

                int totalPacked = patternCounts.Sum();
                if (totalPacked > maxFilled)
                {
                    maxFilled = totalPacked;
                    bestPattern = new CuttingPatternResult
                    {
                        RawLength = stock.Length,
                        TimesUsed = 1,
                        ItemCounts = patternCounts
                    };
                }
            }

            if (bestPattern == null || maxFilled == 0)
            {
                throw new Exception("Cannot pack remaining items into any stock!");
            }

            // Check for duplicate patterns before adding
            if (!solution.Patterns.Any(p => SamePattern(p, bestPattern)))
            {
                solution.Patterns.Add(bestPattern);

                // Reduce remaining demand
                for (int i = 0; i < remaining.Count; i++)
                    remaining[i].Quantity -= bestPattern.ItemCounts[i];
            }
            else
            {
                // If pattern is duplicate, just increment TimesUsed on the existing pattern
                var existing = solution.Patterns.First(p => SamePattern(p, bestPattern));
                existing.TimesUsed++;
                for (int i = 0; i < remaining.Count; i++)
                    remaining[i].Quantity -= bestPattern.ItemCounts[i];
            }
        }

        return solution;
    }
    public static bool SamePattern(Pattern a, Pattern b)
    {
        if (a.RawIndex != b.RawIndex) return false;
        for (int i = 0; i < a.Counts.Length; i++)
            if (a.Counts[i] != b.Counts[i]) return false;
        return true;
    }
    public static bool SamePattern(CuttingPatternResult a, CuttingPatternResult b)
    {
        if (a.RawLength != b.RawLength) return false;
        for (int i = 0; i < a.ItemCounts.Length; i++)
            if (a.ItemCounts[i] != b.ItemCounts[i]) return false;
        return true;
    }

    public static Solution Convert(CuttingStockSolution src)
    {
        var sol = new Solution();

        // ----------------------------
        // Raw materials
        // ----------------------------
        sol.stock_len = src.RawMaterials
            .Select(r => r.Length)
            .ToArray();

        sol.stock_limits = src.RawMaterials
            .Select(r => r.MaxCount)
            .ToArray();

        // ----------------------------
        // Demand items
        // ----------------------------
        sol.lengths = src.DemandItems
            .Select(d => d.Length)
            .ToArray();

        sol.demands = src.DemandItems
            .Select(d => (double)d.Quantity)
            .ToArray();

        // ----------------------------
        // Patterns
        // ----------------------------
        int patternCount = src.Patterns.Count;

        sol.patterns = new List<double[]>(patternCount);
        sol.patterns_cnt = new List<double>(patternCount);
        sol.used = new double[patternCount];
        sol.wastes = new double[patternCount];
        sol.pattern_quantity = new double[patternCount];

        for (int i = 0; i < patternCount; i++)
        {
            var p = src.Patterns[i];

            // pattern vector (counts per demand item)
            var pattern = new double[p.ItemCounts.Length];
            for (int j = 0; j < p.ItemCounts.Length; j++)
                pattern[j] = p.ItemCounts[j];

            sol.patterns.Add(pattern);

            sol.patterns_cnt.Add(p.TimesUsed);
            sol.pattern_quantity[i] = p.TimesUsed;

            // compute used length
            double used = 0.0;
            for (int j = 0; j < p.ItemCounts.Length; j++)
                used += p.ItemCounts[j] * sol.lengths[j];

            sol.used[i] = used;
            sol.wastes[i] = p.RawLength - used;
        }

        // ----------------------------
        // Total waste
        // ----------------------------
        sol.total_waste = sol.wastes
            .Zip(sol.pattern_quantity, (w, q) => w * q)
            .Sum();

        return sol;
    }

}

public class Solution
{
    public double[] stock_len = [];
    public int[] stock_limits = [];
    public double[] demands = [];
    public double[] lengths = [];
    public List<double[]> patterns = [];
    public List<double> patterns_cnt = [];
    public double[] wastes = [];
    public double[] used = [];
    public double[] pattern_quantity = [];
    public double total_waste;
}

public class Pattern
{
    public double RawLength;
    public int RawIndex;
    public int[] Counts = [];
}

public class RawMaterial
{
    public double Length { get; set; }
    public int MaxCount { get; set; }
}

public class DemandItem
{
    public int Quantity { get; set; }
    public double Length { get; set; }
}

public class CuttingPatternResult
{
    public double RawLength { get; set; }
    public int TimesUsed { get; set; }

    public int RawIndex;

    public int[] ItemCounts { get; set; } = [];
}

public class CuttingStockSolution
{
    public List<RawMaterial> RawMaterials { get; set; } = [];
    public List<DemandItem> DemandItems { get; set; } = [];
    public List<CuttingPatternResult> Patterns { get; set; } = [];
}

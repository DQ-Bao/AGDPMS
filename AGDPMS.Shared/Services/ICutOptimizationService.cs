using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Google.OrTools.LinearSolver;

namespace AGDPMS.Shared.Services;

public interface ICutOptimizationService
{
    static Solution Solve(double L, double[] lengths, double[] demands)
    {
        Solution solution = new Solution();
        solution.lengths = lengths;
        solution.demands = demands;
        solution.stock_len = L;
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

        // === Demand Satisfaction ===
        double[] satisfied = new double[n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < patterns.Count; j++)
                satisfied[i] += patterns[j][i] * finalX[j];
        }

        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"Item {i + 1} (len {lengths[i]}): demand = {demands[i]}, produced = {satisfied[i]:F0}");
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
    static double[] SolveMaster(List<double[]> patterns, double[] demand, out double[] x, out double objective)
    {
        int m = demand.Length;
        int n = patterns.Count;
        Solver solver = Solver.CreateSolver("GLOP");

        Variable[] vars = new Variable[n];
        for (int j = 0; j < n; j++)
            vars[j] = solver.MakeNumVar(0.0, double.PositiveInfinity, $"x{j}");

        Google.OrTools.LinearSolver.Constraint[] cons = new Google.OrTools.LinearSolver.Constraint[m];
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
    static void SolveFinalIntegerMaster(List<double[]> patterns, double[] demand, double[] lengths, double L, out double[] x, out double objective)
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
            var cons = solver.MakeConstraint(demand[i], double.PositiveInfinity, $"demand_{i}");
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
}

public class Solution
{
    public double stock_len;
    public double[] demands;
    public double[] lengths;
    public List<double[]> patterns;
    public double[] wastes;
    public double[] used;
    public double[] pattern_quantity;
    public double total_waste;
}
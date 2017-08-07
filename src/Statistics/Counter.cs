//USING
using System;
using System.Collections.Generic;
using C5;

//CLASS
class Counter
{
//CONSTRUCTION
    public Counter(string name)
    {
        this.name = name;
    }
//INTERFACE
    public void Extract(Dictionary<string, object> statistics)
    {
        statistics.Add(name + indexCharacter + sumTag, sum);
        statistics.Add(name + indexCharacter + squareSumTag, squareSum);
        statistics.Add(name + indexCharacter + countTag, count);
        statistics.Add(name + indexCharacter + varianceTag, Variance);
        statistics.Add(name + indexCharacter + varianceEstimatorTag, VarianceEstimator);
        statistics.Add(name + indexCharacter + standardDeviationTag, StandardDeviance);
        statistics.Add(name + indexCharacter + standardDeviationEstimatorTag, StandardDevianceEstimator);
        statistics.Add(name + indexCharacter + averageTag, Average);
    }
    public void Add(double item)
    {
        sum += item;
        ++count;
        squareSum += item * item;
    }

//ACCESSORS
    public long Count
    {
        get { return count; }
    }
    public double Sum
    {
        get { return sum; }
    }
    public double SquareSum
    {
        get { return squareSum; }
    }
    public double Variance
    {
        get 
        {
            if (count < 1)
                return 0;
            return squareSum/count - Average * Average; 
        }
    }
    public double VarianceEstimator
    {
            get 
            {
                if (count < 2)
                    return 0;
                return (squareSum - (sum*sum/count))/(count-1); 
            }
    }
    public double StandardDeviance
    {
        get
        {
            return Math.Sqrt(Variance);
        }
    }
    public double StandardDevianceEstimator
    {
        get
        {
            return Math.Sqrt(VarianceEstimator);
        }
    }
    public double Average
    {
        get
        {
            if (count == 0)
                return 0;
            return sum / count;
        }
    }

//DATA
    double sum;
    double squareSum;
    long count;
    string name;
    //CONSTANSTS
    const string averageTag = "Average";
    const string sumTag = "Sum";
    const string squareSumTag = "SquareSum";
    const string countTag = "Count";
    const string varianceTag = "Variance";
    const string varianceEstimatorTag = "VarianceEstimator";
    const string standardDeviationEstimatorTag = "StandardDeviationEstimator";
    const string standardDeviationTag = "StandardDeviation";

    const char indexCharacter = '/';
}
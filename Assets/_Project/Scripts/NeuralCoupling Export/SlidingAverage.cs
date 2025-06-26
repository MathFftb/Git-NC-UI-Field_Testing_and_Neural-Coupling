using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingAverage
{
    private float[] buffer;
    private float sum;
    private int lastIndex;
 
    public SlidingAverage(int num_samples, float initial_value)
    {
        buffer = new float[num_samples];
        lastIndex = 0;
        reset(initial_value);
    }

    public void reset(float value)
    {
        sum=value*buffer.Length;
        for (int i = 0; i < buffer.Length; ++i)
        buffer[i] = value;
    }

    public void pushValue(float value)
    {
        sum -= buffer[lastIndex]; // subtract the oldest sample from the sum
        sum += value; // add the new sample
        buffer[lastIndex] = value; // store the new sample
 
        // advance the index and wrap it around
        lastIndex += 1;
        if (lastIndex >= buffer.Length) 
        {
            lastIndex = 0;
        }
    }
    
    public float getSmoothedValue()
    {
        return sum/buffer.Length;
    }
}

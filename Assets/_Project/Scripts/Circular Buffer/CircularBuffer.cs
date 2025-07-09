using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;



[Serializable]
public class CircularBuffer<T>
{
    private T[] buffer;
    private int head; // head is the oldest added item position;
    private int tail; // added item are added on the tail
    private bool isFull;

    public int Capacity => buffer.Length;
    public int Count // number of items in the buffer
    {
        get
        {
            if (isFull)
                return Capacity;
            if (tail >= head)
                return tail - head;
            return Capacity - head + tail;
        }
    }

    public bool isEmpty => !isFull && head == tail;
    public bool IsFull => isFull;

    StringBuilder sb = new StringBuilder();

    // Constructor
    public CircularBuffer(int size)
    {
        if (size <= 0)
            throw new ArgumentException("error: buffer size cannot be < 0");

        buffer = new T[size];
        head = 0;
        tail = 0;
        isFull = false;
    }

    /// <summary>
    /// Adds an item to the buffer, Overwrites the oldest item if buffer is full
    /// <summary>
    public void Add(T item)
    {
        buffer[tail] = item; // Add latest item on the tail position

        if (isFull) // if the buffer was already full before the addition
        {
            head = (tail + 1) % Capacity;   // move the head forward, 
                                            // the oldest Item has just been overwritten by the newest coming full circle, so the new oldest is the one next to the newest
            tail = (tail + 1) % Capacity; // Update the tail position
        }
        else
        {
            tail = (tail + 1) % Capacity; // Update the tail position
            if (head == tail) // If the tail just now wrapped around to the head with this new addition, we are full
            {
                isFull = true;
            }
        }

    }

    /// <summary>
    /// Removes and returns the oldest item in the buffer.
    /// </summary>
    public T Dequeue()
    {
        if (isEmpty)
            throw new InvalidOperationException("Buffer is empty.");

        var item = buffer[head];
        head = (head + 1) % Capacity;
        isFull = false;
        return item;
    }
    /// <summary>
    /// Returns the oldest item without removing it.
    /// </summary>
    public T PeekOldest()
    {
        if (isEmpty)
            throw new InvalidOperationException("Buffer is empty.");
        return buffer[head];
    }
    /// <summary>
    /// Clears the buffer.
    /// </summary>
    public void Clear()
    {
        Array.Clear(buffer, 0, Capacity);
        head = 0;
        tail = 0;
        isFull = false;
    }

    // Returns the buffer as an array from head to tail
    public T[] ToArray()
    {
        //int count = this.Count; // Uncomment and replace Capacity by count to produce Arrays without null values but of size < Capacity
        T[] arrayBuffer = new T[Capacity];
        int indexBuffer;

        for (int i = 0; i < Capacity; ++i)
        {
            indexBuffer = (head + i) % Capacity;
            arrayBuffer[i] = buffer[indexBuffer];
        }
        return arrayBuffer;
    }

    // Fills caller‑supplied array; returns the number of elements copied.
    public int CopyTo(T[] destination)
    {
        int n = Math.Min(destination.Length, Count);
        for (int i = 0; i < n; i++)
            destination[i] = buffer[(head + i) % Capacity];
        return n;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return (IEnumerator<T>)ToArray().GetEnumerator();
    }

    public override string ToString()
    {
        sb.Clear();
        int count = Count;       
        for (int i = 0; i < count; ++i)
        {
            sb.Append(buffer[(head + i) % Capacity].ToString());
        }
        return sb.ToString();
    }
}

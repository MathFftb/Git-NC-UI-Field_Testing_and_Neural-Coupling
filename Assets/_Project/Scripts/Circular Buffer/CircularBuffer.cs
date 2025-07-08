using System;
using JetBrains.Annotations;
using UnityEngine;




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

    // Constructor
    public CircularBuffer(int size)
    {
        if (size <= 0)
            throw new ArgumentException("error: buffer size cannot be < 0");

        buffer = new T[Capacity];
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
    public T PeekLast()
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

}

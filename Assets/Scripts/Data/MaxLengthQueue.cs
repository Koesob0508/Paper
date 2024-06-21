using System.Collections.Generic;
using System;
using UnityEngine;

public class MaxLengthQueue<T>
{
    private Queue<T> queue;
    private int maxLength;

    public MaxLengthQueue(int maxLength)
    {
        this.queue = new Queue<T>();
        this.maxLength = maxLength;
    }

    public int Count => queue.Count;

    public void Enqueue(T item)
    {
        if (queue.Count >= maxLength)
        {
            // 큐의 최대 길이를 초과하면 예외를 던지거나
            // 또는 가장 오래된 항목을 제거하고 새 항목을 추가합니다.
            queue.Dequeue(); // 가장 오래된 항목을 제거
        }
        queue.Enqueue(item);
    }

    public T Dequeue()
    {
        if (queue.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }
        return queue.Dequeue();
    }

    public T Peek()
    {
        if (queue.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }
        return queue.Peek();
    }

    public bool IsFull()
    {
        return queue.Count >= maxLength;
    }

    public bool IsEmpty()
    {
        return queue.Count == 0;
    }

    public void Clear()
    {
        queue.Clear();
    }

    // List<T>로 변환하는 함수 추가
    public List<T> ToList()
    {
        return new List<T>(queue);
    }

    public T GetLastElement()
    {
        // 큐의 모든 요소를 하나씩 빼면서 마지막 요소를 찾음
        T lastElement = default;
        foreach (T item in queue)
        {
            lastElement = item;
        }
        return lastElement;
    }
}
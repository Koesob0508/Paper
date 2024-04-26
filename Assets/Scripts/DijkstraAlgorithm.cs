using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraAlgorithm : MonoBehaviour
{
    private int verticesCount;
    private int[,] graph;

    public DijkstraAlgorithm(int[,] graph)
    {
        this.graph = graph;
        verticesCount = graph.GetLength(0);
    }
    
    private int FindMinDistance(int[] distance, bool[] shortestPathTreeSet)
    {
        int minDistance = int.MaxValue;
        int minIndex = -1;

        for (int v = 0; v < verticesCount; v++)
        {
            if (!shortestPathTreeSet[v] && distance[v] <= minDistance)
            {
                minDistance = distance[v];
                minIndex = v;
            }
        }

        return minIndex;
    }

    public void FindShortestPath(int source)
    {
        int[] distance = new int[verticesCount];
        bool[] shortestPathTreeSet = new bool[verticesCount];

        for(int i=0; i < verticesCount; i++)
        {
            distance[i] = int.MaxValue;
            shortestPathTreeSet[i] = false;
        }

        distance[source] = 0;

        for(int count = 0; count < verticesCount - 1; count++)
        {
            int u = FindMinDistance(distance, shortestPathTreeSet);
            shortestPathTreeSet[u] = true;

            for(int v = 0; v < verticesCount; v++)
            {
                if (!shortestPathTreeSet[v] && graph[u, v] != 0 &&
                    distance[u] != int.MaxValue && distance[u] + graph[u, v] < distance[v])
                {
                    distance[v] = distance[u] + graph[u, v];
                }
            }
        }
    }
}

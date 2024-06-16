using log4net.Util;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class MapManager
{
    private GameObject root;
    public GameObject Root { get { return root; } }
    private EnvData currentEnv;

    private GameObject agent = null;
    private GameObject end;

    private GameObject floor;
    private GameObject wall;
    private GameObject[] walls = new GameObject[4];
    private GameObject north { get { return walls[0]; } }
    private GameObject west { get { return walls[1]; } }
    private GameObject south { get { return walls[2]; } }
    private GameObject east { get { return walls[3]; } }

    private GameObject obstacle;
    private GameObject guide;
    private List<GameObject> obstacles;
    private List<GameObject> guides;

    private List<int> currentObstacles;
    private List<int> currentGuides;
    private List<int> emptyList;
    private List<int> startList;
    private List<int> endList;

    private int startIndex;
    private int endIndex;

    private int[,] graph = new int[25, 25];
    private int[,] next = new int[25, 25];

    public GameObject Target
    {
        get
        {
            if (currentGuides.Count == 0)
            {
                return null;
            }
            else
            {
                return guides[currentGuides[0]];
            }
        }
    }

    public MapManager(GameObject _root)
    {
        root = _root;
    }

    public void Init()
    {
        wall = new GameObject { name = "wall" };
        wall.transform.SetParent(root.transform);

        obstacle = new GameObject { name = "obstacle" };
        obstacle.transform.SetParent(root.transform);

        guide = new GameObject { name = "guide" };
        guide.transform.SetParent(root.transform);

        currentObstacles = new List<int>();
        currentGuides = new List<int>();
        emptyList = new List<int>();
        startList = new List<int>();
        endList = new List<int>();

        InitializeLevel();
        StartEpisode();
    }

    public void InitializeLevel()
    {
        if (agent != null) return;

        obstacles = new List<GameObject>();
        guides = new List<GameObject>();

        #region wall
        Managers.Resource.Instantiate("Prefabs/Floor", root.transform);

        walls[0] = Managers.Resource.Instantiate("Prefabs/Obstacle", root.transform); // north
        walls[1] = Managers.Resource.Instantiate("Prefabs/Obstacle", root.transform); // west
        walls[2] = Managers.Resource.Instantiate("Prefabs/Obstacle", root.transform); // south
        walls[3] = Managers.Resource.Instantiate("Prefabs/Obstacle", root.transform); // east

        north.transform.localScale = new Vector3(6, 1, 1);
        north.transform.localPosition = new Vector3(0, 0.5f, 3);
        north.name = "wall_north";

        east.transform.localScale = new Vector3(1, 1, 6);
        east.transform.localPosition = new Vector3(3, 0.5f, 0);
        east.name = "wall_east";

        south.transform.localScale = new Vector3(6, 1, 1);
        south.transform.localPosition = new Vector3(0, 0.5f, -3);
        south.name = "wall_south";

        west.transform.localScale = new Vector3(1, 1, 6);
        west.transform.localPosition = new Vector3(-3, 0.5f, 0);
        west.name = "wall_west";

        foreach (var obj in walls)
        {
            obj.transform.SetParent(wall.transform);
        }
        #endregion

        #region obstacle & marker
        for (int idxY = -2; idxY < 3; idxY++)
        {
            for (int idxX = -2; idxX < 3; idxX++)
            {
                GameObject o = Managers.Resource.Instantiate("Prefabs/Obstacle", root.transform);
                GameObject g = Managers.Resource.Instantiate("Prefabs/Guide", root.transform);

                o.transform.localPosition = new Vector3(idxX, 0.5f, idxY);
                g.transform.localPosition = new Vector3(idxX, 0.25f, idxY);

                obstacles.Add(o);
                guides.Add(g);

                o.transform.SetParent(obstacle.transform);
                g.transform.SetParent(guide.transform);

                o.SetActive(false);
                g.SetActive(false);
            }
        }
        #endregion

        agent = Managers.Resource.Instantiate("Prefabs/Agent", root.transform);
        agent.GetComponent<FirstAgent>().mapManager = this;
        agent.SetActive(false);
    }

    public void StartEpisode(bool isTrainingMode = true)
    {
        if (isTrainingMode)
        {
            var values = Managers.Data.TrainingEnvs.Values.ToList();
            int randomIndex = Random.Range(0, values.Count);

            GenerateLevel(values[randomIndex]);
        }
    }

    public void EndEpisode()
    {
        agent.SetActive(false);
        agent.GetComponent<Agent>().EndEpisode();
        
        currentObstacles.Clear();
        emptyList.Clear();
        startList.Clear();
        endList.Clear();

        foreach(var o in obstacles)
        {
            o.SetActive(false);
        }

        foreach(var g in guides)
        {
            g.SetActive(false);
        }

        StartEpisode();
    }

    public void GenerateLevel(EnvData _env)
    {
        graph = InitializeGraph();

        ReadEnv(_env);

        SetObstacle();
        SetAgentTransform();
        SetEndPosition();

        CalculatePath(graph);
        FindPath(startIndex, endIndex);
        ShowGuides();
    }

    private void ReadEnv(EnvData _env)
    {
        currentEnv = _env;
        

        int obstacleIndex = 0;

        foreach (List<int> row in _env.Env)
        {
            foreach (int status in row)
            {
                switch(status)
                {
                    case -1:
                        currentObstacles.Add(obstacleIndex);
                        break;
                    case 0:
                        emptyList.Add(obstacleIndex);
                        break;
                    case 1:
                        startList.Add(obstacleIndex);
                        break;
                    case 2:
                        endList.Add(obstacleIndex);
                        break;
                    default:
                        break;
                }

                obstacleIndex++;
            }
        }
    }

    private void SetObstacle()
    {
        foreach (var _ in currentObstacles)
        {
            obstacles[_].SetActive(true);
            UpdateGraph(graph, _, int.MaxValue);
        }
    }

    private void SetAgentTransform()
    {
        if(startList.Count != 0)
        {
            int randomIndex = Random.Range(0, startList.Count);
            startIndex = startList[randomIndex];
        }
        else
        {
            int randomIndex = Random.Range(0, emptyList.Count);
            startIndex = emptyList[randomIndex];
            
            emptyList.Remove(startIndex);
        }

        agent.transform.position = obstacles[startIndex].transform.position;

        int _ = Random.Range(0, currentEnv.Directions.Count);
        Vector3 startDirection = Vector3.zero;
        if (currentEnv.Directions.Count != 0)
        {
            switch (currentEnv.Directions[_])
            {
                case 0:
                    break;
                case 1:
                    startDirection = new Vector3(0f, 90f, 0f);
                    break;
                case 2:
                    startDirection = new Vector3(0f, 180f, 0f);
                    break;
                case 3:
                    startDirection = new Vector3(0f, 270f, 0f);
                    break;
            }
        }

        agent.transform.rotation = Quaternion.Euler(startDirection);

        agent.SetActive(true);
    }

    private void SetEndPosition()
    {
        if(endList.Count != 0)
        {
            int randomIndex = Random.Range(0, endList.Count);
            endIndex = endList[randomIndex];
        }
        else
        {
            int randomIndex = Random.Range(0, emptyList.Count);
            endIndex = emptyList[randomIndex];

            emptyList.Remove(endIndex);
        }

        end = guides[endIndex];
        // end.transform.rotation = Quaternion.Euler(90f, 0, 0);
        //end.SetActive(true);
    }

    private int[,] InitializeGraph()
    {
        int[,] result = new int[25, 25];

        // 기본적으로 모두 연결되어 있지 않음
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                if (i == j)
                {
                    result[i, j] = 0;
                }
                else
                {
                    result[i, j] = int.MaxValue;
                }
            }
        }

        for (int index = 0; index < 25; index++)
        {
            UpdateGraph(result, index, 1);
        }

        return result;
    }

    private void UpdateGraph(int[,] _graph, int _index, int _value)
    {
        if (_index - 5 >= 0)
        {
            _graph[_index, _index - 5] = _value;
            _graph[_index - 5, _index] = _value;
        }

        if (_index + 5 <= 24)
        {
            _graph[_index, _index + 5] = _value;
            _graph[_index + 5, _index] = _value;
        }

        if (_index % 5 != 0)
        {
            _graph[_index, _index - 1] = _value;
            _graph[_index - 1, _index] = _value;
        }

        if (_index % 5 != 4)
        {
            _graph[_index, _index + 1] = _value;
            _graph[_index + 1, _index] = _value;
        }
    }

    private void CalculatePath(int[,] _graph)
    {
        int[,] dist = new int[25, 25];
        dist = (int[,])_graph.Clone();
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                //dist[i, j] = graph[i, j];

                if (graph[i, j] != int.MaxValue && i != j)
                {
                    next[i, j] = j;
                }
                else
                {
                    next[i, j] = -1; // 경로가 없는 경우 -1
                }
            }
        }

        for (int k = 0; k < 25; k++)
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (dist[i, k] != int.MaxValue && dist[k, j] != int.MaxValue && dist[i, k] + dist[k, j] < dist[i, j])
                    {
                        dist[i, j] = dist[i, k] + dist[k, j];
                        next[i, j] = next[i, k];
                    }
                }
            }
        }
    }

    private void FindPath(int startIndex, int endIndex)
    {
        currentGuides.Clear();

        int inter = next[startIndex, endIndex];
        while (inter != -1)
        {
            currentGuides.Add(inter);
            inter = next[inter, endIndex];
        }
    }

    private void ShowGuides()
    {

        for(int i = 0; i < currentGuides.Count; i++)
        {
            if(i == currentGuides.Count-1)
            {
                guides[currentGuides[i]].transform.rotation = Quaternion.Euler(-90f, 0, 0);
            }
            else
            {
                // 방향 계산...
                Vector3 current = guides[currentGuides[i]].transform.position;
                Vector3 next = guides[currentGuides[i+1]].transform.position;
                Vector3 _ = (next - current).normalized;
                Vector2 direction = new Vector2(_.x, _.z);
                float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

                guides[currentGuides[i]].transform.rotation = Quaternion.Euler(0, angle, 0);
            }

            guides[currentGuides[i]].SetActive(true);
        }
    }

    public void OnArrival()
    {
        Target.SetActive(false);
        currentGuides.RemoveAt(0);

        if(currentGuides.Count == 0)
        {
            EndEpisode();
        }
    }
}
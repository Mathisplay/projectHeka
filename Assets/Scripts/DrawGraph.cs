using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawGraph : MonoBehaviour
{
    struct Connection
    {
        public int id;
        public char travelBy;
        public Connection(int a, char b)
        {
            id = a;
            travelBy = b;
        }
    };
    class Point
    {
        public int id; // id of a point
        public int depth; // depth of a point in a tree
        public List<Connection> canGoTo; // connection to id by c
        public List<char> unusedConnections; // connections that are still avaliable or connections to itself
        public bool isEnd; // is the node an end node
        public float x;
        public float y;
        public float z;
        public GameObject obj; // graph node diode object reference
        public Point(int difficulty)
        {
            id = 0;
            depth = 0;
            canGoTo = new List<Connection>();
            List<char> letters = new List<char>();
            for (int i = 0; i < difficulty; i++)
            {
                letters.Add((char)('a' + i));
            }
            unusedConnections = letters;
            isEnd = false;
            x = 0.0f;
            y = 0.0f;
        }
        public void SetId(int a)
        {
            id = a;
        }
        public void SetDepth(int a)
        {
            depth = a;
        }
        public void SetCoordinates(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    public GameObject diodePrefab; // state object
    public Material endNodeMaterial; // end state material
    public GameObject linePrefab; // connection object
    public GameObject puzzlePlaceholderPrefab; // puzzle object
    public GameObject text; // display text object
    public GameObject solutionBlock; // correct answer block object
    public GameObject puzzleBlock; // user answer block object
    public GameObject electricity; // spark effect object
    public GameObject playerData;

    private List<Point> points = new List<Point>(); // list of graph nodes and their connections to the nearest points
    private List<int> sizes = new List<int>(); // sizes of each graph depth level for nicer displaying later
    private string path = ""; // correct answer
    private Text textVal; // display text text parameter
    private int sparkLocation; // node at which the spark effect stays at currently
    private GameObject currentSpark = null; // created spark effect reference
    private List<GameObject> socketList; // list of created sockets for puzzles
    private List<Renderer> solutionBricks; // list of created puzzle blocks for changing their type later
    private bool animatingChecking = false; // check if the spark effect is playing
    private PlayerData data;

    void Start()
    {
        data = playerData.GetComponent<PlayerData>();
        textVal = text.GetComponent<Text>(); // get text text parameter
        CreateGraph(2, 5);
    }
    void CreateGraph(int complexity, int numberOfBlocks)
    {
        points = new List<Point>(); // reset the list of graph nodes and their connections to the nearest points
        solutionBricks = new List<Renderer>();
        path = "";
        foreach (Transform child in transform) // destroy all child objects (puzzle blocks, answer blocks, graph connections, sockets, spark)
        {
            Destroy(child.gameObject);
        }
        addPoint(complexity);
        for (int i = 0; i < numberOfBlocks; i++)
        {
            AddRandomPoint(complexity);
        }
        ConnectRandomPointIteration(true);
        ConnectRandomPointIteration(false);
        PlaceEndpoint();
        GetGraphDimensions();
        PlaceDiodes();
        PutLines();
        GenerateRandomPath(complexity);
        DrawAnswer();
        AddPuzzleBlocks();
        SpawnSpark();
    }
    void addPoint(int difficulty) // add a point to a graph
    {
        points.Add(new Point(difficulty));
    }
    void AddRandomPoint(int difficulty) // create a random point and connect it to the graph
    {
        addPoint(difficulty);
        points[points.Count - 1].SetId(points.Count - 1);
        int next;
        while (true) // find a node with a free connection
        {
            next = Random.Range(0, (points.Count - 1));
            if(points[next].unusedConnections.Count != 0)
            {
                break;
            }
        }
        int nextLetter = Random.Range(0, points[next].unusedConnections.Count);
        char letter = points[next].unusedConnections[nextLetter];
        points[points.Count - 1].SetDepth(points[next].depth + 1);
        points[next].unusedConnections.RemoveAt(nextLetter);
        points[next].canGoTo.Add(new Connection(points.Count - 1, letter));
    }
    void PlaceEndpoint() // set one of the furthest points as and endpoint
    {
        int furthest = -1;
        int id = 0;
        for(int i = 0; i < points.Count; i++)
        {
            if(points[i].depth > furthest)
            {
                furthest = points[i].depth;
                id = i;
            }
        }
        points[id].isEnd = true;
    }
    void ConnectRandomPointIteration(bool toBack) // if possible connects a point to a point 1 higher (true) / 1 deeper (false)
    {
        List<int> avaliablePoints;
        int nextLetter;
        for (int i = 1; i < points.Count; i++) // connect every point except the point with id = 0 (depth = 0)
        {
            avaliablePoints = new List<int>();
            if(points[i].unusedConnections.Count == 0) // skip a point if it already has all connections
            {
                continue;
            }
            nextLetter = Random.Range(0, points[i].unusedConnections.Count);
            for(int j = 0; j < points.Count; j++) // put all possible points to connect in a list
            {
                if(toBack == true && points[j].depth == points[i].depth - 1)
                {
                    avaliablePoints.Add(j);
                }
                else if(toBack == false && points[j].depth == points[i].depth + 1)
                {
                    avaliablePoints.Add(j);
                }
            }
            if(avaliablePoints.Count != 0) // if there is a node to connect to, pick a random one and connect a point to it
            {
                points[i].canGoTo.Add(new Connection(avaliablePoints[Random.Range(0, avaliablePoints.Count)], points[i].unusedConnections[nextLetter]));
                points[i].unusedConnections.RemoveAt(nextLetter);
            }
        }
    }
    void GetGraphDimensions() // get the sizes of every node depth group
    {
        sizes = new List<int>();
        int counter;
        for(int i = 0; i < points.Count; i++)
        {
            counter = 0;
            for(int j = 0; j < points.Count; j++)
            {
                if(points[j].depth == i)
                {
                    counter++;
                }
            }
            if(counter == 0)
            {
                break;
            }
            sizes.Add(counter);
        }
    }
    void DrawArrow(Vector2 a, Vector2 b, LineRenderer l) // magical calculations to draw arrows, now draws sharp lines
    {
        Vector2 u = (b - a) * 0.05f;
        //a += u * 2.0f;
        //b -= u * 2.0f;
        b -= u;
        Vector2 s = b - u * 2.0f;
        float degree1 = -90.0f * Mathf.Deg2Rad;
        //float degree2 = 90.0f * Mathf.Deg2Rad;
        Vector2 u1 = new Vector2(Mathf.Cos(degree1) * u.x - Mathf.Sin(degree1) * u.y, Mathf.Sin(degree1) * u.x - Mathf.Cos(degree1) * u.y) / 2.0f;
        //Vector2 u2 = new Vector2(Mathf.Cos(degree2) * u.x - Mathf.Sin(degree2) * u.y, Mathf.Sin(degree2) * u.x - Mathf.Cos(degree2) * u.y) / 2.0f;
        l.SetPosition(0, a - u1);
        //l.SetPosition(1, s - u1);
        //l.SetPosition(2, s - u1 - u1);
        //l.SetPosition(3, b - u1);
        //l.SetPosition(4, s - u2 - u1);
        l.SetPosition(1, s - u1);
    }
    void DrawPlaceholders(Vector2 a, Vector2 b, int idFrom, int idTo) // put sockets on the board
    {
        Vector2 u = (b - a) * 0.05f;
        float degree1 = -90.0f * Mathf.Deg2Rad;
        Vector2 u1 = new Vector2(Mathf.Cos(degree1) * u.x - Mathf.Sin(degree1) * u.y, Mathf.Sin(degree1) * u.x - Mathf.Cos(degree1) * u.y) / 2.0f;
        a += u * 5f;
        a -= u1;
        var obj = Instantiate(puzzlePlaceholderPrefab); // create socket object and put it on the correct position on the board
        obj.transform.parent = gameObject.transform;
        obj.transform.position = gameObject.transform.position;
        obj.transform.rotation = gameObject.transform.rotation;
        obj.transform.localScale = new Vector3(0.001f, 0.001f, 0.0005f);
        obj.transform.localPosition = new Vector3(a.x, a.y, 0.0108f);
        SocketType st = obj.GetComponent<SocketType>(); // set socket properties
        st.from = idFrom;
        st.to = idTo;
        socketList.Add(obj);
    }
    void PlaceDiodes() // place all node diodes on the board
    {
        int width = sizes.Count;
        int heightCounter;
        socketList = new List<GameObject>();
        for (int i = 0; i < width + 1; i++)
        {
            heightCounter = 0;
            for (int j = 0; j < points.Count; j++)
            {
                if(points[j].depth == i)
                {
                    var obj = Instantiate(diodePrefab); // create diode object and put it on the correct position on the board
                    obj.transform.parent = gameObject.transform;
                    obj.transform.position = gameObject.transform.position;
                    obj.transform.rotation = gameObject.transform.rotation;
                    obj.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0005f);
                    obj.transform.localPosition = new Vector3(-0.015f + 0.03f * (float)(heightCounter + 1) / (float)(sizes[i] + 1), -0.025f + 0.05f * (float)(i + 1) / (float)(width + 1), 0.0105f);
                    if(points[j].isEnd == true) // use different material if the node is an end node
                    {
                        obj.GetComponent<Renderer>().material = endNodeMaterial;
                    }
                    points[j].SetCoordinates(-0.015f + 0.03f * (float)(heightCounter + 1) / (float)(sizes[i] + 1), -0.025f + 0.05f * (float)(i + 1) / (float)(width + 1), 0.0105f);
                    points[j].obj = obj;
                    heightCounter++;
                }
            }
        }
        
    }
    void PutLines() // draw all lines and sockets on the board
    {
        foreach (var item in points)
        {
            for (int i = 0; i < item.canGoTo.Count; i++)
            {
                var obj = Instantiate(linePrefab); // create line object and put it on the correct position on the board
                obj.transform.parent = gameObject.transform;
                obj.transform.position = gameObject.transform.position;
                obj.transform.rotation = gameObject.transform.rotation;
                obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                obj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0105f);
                DrawArrow(new Vector2(item.x, item.y), new Vector2(points[item.canGoTo[i].id].x, points[item.canGoTo[i].id].y), obj.GetComponent<LineRenderer>()); // draw an arrow
                DrawPlaceholders(new Vector2(item.x, item.y), new Vector2(points[item.canGoTo[i].id].x, points[item.canGoTo[i].id].y), item.id, item.canGoTo[i].id); // put a socket on an arrow
            }
        }
    }
    void GenerateRandomPath(int difficulty) // generate a random automata and a random word accepted by it
    {
        path = "";
        int steps = 0; // steps taken from the start
        int pointId = 0; // current node selected by the algorithm
        int next;

        while(points[pointId].isEnd == false || steps < difficulty + 2) // go around until minimal length has been acquired and reached the end node
        {
            if(steps > difficulty * difficulty + 1) // if maximal length has been reached, reset parameters
            {
                steps = 0;
                pointId = 0;
                path = "";
            }
            next = Random.Range(0, difficulty); // random letter
            char c = ' ';
            bool selected = false; // is there anywhere else to go using that letter
            for (int i = 0; i < points[pointId].canGoTo.Count; i++)
            {
                if (points[pointId].canGoTo[i].travelBy == (char)('a' + next))
                {
                    c = points[pointId].canGoTo[i].travelBy;
                    pointId = points[pointId].canGoTo[i].id;
                    selected = true;
                    break;
                }
            }
            if(selected == false) // goes to itself
            {
                c = (char)('a' + next);
            }
            path += c;
            steps++;
        }
    }
    void DrawAnswer() // display a word accepted by the automata as blocks
    {
        float len = -0.0016f * (float)path.Length / 2.0f;
        for (int i = 0; i < path.Length; i++)
        {
            var obj = Instantiate(solutionBlock);
            obj.transform.parent = gameObject.transform;
            obj.transform.position = gameObject.transform.position;
            obj.transform.rotation = gameObject.transform.rotation;
            obj.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0005f);
            obj.transform.localPosition = new Vector3(-0.015f, len + 0.0016f * i + 0.0008f, 0.0098f);
            PuzzleType p = obj.GetComponentInChildren<PuzzleType>(); // set block properties
            p.type = path[i] - 'a';
            p.UpdateMaterial();
            solutionBricks.Add(obj.GetComponentInChildren<Renderer>());
        }
    }
    void AddPuzzleBlocks() // add puzzle blocks
    {
        int counter = 0; // how many blocks we need
        foreach (var item in points)
        {
            counter += item.canGoTo.Count;
        }
        float len = -0.0016f * (float)counter / 2.0f;
        counter = 0;
        foreach (var item in points)
        {
            for (int i = 0; i < item.canGoTo.Count; i++)
            {
                var obj = Instantiate(puzzleBlock); // create puzzle block object and put it on the correct position on the board
                obj.transform.parent = gameObject.transform;
                obj.transform.position = gameObject.transform.position;
                obj.transform.rotation = gameObject.transform.rotation;
                obj.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
                obj.transform.localPosition = new Vector3(0.015f, len + 0.0016f * counter + 0.0008f, 0.012f);
                PuzzleType p = obj.GetComponentInChildren<PuzzleType>(); // set block properties
                p.type = item.canGoTo[i].travelBy - 'a';
                p.UpdateMaterial();
                counter++;
            }
        }
    }
    public void Display(string s) // display a message on the text object
    {
        //textVal.text = s;
    }
    void SpawnSpark() // spawn a spark effect on the first node
    {
        if (currentSpark != null)
        {
            Destroy(currentSpark);
        }
        var obj = Instantiate(electricity); // create spark object and put it on the correct position on the board
        obj.transform.parent = gameObject.transform;
        obj.transform.position = gameObject.transform.position;
        obj.transform.rotation = gameObject.transform.rotation;
        obj.transform.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);
        obj.transform.localPosition = new Vector3(points[0].x, points[0].y, 0.011f);
        currentSpark = obj;
    }
    public void CheckSolution() // reset the spark object and run the solution checking if possible
    {
        if (animatingChecking == false)
        {
            animatingChecking = true;
            SpawnSpark();
            sparkLocation = 0;
            StartCoroutine(GoNext(points[0]));
        }
    }
    IEnumerator GoNext(Point b) // check if the solution is correct
    {
        ResetTintSolution();
        for (int i = 0; i < socketList.Count; i++) // check if every transition socket has been filled
        {
            SocketType soc = socketList[i].GetComponent<SocketType>();
            if (soc.val == -1)
            {
                animatingChecking = false;
                yield break;
            }
        }
        for (int i = 0; i < socketList.Count; i++) // check if the automata is deterministic
        {
            for (int j = 0; j < socketList.Count; j++)
            {
                var obj1 = socketList[i].GetComponent<SocketType>();
                var obj2 = socketList[j].GetComponent<SocketType>();
                if (i != j && obj1.from == obj2.from && obj1.val == obj2.val)
                {
                    animatingChecking = false;
                    yield break;
                }
            }
        }
        bool solved = true;
        for(int i = 0; i < path.Length; i++) // check if the automata accepts each letter in a word
        {
            int found = 2;
            for(int j = 0; j < socketList.Count; j++)
            {
                SocketType soc = socketList[j].GetComponent<SocketType>();
                if (soc.val == path[i] - 'a' && soc.from == sparkLocation) // found a valid path
                {
                    b = points[soc.to];
                    sparkLocation = soc.to;
                    found = 1;
                    break;
                }
                else if(soc.val == -1 && soc.from == sparkLocation) // found an empty space
                {
                    found = 0;
                    break;
                }
            }
            if (found == 0) // empty space
            {
                SpawnSpark();
                solved = false;
                break;
            }
            else if (found == 2) // loops to itself
            {
                Material mat = currentSpark.GetComponent<Renderer>().material;
                Material matBackup = mat;
                mat = endNodeMaterial;
                TintSolution(i);
                yield return new WaitForSecondsRealtime(0.25f);
                mat = matBackup;
                yield return new WaitForSecondsRealtime(0.25f);
            }
            else // animate spark path from point a to point b
            {
                float t = 0.0f;
                //textVal.text = i.ToString();
                Point start = b;
                while (t < 1.0f)
                {
                    t += Time.deltaTime;
                    Vector3 newPos = Vector3.Lerp(currentSpark.transform.localPosition, new Vector3(b.x, b.y, currentSpark.transform.localPosition.z), t);
                    currentSpark.transform.localPosition = newPos;
                    yield return new WaitForSecondsRealtime(0.011f);
                }
                Material mat = currentSpark.GetComponent<Renderer>().material;
                Material matBackup = mat;
                mat = endNodeMaterial;
                TintSolution(i);
                yield return new WaitForSecondsRealtime(0.5f);
                mat = matBackup;
            }
        }
        if(solved == true && points[sparkLocation].isEnd == true) // automata puzzle solved
        {
            //textVal.text = "GG";
        }
        else // automata puzzle failed
        {
            //textVal.text = "MEH";
            ResetTintSolution();
            SpawnSpark();
        }
        yield return new WaitForSecondsRealtime(1.0f);
        //textVal.text = "";
        if (solved == true && points[sparkLocation].isEnd == true) // automata puzzle solved
        {
            data.GetPoints(10);
            CreateGraph(2, 5);
        }
        animatingChecking = false; // make checking the solution avaliable
    }
    void TintSolution(int id)
    {
        solutionBricks[id].material.SetFloat("Vector1_3118346", 1);
    }
    void ResetTintSolution()
    {
        for(int i = 0; i < solutionBricks.Count; i++)
        {
            solutionBricks[i].material.SetFloat("Vector1_3118346", 0);
        }
    }
}

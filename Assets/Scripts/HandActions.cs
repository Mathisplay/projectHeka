using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.XR;
using System.Globalization;
using Unity.Barracuda;
using UnityEngine.UI;

public class HandActions : MonoBehaviour
{
    public InputDeviceCharacteristics controllerChatacteristicsLeft; // left hand device characteristics
    public InputDeviceCharacteristics controllerChatacteristicsRight; // right hand device characteristics
    public NNModel modelAsset; // keras pre-learned model
    public GameObject text; // debug text object
    public List<GameObject> spellsPrefabs; // prefabs of all of the spells avaliable

    private GameObject drawObjectL; // object on the left hand used to draw spells
    private GameObject drawObjectR; // object on the right hand used to draw spells
    private InputDevice targetDeviceL; // left hand input device
    private InputDevice targetDeviceR; // right hand input device
    private PlayerData data; // todo
    private LineRenderer lines; // line renderer for spells
    private Material lineMaterial; // material of the line renderer
    private Color savedLineColor; // line color placeholder before changing it to restore it later
    private bool drawingProcess; // is color animation not playing
    private bool drawingCompleted; // have any hand just finished drawing a spell
    private Text textVal; // text value of text object
    public GameObject boardAsset; // board parent asset
    private DrawGraph board = null; // board main script

    private char nowDrawing; // indicates which hand is now drawing the spell

    private Model runtimeModel; // keras model object
    private IWorker worker; // ML model worker
    void Start()
    {
        nowDrawing = '0';
        runtimeModel = ModelLoader.Load(modelAsset); // load the ML model
        textVal = text.GetComponent<Text>();
        float[,,] pic = new float[64, 64, 3]; // empty array for drawings
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    pic[i, j, k] = 1.0f;
                }
            }
        }
        Initialize();
    }
    void Initialize()
    {
        drawingProcess = true;
        drawingCompleted = true;
        List<InputDevice> devicesL = new List<InputDevice>(); // prepare a list for all input devices
        List<InputDevice> devicesR = new List<InputDevice>(); // prepare a list for all input devices
        InputDevices.GetDevicesWithCharacteristics(controllerChatacteristicsLeft, devicesL); // get all input devices with left controller characteristics
        InputDevices.GetDevicesWithCharacteristics(controllerChatacteristicsRight, devicesR); // get all input devices with righe controller characteristics
        lines = GameObject.Find("SpellDrawer").GetComponent<LineRenderer>(); // get a spell line drawer object
        lineMaterial = GameObject.Find("SpellDrawer").GetComponent<Renderer>().material; // get a spell line drawer material
        savedLineColor = lineMaterial.GetColor("_EmissionColor"); // save a line drawer material color
        drawObjectL = GameObject.Find("ColliderDotLeft"); // get left hand drawing object
        drawObjectR = GameObject.Find("ColliderDotRight"); // get right hand drawing object
        if (devicesL.Count > 0) // get first devices with chosen characteristics as controllers
        {
            targetDeviceL = devicesL[0];
        }
        if (devicesR.Count > 0)
        {
            targetDeviceR = devicesR[0];
        }
        board = boardAsset.GetComponent<DrawGraph>(); // get a main board script
    }
    void UpdateManaAnimation()
    {
        if (targetDeviceL.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            
        }
    }
    private void Draw()
    {
        if (targetDeviceL.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) && (nowDrawing == '0' || nowDrawing == 'L')) // run if the player is not drawing or drawing with a left hand
        {
            if (triggerValue > 0.9f && drawingProcess == true)
            {
                nowDrawing = 'L'; // set drawing hand as left
                lines.positionCount += 1; // add a point to a spell drawing
                Vector3 point = drawObjectL.transform.position; // get the position of a drawing object
                lines.SetPosition(lines.positionCount - 1, point); // save the position
                drawingCompleted = false;
            }
            else if(drawingCompleted == false) // just completed drawing
            {
                //reset everything, flatten the 3D drawing to 2D and play the spell color animation
                drawingCompleted = true;
                drawingProcess = false;
                FlattenAndProcessDrawing();
                StartCoroutine(CleanDrawing());
                nowDrawing = '0';
            }
        }
        if (targetDeviceR.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue1) && (nowDrawing == '0' || nowDrawing == 'R')) // run if the player is not drawing or drawing with a right hand
        {
            if (triggerValue1 > 0.9f && drawingProcess == true)
            {
                nowDrawing = 'R'; // set drawing hand as right
                lines.positionCount += 1; // add a point to a spell drawing
                Vector3 point = drawObjectR.transform.position; // get the position of a drawing object
                lines.SetPosition(lines.positionCount - 1, point); // save the position
                drawingCompleted = false;
            }
            else if (drawingCompleted == false) // just completed drawing
            {
                //reset everything, flatten the 3D drawing to 2D and play the spell color animation
                drawingCompleted = true;
                drawingProcess = false;
                FlattenAndProcessDrawing();
                StartCoroutine(CleanDrawing());
                nowDrawing = '0';
            }
        }
    }
    private bool FlattenAndProcessDrawing()
    {
        try
        {
            //find a line approximating the plane
            float a, b;
            Vector3[] positions = new Vector3[lines.positionCount];
            lines.GetPositions(positions);
            float s1 = 0.0f, s2 = 0.0f, s3 = 0.0f, s4 = 0.0f;
            foreach (Vector3 item in positions)
            {
                s1 += item.x * item.z;
                s2 += item.x;
                s3 += item.z;
                s4 += item.x * item.x;
            }
            s1 = s1 * (float)lines.positionCount - s2 * s3;
            s4 *= (float)lines.positionCount;
            s4 -= s2 * s2;
            a = s1 / s4;
            b = (s3 - a * s2) / (float)lines.positionCount;
            //find edge cases
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
            foreach (Vector3 item in positions)
            {
                if (item.x < minX)
                {
                    minX = item.x;
                }
                if (item.x > maxX)
                {
                    maxX = item.x;
                }
                if (item.y < minY)
                {
                    minY = item.y;
                }
                if (item.y > maxY)
                {
                    maxY = item.y;
                }
                if (item.z < minZ)
                {
                    minZ = item.z;
                }
                if (item.z > maxZ)
                {
                    maxZ = item.z;
                }
            }
            float distanceHorizontal = (float)Math.Sqrt(Math.Pow(maxX - minX, 2) + Math.Pow(maxZ - minZ, 2)); // get width
            float distanceVertical = maxY - minY; // get height
            float[,,] pic = new float[64, 64, 3]; // clear an array
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        pic[i, j, k] = 0.0f;
                    }
                }
            }
            foreach (Vector3 item in positions) // lerp position of a point to array
            {
                int distanceHoriz = Math.Min(Mathf.FloorToInt((float)Math.Sqrt(Math.Pow(maxX - item.x, 2) + Math.Pow(maxZ - item.z, 2)) / distanceHorizontal * 64.0f), 63);
                int distanceVert = Math.Min(Mathf.FloorToInt((maxY - item.y) / distanceVertical * 64.0f), 63);
                for (int i = 0; i < 3; i++)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        for (int k = -2; k < 3; k++)
                        {
                            pic[Mathf.Min(Mathf.Max(distanceVert + j, 0), 63), Mathf.Min(Mathf.Max(distanceHoriz + k, 0), 63), i] = 255.0f;
                        }
                    }
                }
            }
            int typeOfDrawing = TypeOfDrawing(pic); // identify drawn image
            //textVal.text = lines.positionCount.ToString() + " " + typeOfDrawing.ToString();
            if (lines.positionCount > 30 && typeOfDrawing != -1)
            {
                switch (typeOfDrawing) // change line color according to image drawn
                {
                    case 0:
                        lineMaterial.SetColor("_EmissionColor", Color.red);
                        Instantiate(spellsPrefabs[1], new Vector3(maxX - (maxX - minX) / 2, maxY - (maxY - minY) / 2, maxZ - (maxZ - minZ) / 2), Quaternion.identity);
                        break;
                    case 1:
                        lineMaterial.SetColor("_EmissionColor", Color.green);
                        Instantiate(spellsPrefabs[2], new Vector3(maxX - (maxX - minX) / 2, maxY - (maxY - minY) / 2, maxZ - (maxZ - minZ) / 2), Quaternion.identity);
                        break;
                    case 2:
                        lineMaterial.SetColor("_EmissionColor", Color.yellow);
                        board.CheckSolution();
                        break;
                    case 3:
                        Instantiate(spellsPrefabs[0], new Vector3(maxX - (maxX - minX) / 2, maxY - (maxY - minY) / 2, maxZ - (maxZ - minZ) / 2), Quaternion.identity);
                        lineMaterial.SetColor("_EmissionColor", Color.blue);
                        break;
                }
            }
        }
        catch
        {
            return false;
        }
        return true;
    }
    IEnumerator CleanDrawing() // reset line renderer after a delay
    {
        yield return new WaitForSecondsRealtime(0.5f);
        lines.positionCount = 0;
        drawingProcess = true; // color animation is not playing
        lineMaterial.SetColor("_EmissionColor", savedLineColor);
    }
    private int TypeOfDrawing(float[,,] pic) // identify the image
    {
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, runtimeModel);
        var tensor = new Tensor(1, 64, 64, 3);
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    tensor[0, i, j, k] = pic[i, j, k];
                }
            }
        }
        worker.Execute(tensor);
        var a = worker.PeekOutput("dense_1");
        float maxVal = -1.0f;
        int maxId = -1;
        for(int i = 0; i < 4; i++)
        {
            if(a.ToReadOnlyArray()[i] > maxVal)
            {
                maxVal = a.ToReadOnlyArray()[i];
                maxId = i;
            }
        }
        //textVal.text = maxVal.ToString();
        worker.Dispose();
        if (maxVal < 0.7f)
        {
            maxId = -1;
        }
        return maxId;
    }
    void Update()
    {
        if (!targetDeviceL.isValid || !targetDeviceR.isValid) // if a controller is found
        {
            Initialize();
        }
        else // check if the player is drawing
        {
            Draw();
        }
    }
}

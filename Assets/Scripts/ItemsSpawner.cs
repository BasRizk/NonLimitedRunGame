using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsSpawner : MonoBehaviour
{
    public GameObject boostSpherePrefab;
    public GameObject coinPrefab;
    public GameObject ironBallPrefab;
    public GameObject bombPrefab;
    public float lineSpacing;
    public int collectiblesMaxNum;
    public int obstaclesMaxNum;
    public int step = 10;
    public int firstPassLimit = 10;
    private GameObject gamePlaneGameObject;
    private EndlessPlane gamePlane;
    private float zPosReset;
    private float zBackLimit;

    private int lastAccessedCurrentPlaneIndex;
    private Queue<GameObject> coinsPool;
    private Queue<GameObject> boostSpheresPool;
    private Queue<GameObject> ironBallsPool;
    private Queue<GameObject> bombsPool;
    private bool isPaused;
    private GameObject lastItemAdded;

    // Start is called before the first frame update
    void Awake()
    {
        initSpace();
    }
    void Start()
    {
        initConnections();
    }

    private void initSpace()
    {
        coinsPool = new Queue<GameObject>();
        boostSpheresPool = new Queue<GameObject>();
        ironBallsPool = new Queue<GameObject>();
        bombsPool = new Queue<GameObject>();
    }


    private void initConnections()
    {
        isPaused = false;
        gamePlane = GameObject.FindGameObjectWithTag("EndlessPlane")
                    .GetComponent<EndlessPlane>();
        zPosReset = (int) (gamePlane.getzPlaneHeight()*0.9);
        zBackLimit = gamePlane.getZBackLimit();
        createSomeItems("Collectibles", gamePlane.getCurrentPlaneTransform());
        lastAccessedCurrentPlaneIndex = gamePlane.getCurrentPlaneIndex();
        firstPassLimit = 0;

        createSomeItems("ALL", gamePlane.getNextPlaneTransforms(1)[0]);
        // lastItemAdded = createItemsOnRow(zPosReset,
        //                  gamePlane.getNextPlaneTransforms(1)[0],
        //                  "All");
        Debug.Log("z PosOriginMin, BackLimit = " + zPosReset + ", " + zBackLimit);
    }

    public void destroy()
    {
        while (coinsPool.Count > 0)
        {
            Destroy(coinsPool.Dequeue());
        }

        while (boostSpheresPool.Count > 0)
        {
            Destroy(boostSpheresPool.Dequeue());
        }

        while (ironBallsPool.Count > 0)
        {
            Destroy(ironBallsPool.Dequeue());
        }

        while (bombsPool.Count > 0)
        {
            Destroy(bombsPool.Dequeue());
        }

    }

    public void reinit()
    {
        destroy();
        initSpace();
        initConnections();
    }

    void FixedUpdate()
    {
        if (isPaused)
        {
            return;
        }

        int currentPlan = gamePlane.getCurrentPlaneIndex();
        // Debug.Log("lastAccessedCurrentPlanNum #" + lastAccessedCurrentPlaneIndex);
        if (lastAccessedCurrentPlaneIndex != currentPlan)
        {
            // Debug.Log("About to create more collectibles as new plane is current");
            createSomeItems("All", gamePlane.getNextPlaneTransforms(1)[0]);
            lastAccessedCurrentPlaneIndex = currentPlan;
        }

        // if(lastItemAdded != null && lastItemAdded.transform.position.z - step < zPosReset) {
        //     Debug.Log("lastItemTransformed.transform.position.z " + lastItemAdded.transform.position.z);
        //     Debug.Log("zPosReset " + zPosReset);
        //     lastItemAdded = createItemsOnRow(zPosReset,
        //                  gamePlane.getNextPlaneTransforms(1)[0],
        //                  "All");
        // }

    }

    private void createSomeItems(string type, Transform targetTransform)
    {

        for (float z = firstPassLimit; z < zPosReset; z += step)
        {
            if(Random.Range(0,10) > 8) {
                continue;
            }
            createItemsOnRow(z, targetTransform, type);
        }
    }

    private GameObject createItemsOnRow(float z, Transform targetTransform, string type)
    {
        GameObject lastAddedObj = null;
        HashSet<float> pastXs = new HashSet<float>();
        int numOfItemsInRow = Random.Range(1, 4);
        for (int i = 0; i < numOfItemsInRow; i++)
        {
            float x = 0;
            switch (Random.Range(-1, 2))
            {
                case -1: x = -1 * lineSpacing; break;
                case 1: x = 1 * lineSpacing; break;
                case 0: x = 0; break;
            }
            if(pastXs.Contains(x)) {
                Debug.Log("item already spawned at this position.");
                i--;
                continue;
            } else {
                Debug.Log("item to be spawn here, never spawned before.");
                Debug.Log(pastXs.ToString());
                pastXs.Add(x);
            }

            if (type == "Collectibles")
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        Debug.Log("Coin added");
                        lastAddedObj = addToItemsPool(x, z, "Coin", targetTransform);
                        break;

                    case 1:
                        Debug.Log("BoostSphere added");
                        lastAddedObj = addToItemsPool(x, z, "BoostSphere", targetTransform);
                        // targetPrefab = boostSpherePrefab; 
                        break;
                }
            }
            else if (type == "Obstacles")
            {
                // Obstacles
                switch (Random.Range(0, 2))
                {
                    case 0:
                        Debug.Log("IronBall added");
                        lastAddedObj = addToItemsPool(x, z, "IronBall", targetTransform);
                        break;

                    case 1:
                        Debug.Log("Bomb Added");
                        lastAddedObj = addToItemsPool(x, z, "Bomb", targetTransform);
                        break;
                }
            }
            else
            {
                switch (Random.Range(0, 4))
                {
                    case 0:
                        Debug.Log("IronBall added");
                        lastAddedObj = addToItemsPool(x, z, "IronBall", targetTransform);
                        break;
                    case 1:
                        Debug.Log("Bomb added");
                        lastAddedObj = addToItemsPool(x, z, "Bomb", targetTransform);
                        break;
                    case 2:
                        Debug.Log("Coin added");
                        lastAddedObj = addToItemsPool(x, z, "Coin", targetTransform);
                        type = "Obstacles";
                        break;
                    case 3:
                        Debug.Log("BoostSphere added");
                        lastAddedObj = addToItemsPool(x, z, "BoostSphere", targetTransform);
                        type = "Obstacles";
                        break;
                }
            }
        }

        return lastAddedObj;
    }

    private GameObject addToItemsPool(float x, float z, string type, Transform targetTransform)
    {
        Debug.Log("Spawning at Z,X value " + x + "," + z + " of type " + type );

        z = z + targetTransform.position.z - targetTransform.localScale.z * 5;
        // Debug.Log("Value of Z for new object = " + z);
        Queue<GameObject> objPool = null;
        GameObject objPrefab = null;

        switch (type)
        {
            case "Coin":
                objPool = coinsPool;
                objPrefab = coinPrefab;
                break;
            case "BoostSphere":
                objPool = boostSpheresPool;
                objPrefab = boostSpherePrefab;
                break;
            case "IronBall":
                objPool = ironBallsPool;
                objPrefab = ironBallPrefab;
                break;
            case "Bomb":
                objPool = bombsPool;
                objPrefab = bombPrefab;
                break;
        }

        if (objPool == null || objPrefab == null)
        {
            Debug.LogError("UnMatched type of Item is to be generated.");
            return null;
        }

        GameObject objHolder = null;
        if (objPool.Count > 0 && objPool.Peek().transform.position.z < zBackLimit)
        {
            // Debug.Log("Recycling old gameobject.");
            objHolder = coinsPool.Dequeue();
            objHolder.transform.Translate(x, 0.0f, z);
        }
        else
        {
            // Debug.Log("Creating new gameobject");
            objHolder = Instantiate(objPrefab, new Vector3(x, 0.0f, z), Quaternion.identity);
        }
        // Debug.Log("Current Transform z = " + targetTransform.position.z);
        objHolder.transform.SetParent(targetTransform, true);
        objHolder.SetActive(true);
        // objHolder.GetComponent<Collider>().enabled = true;
        coinsPool.Enqueue(objHolder);

        return objHolder;
    }

    public void unPause()
    {
        isPaused = false;
    }

    public void pause()
    {
        isPaused = true;
    }
}

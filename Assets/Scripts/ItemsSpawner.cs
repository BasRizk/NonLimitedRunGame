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

        createSomeItems("Collectibles", gamePlane.getNextPlaneTransforms(1)[0]);
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
    }

    private void createItemsOnRow(float z, Transform targetTransform, string type)
    {
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
                i--;
                continue;
            } else {
                pastXs.Add(x);
            }

            if (type == "Collectibles")
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        addToItemsPool(x, z, "Coin", targetTransform);
                        break;

                    case 1:
                        addToItemsPool(x, z, "BoostSphere", targetTransform);
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
                        addToItemsPool(x, z, "IronBall", targetTransform);
                        break;

                    case 1:
                        addToItemsPool(x, z, "Bomb", targetTransform);
                        break;
                }
            }
            else
            {
                switch (Random.Range(0, 4))
                {
                    case 0:
                        addToItemsPool(x, z, "IronBall", targetTransform);
                        break;
                    case 1:
                        addToItemsPool(x, z, "Bomb", targetTransform);
                        break;
                    case 2:
                        addToItemsPool(x, z, "Coin", targetTransform);
                        type = "Obstacles";
                        break;
                    case 3:
                        addToItemsPool(x, z, "BoostSphere", targetTransform);
                        type = "Obstacles";
                        break;
                }
            }
        }

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

    private void addToItemsPool(float x, float z, string type, Transform targetTransform)
    {
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
            return;
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
            objHolder = Instantiate(objPrefab, new Vector3(x, 0, z), Quaternion.identity);
        }
        // Debug.Log("Current Transform z = " + targetTransform.position.z);
        objHolder.transform.SetParent(targetTransform, true);
        objHolder.SetActive(true);
        // objHolder.GetComponent<Collider>().enabled = true;
        coinsPool.Enqueue(objHolder);
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

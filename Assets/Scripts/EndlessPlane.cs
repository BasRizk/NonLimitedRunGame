using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessPlane : MonoBehaviour
{
    public float originalSpeed;
    public float speed;
    public GameObject planePrefab;
    public GameObject garbage;
    private float zBackLimit;
    private int currentPlaneIndex;
    private float zPosOrigMax;
    private float zPlaneHeight;
    private GameObject[] planes;
    private bool isPaused;
    private float unAccessedTravelledDistance;
    private float updateDeltaSpeed;

    // Start is called before the first frame update

    void Awake()
    {
        init();
    }

    public void init()
    {
        reset();
        speed = originalSpeed;
        isPaused = false;
        // planes = GameObject.FindGameObjectsWithTag("Plane");
        planes = new GameObject[3];
        float zValue = 0;
        for(int i = 0; i< planes.Length; i++) {
            planes[i] = Instantiate(planePrefab, new Vector3(0,-1,zValue), Quaternion.identity);
            planes[i].transform.SetParent(transform);
            zValue += planes[i].transform.localScale.z*10;
        }

        zBackLimit = -1 * planes[0].transform.localScale.z * 10.0f * 0.75f;
        zPosOrigMax = planes[planes.Length - 1].transform.position.z;
        zPlaneHeight = planes[0].transform.localScale.z * 10;
        currentPlaneIndex = 0;
        unAccessedTravelledDistance = 0;
    }

     public void reset()
    {
        if(planes == null) {
            return;
        }
        for(int i = 0; i< planes.Length; i++) {
            Destroy(planes[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused)
        {
            return;
        }

        float toBeMovedAmount = Time.deltaTime * speed;
        Vector3 movement = new Vector3(0.0f, 0.0f, -1 * toBeMovedAmount);
        unAccessedTravelledDistance += toBeMovedAmount;
        for (int i = 0; i < planes.Length; i++)
        {
            updatePlane(i, movement);
        }
    }

    void FixedUpdate()
    {
        if (isPaused)
        {
            return;
        }
        float changeAmount = Time.deltaTime * updateDeltaSpeed;
        if (updateDeltaSpeed > 0)
        {
            speed += changeAmount;
            updateDeltaSpeed -= changeAmount;
            if (updateDeltaSpeed < 0)
            {
                speed += updateDeltaSpeed;
                updateDeltaSpeed = 0;
            }
        }
        else if (updateDeltaSpeed < 0)
        {
            speed += changeAmount;
            updateDeltaSpeed -= changeAmount;
            if (updateDeltaSpeed > 0)
            {
                speed += updateDeltaSpeed;
                updateDeltaSpeed = 0;
            }
        }
    }

    private void updatePlane(int planeIndex, Vector3 movement)
    {

        // Debug.Log("About to update plane.");
        // Debug.Log("planes length = " + planes.Length);
        GameObject obj = planes[planeIndex];
        // Debug.Log("Objet plane is in hold.");

        if (obj.transform.position.z > zBackLimit)
        {
            // Debug.Log("obj not surpassed zBackLimit yet.");
            obj.transform.Translate(movement);

        }
        else
        {
            // Debug.Log("obj plane to be reset.");
            int pastPlaneIndex = (currentPlaneIndex > 0) ? currentPlaneIndex - 1 : planes.Length - 1;
            currentPlaneIndex++;
            if (currentPlaneIndex > planes.Length - 1)
            {
                currentPlaneIndex = 0;
            }

            // Debug.Log("Current PLANE # " + currentPlaneIndex);

            float latestZEdge = planes[pastPlaneIndex].transform.position.z +
                                (planes[pastPlaneIndex].transform.localScale.z * 10);

            obj.transform.position =
                new Vector3(obj.transform.position.x, obj.transform.position.y, latestZEdge);

            while (obj.transform.childCount > 0)
            {
                obj.transform.GetChild(0).SetParent(garbage.transform);
            }
            // Debug.Log("Zpos (afterUpdate) = " + planes[pastPlaneIndex - 1].transform.position.z);

        }
    }

    public float getUnAccessedTravelledDistance()
    {
        if (unAccessedTravelledDistance > 1)
        {
            int unAccessedTravelledDistanceTmp = (int)unAccessedTravelledDistance;
            unAccessedTravelledDistance = 0;
            return unAccessedTravelledDistanceTmp;
        }
        return 0;
    }

    public float getZPosOrigMax()
    {
        return zPosOrigMax;
    }
    public float getzPlaneHeight()
    {
        return zPlaneHeight;
    }
    public float getZBackLimit()
    {
        return zBackLimit;
    }
    public Transform getCurrentPlaneTransform()
    {
        return planes[currentPlaneIndex].transform;
    }

    public int getCurrentPlaneIndex()
    {
        return currentPlaneIndex;
    }

    public Transform getNextPlaneTransform()
    {
        Debug.Log("NextPlaneTransform Index #" + (currentPlaneIndex + 1) % planes.Length);
        return planes[(currentPlaneIndex + 1) % planes.Length].transform;
    }

    public Transform[] getNextPlaneTransforms(int numOfTransforms)
    {
        // Debug.Log("getNextPlaneTransforms:: currentPlaneIndex #" + currentPlaneIndex);
        if (numOfTransforms > planes.Length)
        {
            numOfTransforms = planes.Length;
        }
        Transform[] transforms = new Transform[numOfTransforms];
        for (int i = 1; i <= numOfTransforms; i++)
        {
            transforms[i - 1] = planes[(currentPlaneIndex + i) % planes.Length].transform;
            // Debug.Log("getNextPlaneTransforms:: currentPlaneIndex #" + (currentPlaneIndex + i) % planes.Length);

        }
        return transforms;
    }

    public void unPause()
    {
        isPaused = false;
    }
    public void pause()
    {
        isPaused = true;
    }

    public void updateSpeed(float updateSpeed)
    {
        if (updateDeltaSpeed == 0)
        {
            updateDeltaSpeed = updateSpeed;
        }
        else
        {
            speed += updateDeltaSpeed;
            updateDeltaSpeed = updateSpeed;
        }
    }
}

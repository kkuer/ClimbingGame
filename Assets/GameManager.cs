using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Leap;
using Unity.VisualScripting;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<GameObject> wallPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> usedWalls = new List<GameObject>();
    public GameObject currentWall = null;
    public Transform parentObj;
    public Slider playerSlider;

    public float fallspeed;
    public float fallspeedMin;

    private Vector3 lastChildPosition;
    public bool gameStarted;

    public bool grabbingLeft;
    public bool grabbingRight;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (playerSlider != null && playerSlider.value <= 0f)
        {
            fallspeed = 0f;
        }

        if (gameStarted)
        {
            if (grabbingLeft == false && grabbingRight == false)
            {
                fallspeed += Time.deltaTime * 0.2f;
                parentObj.Translate(0, fallspeed * Time.deltaTime, 0);
            }
            else
            {
                fallspeed = fallspeedMin;
            }
        }
    }

    private void Start()
    {
        lastChildPosition = Vector3.zero;
        gameStarted = false;

        grabbingLeft = false;
        grabbingRight = false;
    }

    public void SpawnNewWall()
    {
        Vector3 newPos = new Vector3(currentWall.transform.position.x, currentWall.transform.position.y + 5, currentWall.transform.position.z);

        var availableWalls = wallPrefabs.Except(usedWalls).ToList();

        if (availableWalls.Count == 0)
        {
            usedWalls.Clear();
            availableWalls = wallPrefabs.ToList();
        }

        GameObject newWall = Instantiate(availableWalls[Random.Range(0, availableWalls.Count)], newPos, Quaternion.identity, parentObj);
        usedWalls.Add(newWall);

        currentWall.GetComponent<Wall>().old = true;
        currentWall = newWall;
    }

    public void moveWorld(GameObject grabTarget)
    {
        Vector3 positionDelta = grabTarget.transform.position - lastChildPosition;

        parentObj.position += new Vector3 (positionDelta.x, positionDelta.y, 0f);
        lastChildPosition = grabTarget.transform.position;
    }
}

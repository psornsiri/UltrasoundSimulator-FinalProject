using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;


public class GameMenuScript : MonoBehaviour
{
    public GameObject userObject;
    public GameObject targetObject;
    public GameObject body_model;
    public GameObject fetus;
    public GameObject[] baby_volumes;
    public bool babyMove = false;
    public bool enableCollision = true;
    public Text BabyModelText;

    private LogicScript logic;
    private SocketClient socket;
    private ProbeCollision probe;
    private GameSettings gameSettings;

    private int random_time, random_turn;
    private float timeMove;
    private Quaternion defaultBaby;

    // Start is called before the first frame update
    void Start()
    {
        defaultBaby = fetus.transform.rotation;
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        socket = GameObject.FindGameObjectWithTag("Client").GetComponent<SocketClient>();
        probe = GameObject.FindGameObjectWithTag("Probe").GetComponent<ProbeCollision>();
        gameSettings = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameSettings>();

        socket.pos1 = true;
        BabyModelText.text = "23";
    }

    // Update is called once per frame
    void Update()
    {
        // settings for obejct collision
        if (enableCollision)
        {
            probe.collisionThreshold = (float)0.19;
            //gameSettings.EnableCollision(userObject, body_model);
        }
        else
        {
            probe.collisionThreshold = (float)0.0;
            //gameSettings.DisableCollision(userObject, body_model);
        }

        // baby moving during game mode
        if (babyMove)
        {
            if (timeMove == 0)
            {
                random_time = Random.Range(1, 4);
                random_turn = Random.Range(1, 3);
            }
            timeMove += Time.deltaTime;
            if (timeMove < random_time)
            {
                if (random_turn == 1)
                {
                    fetus.transform.GetChild(0).RotateAround(fetus.transform.GetChild(0).position, fetus.transform.GetChild(1).up, 12f * Time.deltaTime);
                }
                else
                {
                    fetus.transform.GetChild(0).RotateAround(fetus.transform.GetChild(0).position, fetus.transform.GetChild(1).up, -12f * Time.deltaTime);
                }
            }
            else
            {
                timeMove = 0;
            }
            
        }
        else
        {
            timeMove = 0;
        }
    }

    public void ResetGame()
    {
        babyMove = false;
        fetus.transform.rotation = defaultBaby;
        int baby_GA = RandomBabyGA();
        RandomBabyPosition();
        BabyModelText.text = baby_GA.ToString();
        logic.resetButton.SetActive(false);

        logic.prevTimeCount = 0;
        logic.targetComplete = false;
    }

    private void FlipOtherSide(Vector3 initPosition)
    {
        fetus.transform.RotateAround(fetus.transform.GetChild(0).position, fetus.transform.GetChild(0).up, 180f);
        // probe.transform.RotateAround(fetus.transform.GetChild(0).position, fetus.transform.GetChild(0).up, -180f);
        fetus.transform.position = initPosition;
    }

    private void FlipUpsideDown(Vector3 initPosition)
    {
        fetus.transform.RotateAround(fetus.transform.GetChild(0).position, fetus.transform.GetChild(0).forward, 180f);
        fetus.transform.position = initPosition;
    }

    private void RandomBabyPosition()
    {
        int random_pos = Random.Range(1, 5);
        //int[] random_num = new int[] { 1, 3 };
        //int random_id = Random.Range(0, 2);
        //int random_pos = random_num[random_id];

        Vector3 initPosition = fetus.transform.position;

        if (random_pos == 1)
        {
            socket.pos1 = true;
            socket.pos2 = socket.pos3 = socket.pos4 = false;
            //targetObject.transform.position = new Vector3((float)0.084, (float)0.04, (float)-0.034);
        }
        else if (random_pos == 2)
        {
            FlipOtherSide(initPosition);
            socket.pos2 = true;
            socket.pos1 = socket.pos3 = socket.pos4 = false;
        }
        else if (random_pos == 3)
        {
            FlipUpsideDown(initPosition);
            socket.pos3 = true;
            socket.pos1 = socket.pos2 = socket.pos4 = false;
        }
        else if (random_pos == 4)
        {
            FlipUpsideDown(initPosition);
            FlipOtherSide(initPosition);
            socket.pos4 = true;
            socket.pos1 = socket.pos2 = socket.pos3 = false;
            //targetObject.transform.position = new Vector3((float)-0.137, (float)0.1, (float)0.05);
        }
    }

    private int RandomBabyGA()
    {
        int random_GA = Random.Range(0, 5);
        foreach (GameObject volume in baby_volumes)
        {
            volume.SetActive(false);
        }
        baby_volumes[random_GA].SetActive(true);

        return random_GA + 21;
    }
}

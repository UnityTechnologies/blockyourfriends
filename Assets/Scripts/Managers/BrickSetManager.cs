using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockYourFriends.Utility.SO;
using BlockYourFriends.Utility;
using BlockYourFriends.UI;
using BlockYourFriends.Ads;
using Unity.Netcode;
using BlockYourFriends.Multiplayer.ngo;
using BlockYourFriends.Gameplay;

namespace BlockYourFriends
{
    //This class manages the brick sets in a session, both setting up and managing the clearing/counting/analytics for each individual brick sequence
    //Setup is done by pulling a list of levels (and their arrangmeents in the form of a Scriptable Object) and using that information to populate the level
    //A level is cleared when the timer (unique to each level) expires or the blocks are all destroyed and the next one is set up
    //A session ends when the number of levels played matches the game length

    public class BrickSetManager : NetworkBehaviour
    {
        [Header("Default layout settings")]
        [SerializeField] private Vector3 startingRotation;
        [SerializeField] private Vector3 startingScale;
        [Tooltip("Number of levels/layouts per game session")]
        [SerializeField] private int gameLength = 5;
        public int maxRowLength = 5;
        public int maxNumberOfRows = 5;
        public int currentLevel = 0;

        [Header("References")]
        [Tooltip("Game Object to hold existing blocks (and be destroyed on level changes")]
        [SerializeField] private NetworkObject layoutParentPrefab = null;
        [Tooltip("Randomize the order of the layouts")]
        [SerializeField] private bool randomizeLevels = false;
        [Tooltip("Brick Layouts to be used in game rotation")]
        [SerializeField] private BrickLayout[] layouts;
        [Tooltip("Ball spawner is responsible of spawning ball on the host's side and spawn to a clients.")]
        [SerializeField] private NetworkBallSpawner ballSpawner;

        private bool rotation = false;
        private float rotationRate = 0f;
        private LevelAdvanceUI levelAdvanceUI;
        private NetworkObject layoutParent;
        private int bricksThisLevel;


        public Transform layoutParentFromClient;

        //Layout Order
        private List<int> levelOrder = new List<int>();

        //Analytics
        private string levelAdvanceReason = "N/A"; //String to store hte Analytics reason for level advance (bricks gone or time out)

        [Header("Brick Count")]
        private int brickCount = 0;
        [Tooltip("Set the number of bricks remaining to force level advance, Default 0")]
        public int brickThreshold = 0;
        private int numberOfPowerUps = 0;

        [Header("Level Transition Settings")]
        public float timeToDisplayLevelText = 5f;
        public string endGameText = "That's all folks!";
        public float endGameTimer = 5f;

        [Header("Dev Settings")]
        [SerializeField] private bool fastLevelEnd;

        //Slot locations for new blocks to be instantiated
        private Vector3[] row1SpawnLoc = new[] { new Vector3(-4, 4, 0), new Vector3(-2, 4, 0), new Vector3(0, 4, 0), new Vector3(2, 4, 0), new Vector3(4, 4, 0) };
        private Vector3[] row2SpawnLoc = new[] { new Vector3(-4, 2, 0), new Vector3(-2, 2, 0), new Vector3(0, 2, 0), new Vector3(2, 2, 0), new Vector3(4, 2, 0) };
        private Vector3[] row3SpawnLoc = new[] { new Vector3(-4, 0, 0), new Vector3(-2, 0, 0), new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(4, 0, 0) };
        private Vector3[] row4SpawnLoc = new[] { new Vector3(-4, -2, 0), new Vector3(-2, -2, 0), new Vector3(0, -2, 0), new Vector3(2, -2, 0), new Vector3(4, -2, 0) };
        private Vector3[] row5SpawnLoc = new[] { new Vector3(-4, -4, 0), new Vector3(-2, -4, 0), new Vector3(0, -4, 0), new Vector3(2, -4, 0), new Vector3(4, -4, 0) };

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        void Start()
        {
            levelAdvanceUI = LevelAdvanceUI.Instance;
        }

        public void StartNewGame()
        {
            if (layouts.Length <= 0 || layouts[0] == null)
            {
                Debug.Log("There are no layouts set... Create a Layout Scriptable Object and assign it");
                return;
            }
            else
            {
                NewGame();
            }
        }

        public void SetGameLength(int newGameLength)
        {
            gameLength = newGameLength;
        }

        //Run at the start of a new game session (Setup)
        public void NewGame()
        {
            MusicManager.Instance.PlayPlayList();
            rotation = false;
            SetLevelOrder();
            currentLevel = 0;
            SetupBricks();
            if (layouts[currentLevel].rotateAssembly)
            {
                rotation = true;
                rotationRate = layouts[currentLevel].rotationRate;
            }
            ballSpawner.StartNewLevel();  //allocate Starting balls
            StartCoroutine(CountDownLevelTime());
        }

        //Sets the next level up and resets parameters
        private void SetNextLevel()
        {
            numberOfPowerUps = 0;
            rotation = false;
            currentLevel++;
            brickCount = 0;
            SetupBricks();
            if (layouts[levelOrder[currentLevel]].rotateAssembly)
            {
                rotation = true;
                rotationRate = layouts[levelOrder[currentLevel]].rotationRate;
            }
            StartCoroutine(CountDownLevelTime());
        }

        //Countdown display update
        private IEnumerator CountDownLevelTime()
        {
            yield return new WaitForSeconds(layouts[currentLevel].levelTime - 5f); //start countdown at 5 seconds remaining
            SetLevelRemainingTime(5);
            yield return new WaitForSeconds(1f); //start countdown at 5 seconds remaining
            SetLevelRemainingTime(4);
            yield return new WaitForSeconds(1f); //start countdown at 5 seconds remaining
            SetLevelRemainingTime(3);
            yield return new WaitForSeconds(1f); //start countdown at 5 seconds remaining
            SetLevelRemainingTime(2);
            yield return new WaitForSeconds(1f); //start countdown at 5 seconds remaining
            SetLevelRemainingTime(1);
            yield return new WaitForSeconds(1f); //start countdown at 5 seconds remaining
            levelAdvanceReason = "Time Out";
            StartCoroutine(LevelAdvance());
        }

        private void SetLevelRemainingTime(int timer)
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                levelAdvanceUI.SetLevelRemainingTime(timer);
            else
                SetLevelRemainingTime_ClientRpc(timer);
        }

        [ClientRpc]
        private void SetLevelRemainingTime_ClientRpc(int timer)
        {
            levelAdvanceUI.SetLevelRemainingTime(timer);
        }

        //Advance the level (either ot the next one or session end)
        private IEnumerator LevelAdvance()
        {
            BYFAnalytics.Instance.ReportLevelEnd(layouts[levelOrder[currentLevel]].layoutName, levelAdvanceReason);
            BYFAnalytics.Instance.ReportBricksDestroyed(layouts[levelOrder[currentLevel]].layoutName, bricksThisLevel, (bricksThisLevel - brickCount) / bricksThisLevel, layouts[currentLevel].levelTime);
#if !UNITY_EDITOR
            fastLevelEnd = false;
#endif
            if ((currentLevel >= layouts.Length - 1) || (currentLevel >= gameLength - 1) || fastLevelEnd)
            {
                GameManager.Instance.AdvanceLevel(); //Freeze player input and destroy balls in play
                SetLevelName(endGameText);
                //End Game Analytics Reporting
                BYFAnalytics.Instance.EndOfGameReporting(gameLength);
                yield return new WaitForSeconds(endGameTimer);
                DisableTransitionUI();
                //FindObjectOfType<UIManager>().MainMenu();
                if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                {
                    //UIManager.Instance.MainMenu();
                    //AdsManager.Instance.ShowAd();
                    //NewGame(); //Should probably be called on start from Main Menu... need to refactor this a bit to make more sense ;)
                }
                else
                {
                    //if (IsHost)
                    //    ShowAd_ServerRpc();

                    InGameRunner.Instance.EndGame();
                }
                GameManager.Instance.EndGame();
                Destroy(layoutParent.gameObject);
                //Debug.Log("No Next Level");
            }
            else
            {
                GameManager.Instance.AdvanceLevel();
                ballSpawner.StartNewLevel(); //Re-allocate Starting balls
                SetLevelName(layouts[levelOrder[currentLevel + 1]].layoutName);
                SetNextLevel();
                yield return new WaitForSeconds(timeToDisplayLevelText);
                DisableTransitionUI();
                GameManager.Instance.StartGame();
            }
        }

        [ServerRpc]
        private void ShowAd_ServerRpc()
        {
            //ShowAd_ClientRpc();
        }

        [ClientRpc]
        private void ShowAd_ClientRpc()
        {
            //AdsManager.Instance.ShowAd();
        }

        private void SetLevelName(string nextLevelName)
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                levelAdvanceUI.SetLevelName(nextLevelName);
            else
                SetLevelName_ClientRpc(nextLevelName);
        }

        [ClientRpc]
        private void SetLevelName_ClientRpc(string nextLevelName)
        {
            levelAdvanceUI.SetLevelName(nextLevelName);
        }

        private void DisableTransitionUI()
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                levelAdvanceUI.DisableTransitionUI();
            else
                DisableTransitionUI_ClientRpc();
        }

        [ClientRpc]
        private void DisableTransitionUI_ClientRpc()
        {
            levelAdvanceUI.DisableTransitionUI();
        }

        //Establish the level order (randomize or linear)
        private void SetLevelOrder()
        {
            levelOrder.Clear();
            List<int> tempLayoutList = new List<int>();
            for (int i = 0; i < layouts.Length; i++)
            {
                tempLayoutList.Add(i);
            }
            if (randomizeLevels)
            {
                for (int i = layouts.Length; i > 0; i--)
                {
                    int selectedLayout = Random.Range(0, i);
                    //Debug.Log(selectedLayout);
                    levelOrder.Add(tempLayoutList[selectedLayout]);
                    tempLayoutList.Remove(tempLayoutList[selectedLayout]);
                }
            }
            else
            {
                levelOrder = tempLayoutList;
            }
        }

        //Set up the bricklayout
        private void SetupBricks()
        {
            if (layoutParent == null)
            {
                layoutParent = Instantiate(layoutParentPrefab);

                if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer)
                    layoutParent.Spawn();
            }
            else
            {
                for (int i = 0; i < layoutParent.transform.childCount; i++)
                {
                    Destroy(layoutParent.transform.GetChild(i).gameObject);
                }
                layoutParent.transform.localScale = Vector3.one;
            }

            SetupRow(row1SpawnLoc, 0);
            SetupRow(row2SpawnLoc, 1);
            SetupRow(row3SpawnLoc, 2);
            SetupRow(row4SpawnLoc, 3);
            SetupRow(row5SpawnLoc, 4);
            layoutParent.name = layouts[levelOrder[currentLevel]].layoutName;
            layoutParent.transform.Rotate(0, 0, layouts[levelOrder[currentLevel]].zRotation);
            float equalScale = layouts[levelOrder[currentLevel]].layoutScale;
            layoutParent.transform.localScale = new Vector3(equalScale, equalScale, equalScale);
            bricksThisLevel = brickCount;

        }

        //Reduce the brick count and track it against bricks spawned
        public void ReduceBrickCount()
        {
            brickCount--;
            //Debug.Log("Bricks Left:" + brickCount);
            if (brickCount <= brickThreshold)
            {
                StopAllCoroutines(); //stop the existing timer
                levelAdvanceReason = "Bricks Destroyed";
                StartCoroutine(LevelAdvance());
            }
        }

        //Setup an individual row of bricks
        private void SetupRow(Vector3[] spawnLocs, int rowNumber)
        {
            for (int i = 0; i < maxRowLength; i++)
            {
                if (layouts[levelOrder[currentLevel]].brickLayout[rowNumber].rowBricks[i] == null)
                {
                    continue; //go to the next brick for a null object (can have blanks in the pattern :)).
                }
                brickCount++;
                NetworkObject brickToSpawn = Instantiate(layouts[levelOrder[currentLevel]].brickLayout[rowNumber].rowBricks[i], spawnLocs[i], Quaternion.identity, layoutParent.transform);
                brickToSpawn.name = "row" + rowNumber + "Brick" + i;

                    if (numberOfPowerUps < layouts[levelOrder[currentLevel]].maxPowerUps)
                    {
                        if (Random.Range(0, 1)<=layouts[levelOrder[currentLevel]].powerUpChance)
                        {
                        brickToSpawn.GetComponent<Brick>().SetPowerUp();
                        numberOfPowerUps++;
                        }
                    }

                if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost)
                    brickToSpawn.Spawn();
            }
        }

        //Run each frame to rotate the assembly if enabled
        private void Rotate()
        {
            if (layoutParent != null)
                layoutParent.transform.Rotate(0, 0, rotationRate * Time.deltaTime);
        }

        // Update is called once per frame
        void Update()
        {
            if (((GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost) ||
                  GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer) && rotation)
            {
                Rotate();
            }
        }
    }
}


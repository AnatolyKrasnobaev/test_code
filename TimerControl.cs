using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimerControl : MonoBehaviour {
    public static GameObject LearningTimer;
    public static GameObject UpgradeTimer;
    public static GameObject ComposeTimer;
    public static GameObject WriteTimer;
    public static GameObject EnergyRecoverTimer;

    static UnityEvent FinishLearning = new UnityEvent();
    static UnityEvent FinishUpgrade = new UnityEvent();
    static UnityEvent FinishCompose = new UnityEvent();
    static UnityEvent FinishWrite = new UnityEvent();
    public static UnityEvent ResetEnergyRecover = new UnityEvent();

    [SerializeField]private Image miniLearning;
    Text miniLearningTimerText;
    [SerializeField]private Image miniUpgrade;
    Text miniUpgradeTimerText;
    [SerializeField]private Image miniCompose;
    Text miniComposeTimerText;
    [SerializeField]private Image miniWrite;
    Text miniWriteTimerText;
    public static int remainingSec;
    bool action = false;
   [SerializeField]private GameObject miniIconsBackground;

    void Awake()
    {
        miniLearningTimerText = miniLearning.transform.GetChild(0).GetComponent<Text>();
        miniUpgradeTimerText = miniUpgrade.transform.GetChild(0).GetComponent<Text>();
        miniComposeTimerText = miniCompose.transform.GetChild(0).GetComponent<Text>();
        miniWriteTimerText = miniWrite.transform.GetChild(0).GetComponent<Text>();
        FinishLearning.AddListener(() =>
        {
                StartCoroutine(FinishAction("Learning"));
        });
        FinishUpgrade.AddListener(() =>
        {
            StartCoroutine(FinishAction("Upgrading"));
        });
        FinishCompose.AddListener(() =>
        {
            StartCoroutine(FinishAction("Compose"));
        });
        FinishWrite.AddListener(() =>
        {
            StartCoroutine(FinishAction("Write"));
        });
        ResetEnergyRecover.AddListener(() =>
        {
            EnergyRecoverCalculate();
        });
    }

    void OnDisable()
    {
        FinishLearning.RemoveAllListeners();
        FinishUpgrade.RemoveAllListeners();
        FinishCompose.RemoveAllListeners();
        FinishWrite.RemoveAllListeners();
        ResetEnergyRecover.RemoveAllListeners();
    }
    IEnumerator Delay(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (PlayerPrefs.GetString("IsLearningTimerDate") != "" &&
            PlayerPrefs.GetString("SchoolWhereLearningNow") !="")
        {
            CalculateDiffFromStartTimer(PlayerPrefs.GetString("IsLearningTimerDate"),"Learning");
        }
        if (PlayerPrefs.GetString("IsUpgradingTimerDate") != "")
        {
            CalculateDiffFromStartTimer(PlayerPrefs.GetString("IsUpgradingTimerDate"), "Upgrading");
        }
        if (PlayerPrefs.GetString("IsComposeTimerDate") != "")
        {
            CalculateDiffFromStartTimer(PlayerPrefs.GetString("IsComposeTimerDate"), "Compose");
        }
        if (PlayerPrefs.GetString("IsWriteTimerDate") != "")
        {
            CalculateDiffFromStartTimer(PlayerPrefs.GetString("IsWriteTimerDate"), "Write");
        }
    }
    IEnumerator Wait(float sec)
    {
        yield return new WaitForSeconds(sec);
        EnergyRecoverCalculate();
    }
    void EnergyRecoverCalculate()
    {
        GameObject energyHudText = GameObject.Find("EnergyHudText");
        if (remainingSec > 0)
        {
            if (GameObject.Find("EnergyRecoverTimer") == null)
            {
                EnergyRecoverTimer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                EnergyRecoverTimer.name = "EnergyRecoverTimer";
                EnergyRecoverTimer.AddComponent<TimerWithSpan>();
                EnergyRecoverTimer.GetComponent<TimerWithSpan>().zavTime = new int[] { 00, 00, remainingSec };
                EnergyRecoverTimer.GetComponent<TimerWithSpan>().targTime = new int[] { 00, 00, 00 };
                EnergyRecoverTimer.GetComponent<TimerWithSpan>().Reset = false;
                EnergyRecoverTimer.GetComponent<TimerWithSpan>().runTimer = true;
                EnergyRecoverTimer.GetComponent<TimerWithSpan>().zvon.AddListener(() => {
                    energyHudText.GetComponent<Text>().text = "+1";
                    StartCoroutine(PlayerClass.ChangeAttr("energy_bar", 1));
                    Destroy(EnergyRecoverTimer, 0.001f);
                });
            }
        } else
        {
            int energyBar = PlayerPrefs.GetInt("Energy");
            int maxEnergyVal = PlayerClass.GetAttributes()["energy"];

            if (energyBar < maxEnergyVal)
            {
                if (GameObject.Find("EnergyRecoverTimer") == null)
                {
                        EnergyRecoverTimer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        EnergyRecoverTimer.name = "EnergyRecoverTimer";
                        EnergyRecoverTimer.AddComponent<TimerWithSpan>();
                        EnergyRecoverTimer.GetComponent<TimerWithSpan>().zavTime = new int[] { 00, 00, 60 };
                        EnergyRecoverTimer.GetComponent<TimerWithSpan>().targTime = new int[] { 00, 00, 00 };
                        EnergyRecoverTimer.GetComponent<TimerWithSpan>().Reset = false;
                        EnergyRecoverTimer.GetComponent<TimerWithSpan>().runTimer = true;
                        EnergyRecoverTimer.GetComponent<TimerWithSpan>().zvon.AddListener(() => {
                            energyHudText.GetComponent<Text>().text = "+1";
                            StartCoroutine(PlayerClass.ChangeAttr("energy_bar", 1));
                            Destroy(EnergyRecoverTimer, 0.001f);
                        });
                } else
                {
                    remainingSec = int.Parse(EnergyRecoverTimer.GetComponent<TimerWithSpan>().timeString.Substring(6, 2));
                }
            }
        }
    }

    void Start()
    {//string has format ((true/fals)(000000)(20190101)(235959)) without '/' and '()'
        
        StartCoroutine(Delay(0.5f));
        if (remainingSec > 0)
        {
            EnergyRecoverCalculate();
        }else
        {
            StartCoroutine(Wait(0.5f));
        }
    }

    void CalculateDiffFromStartTimer(string actionData,string nameOfAction)
    {
        string substringIsAction;
        string substringDate;
        string substringTimer;
        float rezultTime;
        float timerSub;
        substringIsAction = actionData.Substring(0, 4);//true/fals
        substringDate = actionData.Substring(10, 14);//20190101235959 - date year mounth day hour minute second
        substringTimer = actionData.Substring(4, 6);//000000 timer string hour minute second
        DateTime last = new DateTime(int.Parse(substringDate.Substring(0, 4)), int.Parse(substringDate.Substring(4, 2)),
                int.Parse(substringDate.Substring(6, 2)), int.Parse(substringDate.Substring(8, 2)), int.Parse(substringDate.Substring(10, 2)),
                int.Parse(substringDate.Substring(12, 2)));

            DateTime nowByTimer = new DateTime(DayNightControl.timY, DayNightControl.timMo,
                DayNightControl.timD, DayNightControl.timH, DayNightControl.timMi, DayNightControl.timS);
            rezultTime = Convert.ToSingle(nowByTimer.Subtract(last).TotalSeconds);
            Debug.Log(string.Format("Diff from start {0} by now in seconds : ",nameOfAction.ToUpper()) + rezultTime);
            timerSub = (float.Parse(substringTimer.Substring(0, 2)) * 3600) + (float.Parse(substringTimer.Substring(2, 2)) * 60) +
            (float.Parse(substringTimer.Substring(4, 2)));
            Debug.Log(string.Format("Seconds from timer to finish {0} :",nameOfAction.ToUpper()) + timerSub);
        if (nameOfAction == "Learning")
        {
            Debug.Log("Learned skill : " + PlayerPrefs.GetString("SelectedLearnedSkill"));
            Debug.Log("School name : " + PlayerPrefs.GetString("SchoolWhereLearningNow"));
        } else if (nameOfAction == "Upgrading")
        {
            Debug.Log("Selected item to upgrade : " + PlayerPrefs.GetString("SelectedItemToUpgrade"));
        }
        if (rezultTime < timerSub)
            {
                if (GameObject.Find(string.Format("{0}Timer",nameOfAction)) == null)
                {
                    GameObject tim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tim.name = string.Format("{0}Timer",nameOfAction);
                    tim.AddComponent<TimerWithSpan>();
                TimeSpan rezult = new TimeSpan();
                rezult = TimeSpan.FromSeconds(rezultTime);// Convert.ToInt32(rezultTime) / 3600, Convert.ToInt32(rezultTime) / 60, Convert.ToInt32(rezultTime) % 60);
                    TimeSpan subrezult = new TimeSpan(int.Parse(substringTimer.Substring(0, 2)), int.Parse(substringTimer.Substring(2, 2)), int.Parse(substringTimer.Substring(4, 2)));
                    int zav = Convert.ToInt32(subrezult.Subtract(rezult).TotalSeconds);
                var zavod = new TimeSpan();
                zavod = TimeSpan.FromSeconds(zav);
                    tim.GetComponent<TimerWithSpan>().zavTime = new int[] { zavod.Hours, zavod.Minutes, zavod.Seconds};
                    tim.GetComponent<TimerWithSpan>().targTime = new int[] { 00, 00, 00 };
                    tim.GetComponent<TimerWithSpan>().Reset = false;
                    tim.GetComponent<TimerWithSpan>().runTimer = true;
                    tim.GetComponent<TimerWithSpan>().zvon.AddListener(() => {
                        StartCoroutine(FinishAction(nameOfAction));
                    });
                }
            } else {
                Debug.Log(string.Format("{0} action is finished earlier",nameOfAction));
                StartCoroutine(FinishAction(nameOfAction));
            }

    }

    public static IEnumerator StartAction(string nameOfAction,string playerName,
                                            int moneyVal,string moneyType,
                                            string learningCost,string currentSchool,
                                            string selectedSkill,string timer)
    {
        yield return PlayerClass.CheckConn();
        if (PlayerClass.conOk)
        {
            WWWForm form = new WWWForm();
            form.AddField("playername", playerName);
            form.AddField("money", moneyVal);
            form.AddField("moneyname", moneyType);
            form.AddField("schoolname", currentSchool);
            form.AddField("cost",learningCost);
            UnityWebRequest www = UnityWebRequest.Post(URLs.Link("Learning"), form);
            yield return www.SendWebRequest();
            Debug.Log(www.downloadHandler.text);
            string[] answer = www.downloadHandler.text.Split('^');
            if (!www.isNetworkError)
            {
                for (int i = 0; i < answer.Length; i++)
                {
                    if (answer[i] == "Learning buyed")
                    {
                        if (LearningTimer == null)
                        {
                            LearningTimer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        }
                        LearningTimer.name = string.Format("{0}Timer", nameOfAction);
                        LearningTimer.AddComponent<TimerWithSpan>();
                        LearningTimer.GetComponent<TimerWithSpan>().zavTime = new int[]
                        {
                            int.Parse(timer.Substring(0,2)),
                            int.Parse(timer.Substring(5,2)),
                            int.Parse(timer.Substring(10,2))
                        };
                        LearningTimer.GetComponent<TimerWithSpan>().targTime = new int[] { 00, 00, 00 };
                        LearningTimer.GetComponent<TimerWithSpan>().Reset = false;
                        LearningTimer.GetComponent<TimerWithSpan>().zvon.AddListener(() =>
                        {
                            FinishLearning.Invoke();
                        });
                        LearningTimer.GetComponent<TimerWithSpan>().runTimer = true;
                        
                        yield return DayNightControl.GetTime();

                        PlayerPrefs.SetString("SelectedLearnedSkill", selectedSkill);
                        PlayerPrefs.SetString("SchoolWhereLearningNow", currentSchool);
                        PlayerPrefs.SetString("IsLearningTimerDate",
                            "true" +
                            string.Format("{0:00}{1:00}{2:00}",
                                int.Parse(timer.Substring(0, 2)),
                                int.Parse(timer.Substring(5, 2)),
                                int.Parse(timer.Substring(10, 2))) +
                            string.Format("{0,4}{1:00}{2:00}{3:00}{4:00}{5:00}",
                            DayNightControl.timY,
                            DayNightControl.timMo,
                            DayNightControl.timD,
                            DayNightControl.timH,
                            DayNightControl.timMi,
                            DayNightControl.timS));
                        PlayerPrefs.Save();
                    }
                }
                yield return PlayerClass.GetPlayerData("Items");
            } 
        }
        else
        {
            GameStateManager.Message(Localize.LocalizeMessage("ConnectionError"));
            yield return GameStateManager.ClearMessage();
        }
    }

    public static IEnumerator StartAction(string nameOfAction,string playerName,string itemName,string timer)
    {
        yield return PlayerClass.CheckConn();
        if (PlayerClass.conOk)
        {
            WWWForm form = new WWWForm();
            form.AddField("playername", playerName);
            form.AddField("itemname", itemName);
            UnityWebRequest www = UnityWebRequest.Post(URLs.Link("UpgradeItem"), form);
            yield return www.SendWebRequest();
            Debug.Log(www.downloadHandler.text);
            string[] answer = www.downloadHandler.text.Split('^');
            if (!www.isNetworkError)
            {
                for (int i = 0; i < answer.Length; i++)
                {
                    if (answer[i] == "Upgrade payed")
                    {
                        if (UpgradeTimer == null)
                        {
                            UpgradeTimer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        }
                        UpgradeTimer.name = string.Format("{0}Timer", nameOfAction);
                        UpgradeTimer.AddComponent<TimerWithSpan>();
                        UpgradeTimer.GetComponent<TimerWithSpan>().zavTime = new int[]
                        {
                            int.Parse(timer.Substring(0,2)),
                            int.Parse(timer.Substring(5,2)),
                            int.Parse(timer.Substring(10,2))
                        };
                        UpgradeTimer.GetComponent<TimerWithSpan>().targTime = new int[] { 00, 00, 00 };
                        UpgradeTimer.GetComponent<TimerWithSpan>().Reset = false;
                        UpgradeTimer.GetComponent<TimerWithSpan>().zvon.AddListener(() =>
                        {
                            FinishUpgrade.Invoke();
                        });
                        UpgradeTimer.GetComponent<TimerWithSpan>().runTimer = true;

                        yield return DayNightControl.GetTime();

                        PlayerPrefs.SetString("SelectedItemToUpgrade", itemName);
                        PlayerPrefs.SetString("IsUpgradingTimerDate", 
                            "true"+
                            string.Format("{0:00}{1:00}{2:00}",
                            int.Parse(timer.Substring(0, 2)),
                            int.Parse(timer.Substring(5, 2)),
                            int.Parse(timer.Substring(10, 2))) +
                            string.Format("{0,4}{1:00}{2:00}{3:00}{4:00}{5:00}",
                            DayNightControl.timY,
                            DayNightControl.timMo,
                            DayNightControl.timD,
                            DayNightControl.timH,
                            DayNightControl.timMi,
                            DayNightControl.timS));
                        PlayerPrefs.Save();
                    }
                }
                yield return PlayerClass.GetPlayerData("Items");
            }
        }
        else {
            GameStateManager.Message(Localize.LocalizeMessage("ConnectionError"));
            yield return GameStateManager.ClearMessage();
        }
    }

    public static void StartAction(string nameOfAction, string timerComposeText, string timerWriteText)
    {
        if (nameOfAction == "Compose")
        {
            if (ComposeTimer == null)
            {
                ComposeTimer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            ComposeTimer.name = "ComposeTimer";
            ComposeTimer.AddComponent<TimerWithSpan>();
            ComposeTimer.GetComponent<TimerWithSpan>().targTime = new int[] { 0, 0, 0 };
            ComposeTimer.GetComponent<TimerWithSpan>().zavTime = new int[] {int.Parse(timerComposeText.Substring(0,2)),
                int.Parse(timerComposeText.Substring(3,2)),int.Parse(timerComposeText.Substring(6,2))};
            ComposeTimer.GetComponent<TimerWithSpan>().Reset = false;
            ComposeTimer.GetComponent<TimerWithSpan>().zvon.AddListener(() => {
                FinishCompose.Invoke();
            });
            ComposeTimer.GetComponent<TimerWithSpan>().runTimer = true;
        }
        if (nameOfAction == "Write")
        {
            if (WriteTimer == null)
            {
                WriteTimer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            WriteTimer.name = "WriteTimer";
            WriteTimer.AddComponent<TimerWithSpan>();
            WriteTimer.GetComponent<TimerWithSpan>().targTime = new int[] { 0, 0, 0 };
            WriteTimer.GetComponent<TimerWithSpan>().zavTime = new int[] {int.Parse(timerWriteText.Substring(0,2)),
                int.Parse(timerWriteText.Substring(3,2)),int.Parse(timerWriteText.Substring(6,2))};
            WriteTimer.GetComponent<TimerWithSpan>().Reset = false;
            WriteTimer.GetComponent<TimerWithSpan>().zvon.AddListener(() => {
                FinishWrite.Invoke();
            });
            WriteTimer.GetComponent<TimerWithSpan>().runTimer = true;
        }
}


    IEnumerator FinishAction(string nameOfAction) {
        yield return StartCoroutine(PlayerClass.CheckConn());
        if (PlayerClass.conOk)
        {
            WWWForm form = new WWWForm();
            form.AddField("playername", PlayerPrefs.GetString("PlayerName"));
            if (nameOfAction == "Learning")
            {
                form.AddField("schoolname", PlayerPrefs.GetString("SchoolWhereLearningNow"));
                form.AddField("selskill", PlayerPrefs.GetString("SelectedLearnedSkill"));
                UnityWebRequest www = UnityWebRequest.Post(URLs.Link("FinishLearning"), form);
                yield return www.SendWebRequest();
                Debug.Log(www.downloadHandler.text);
                string[] answer = www.downloadHandler.text.Split('^');
                if (!www.isNetworkError)
                {
                    for (int i = 0; i < answer.Length; i++)
                    {
                        if (answer[i] == "Skill learned")
                        {
                            PlayerPrefs.DeleteKey("IsLearningTimerDate");
                            GameStateManager.Message(Localize.LocalizeMessage("LearningComplete"));
                            StartCoroutine(GameStateManager.ClearMessage());
                        }
                    }
                    yield return (PlayerClass.GetPlayerData("PassiveSkills"));
                    Destroy(LearningTimer, 0.001f);
                    if (GameObject.Find("MusicSchoolWindow") != null)
                    {
                        StartCoroutine(GameObject.Find("MusicSchoolWindow").GetComponent<LearningPlaces>().OnSchoolIconButtonPressed());
                    }
                }

                WWWForm formstat = new WWWForm();
                formstat.AddField("playername", PlayerClass.GetPlayerName());
                formstat.AddField("learningcomplete", 1);
                formstat.AddField("schoolname", PlayerPrefs.GetString("SchoolWhereLearningNow"));
                UnityWebRequest wwwstat = UnityWebRequest.Post(URLs.Link("UpdateStatistic"), formstat);
                yield return wwwstat.SendWebRequest();
                Debug.Log(wwwstat.downloadHandler.text);
            }
            else if (nameOfAction == "Upgrading")
            {
                form.AddField("itemname", PlayerPrefs.GetString("SelectedItemToUpgrade"));
                form.AddField("finish", "yes");
                UnityWebRequest www = UnityWebRequest.Post(URLs.Link("UpgradeItem"), form);
                yield return www.SendWebRequest();
                Debug.Log(www.downloadHandler.text);
                string[] answer = www.downloadHandler.text.Split('^');
                if (!www.isNetworkError)
                {
                    for (int i = 0; i < answer.Length; i++)
                    {
                        if (answer[i] == "Upgrade finished")
                        {
                            PlayerPrefs.DeleteKey("IsUpgradingTimerDate");
                            PlayerPrefs.DeleteKey("SelectedItemToUpgrade");
                            GameStateManager.Message(Localize.LocalizeMessage("ItemUpdatingComplete"));
                            StartCoroutine(GameStateManager.ClearMessage());
                        }
                    }
                    yield return PlayerClass.GetPlayerData("Items");
                    Destroy(UpgradeTimer, 0.001f);
                    UserInterfControl.GetPlayerData.Invoke();
                    if (GameObject.Find("ServisWindow") != null)
                    {
                        GameObject.Find("ServisWindow").GetComponent<ItemUpgrade>().OpenUpgradeWindow();
                    }
                }

            }
            else if (nameOfAction == "Compose" ||
                nameOfAction == "Write")
            {
                string objectUid = "";
                if (nameOfAction == "Compose")
                {
                    foreach (Music music in PlayerClass.playerMusics.Values)
                    {
                        if (music.created == 0)
                        {
                            objectUid = music.uid;
                        }
                    }
                }
                if (nameOfAction == "Write")
                {
                    foreach (Verse verse in PlayerClass.playerVerses.Values)
                    {
                        if (verse.created == 0)
                        {
                            objectUid = verse.uid;
                        }
                    }
                }
                form.AddField("uid", objectUid);
                UnityWebRequest www = UnityWebRequest.Post(URLs.Link("FinishCreatingMusicVerseSong"), form);
                yield return www.SendWebRequest();
                Debug.Log(www.downloadHandler.text);
                string[] answer = www.downloadHandler.text.Split('^');
                if (!www.isNetworkError)
                {
                    for (int i = 0; i < answer.Length; i++)
                    {
                        if (answer[i] == "Created finished")
                        {
                            if (nameOfAction == "Compose")
                            {
                                PlayerClass.playerMusics[objectUid].created = 1;
                                GameStateManager.Message(Localize.LocalizeMessage("MusicCreateComplete"));
                            }
                            if (nameOfAction == "Write")
                            {
                                PlayerClass.playerVerses[objectUid].created = 1;
                                GameStateManager.Message(Localize.LocalizeMessage("VerseCreateComplete"));
                            }
                            PlayerPrefs.DeleteKey(string.Format("Is{0}TimerDate", nameOfAction));
                        }
                        else if (answer[i] == "Skill updated")
                        {
                            yield return PlayerClass.GetPlayerData("PassiveSkills");
                        }
                    }
                    Destroy(GameObject.Find(string.Format("{0}Timer", nameOfAction)), 0.001f);
                    StartCoroutine(GameStateManager.ClearMessage());
                    if (GameObject.Find("RehearsalPointWindow") != null)
                    {
                        GameObject.Find("RehearsalPointWindow").GetComponent<RehearsalPoint>().ResetTimeLine(nameOfAction);
                    }

                }
            }
        } else {
            GameStateManager.Message(Localize.LocalizeMessage("ConnectionError"));
            yield return GameStateManager.ClearMessage();
        }
    }

    void MiniIcons()
    {
        if (GameObject.Find("LearningTimer") != null || GameObject.Find("UpgradeTimer")!=null ||
            GameObject.Find("ComposeTimer") != null || GameObject.Find("WriteTimer") != null)
        {
            miniIconsBackground.SetActive(true);
        } else
        {
            miniIconsBackground.SetActive(false);
        }
        if (GameObject.Find("LearningTimer") != null)
        {
            miniLearning.gameObject.SetActive(true);
            miniLearningTimerText.text = GameObject.Find("LearningTimer").GetComponent<TimerWithSpan>().timeString;
        } else {
            miniLearning.gameObject.SetActive(false);
        }
        if (GameObject.Find("UpgradeTimer") != null)
        {
            miniUpgrade.gameObject.SetActive(true);
            miniUpgradeTimerText.text = GameObject.Find("UpgradeTimer").GetComponent<TimerWithSpan>().timeString;
        } else
        {
            miniUpgrade.gameObject.SetActive(false);
        }
        if (GameObject.Find("ComposeTimer") != null)
        {
            miniCompose.gameObject.SetActive(true);
            miniComposeTimerText.text = GameObject.Find("ComposeTimer").GetComponent<TimerWithSpan>().timeString;
        } else
        {
            miniCompose.gameObject.SetActive(false);
        }
        if (GameObject.Find("WriteTimer") != null)
        {
            miniWrite.gameObject.SetActive(true);
            miniWriteTimerText.text = GameObject.Find("WriteTimer").GetComponent<TimerWithSpan>().timeString;
        } else
        {
            miniWrite.gameObject.SetActive(false);
        }
    }
    void Update()
    {
        MiniIcons();
        if (GameObject.Find("EnergyRecoverTimer") != null)
        {
            if (GameObject.Find("EnergyRecoverTimer").GetComponent<TimerWithSpan>() != null)
            {
                if (GameObject.Find("EnergyTimerText") != null)
                {
                    if (GameObject.Find("EnergyTimerText").GetComponent<Text>() != null)
                    {
                        GameObject.Find("EnergyTimerText").GetComponent<Text>().text = GameObject.Find("EnergyRecoverTimer").GetComponent<TimerWithSpan>().timeString.Substring(3, 5);
                    }
                }
            }
        }
        else
        {
            if (GameObject.Find("EnergyTimerText").GetComponent<Text>() != null)
            {
                GameObject.Find("EnergyTimerText").GetComponent<Text>().text = "";
            }
        }
    }
}

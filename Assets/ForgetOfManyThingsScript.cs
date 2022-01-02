//USE DO HYEON AND FA PLAY FOR NEW ARROWS (250 × 250 FONT-SIZE 155px AT X = 70 FOR RIGHT ARROW)

using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class ForgetOfManyThingsScript : MonoBehaviour
{
    static int _moduleIdCounter = 1;
    int _moduleID = 0;
    public static int HighestID;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBossModule Boss;
    public KMSelectable[] Buttons;
    public KMSelectable ModuleObject;
    public KMSelectable ModuleSelectable;
    public TextMesh Display;
    public TextMesh DisplayInputs;
    public TextMesh StageIndicator;
    public MeshRenderer[] LEDs;
    public GameObject[] Arrows;
    public static string[] IgnoredModules = null;

    private int[] InformationTypes;

    #region Sub-module variables + number of sub-modules
    private int[] ListeningSounds;

    private bool[,] EightyOneGrids;
    private int[] EightyOneInitialButtons;
    private int[] EightyOneFinalValues;

    private int[] ForgetItNotValues;

    private int[] ForgetMeNotPrevious;
    private int[] ForgetMeNotCurrent;
    private int[] ForgetMeNotFinalValues;

    private int[] DOMTCardTypes, DOMTCardRanks, DOMTCardSuits, DOMTCardTropicColours, DOMTFinalValues;
    private string[] DOMTCardTypeNames = { "Standard", "Metropolitan", "Maritime", "Arctic", "Tropical", "Oasis" };
    private string[] DOMTCardColourNames = { "Blue", "Pink", "Purple", "Green", "Yellow", "Orange" };
    private string[] DOMTSuitNames = { "Clubs", "Diamonds", "Hearts", "Spades", "Moons", "Hands", "Suns", "Stars" };

    private int[][] KeypadDirDirections;
    private int[] KeypadDirNumbers;
    private int[] KeypadDirFinalValues;
    private string[] KeypadDirDirectionNames = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

    private int[] FinalAnswerExpected;
    private readonly int TypeAmount = 6; //Increment this when adding a new sub-module.
    private string[] ModuleNames = { "Listening", "81", "Forget Me Not", "Forget It Not", "The Deck of Many Things", "Keypad Directionality" }; //Add your sub-module's name here.
    #endregion

    private List<int> Types = new List<int>();
    private List<int> Stages = new List<int>();
    private int CorrectlyInputStages, NonIgnored, SolvedCheck, Stage, Page, PreActiveSolves;
    private float DefaultGameMusicVolume;
    private bool Active, Focused, InputAllow, Solved;
    private KMAudio.KMAudioRef Sound;
    private KeyCode[] NormalKeyCodes = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };
    private KeyCode[] KeypadKeyCodes = { KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9, KeyCode.Keypad0 };

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        StageIndicator.text = "";
        Display.text = "";
        if (Application.isEditor)
            Focused = true;
        for (int i = 0; i < 2; i++)
            Arrows[i].SetActive(false);
        if (_moduleID > HighestID)
            HighestID = _moduleID;
        if (IgnoredModules == null)
            IgnoredModules = GetComponent<KMBossModule>().GetIgnoredModules("The Forget of Many Things", new string[]{
                "14",
                "Forget Enigma",
                "Forget Everything",
                "Forget It Not",
                "Forget Me Later",
                "Forget Me Not",
                "The Forget of Many Things",
                "Forget Perspective",
                "Forget Them All",
                "Forget This",
                "Forget Us Not",
                "Organization",
                "Purgatory",
                "Simon's Stages",
                "Souvenir",
                "Tallordered Keys",
                "The Time Keeper",
                "Timing is Everything",
                "The Troll",
                "Turn The Key",
                "Übermodule",
                "Ültimate Custom Night",
                "The Very Annoying Button"
            });
        Module.OnActivate += delegate
        {
            if (NonIgnored == 0)
            {
                Display.text = "PRESS 0 TO\nAUTO-SOLVE";
                StageIndicator.text = "---";
            }
            if (_moduleID == HighestID)
                Audio.PlaySoundAtTransform("activate", ModuleObject.transform);
            Active = true;
            if (!InputAllow)
                Activate();
            for (int i = 0; i < 10; i++)
            {
                int x = i;
                Buttons[i].OnInteract += delegate { ButtonPress(x); return false; };
            }
        };
        try
        {
            DefaultGameMusicVolume = GameMusicControl.GameMusicVolume;
        }
        catch { }
        Bomb.OnBombExploded += delegate
        {
            try
            {
                Sound.StopSound();
            }
            catch { }
            try
            {
                GameMusicControl.GameMusicVolume = DefaultGameMusicVolume;
            }
            catch { }
        };
        Bomb.OnBombSolved += delegate
        {
            try
            {
                Sound.StopSound();
            }
            catch { }
            try
            {
                GameMusicControl.GameMusicVolume = DefaultGameMusicVolume;
            }
            catch { }
        };
        ModuleSelectable.OnFocus += delegate { Focused = true; };
        ModuleSelectable.OnDefocus += delegate { Focused = false; };
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(SolveCheck());
        CalcStages();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 10; i++)
            if (Focused && (Input.GetKeyDown(NormalKeyCodes[i]) || Input.GetKeyDown(KeypadKeyCodes[i])))
                Buttons[i].OnInteract();
        if (Focused && Input.GetKeyDown(KeyCode.A))
            Buttons[3].OnInteract();
        if (Focused && Input.GetKeyDown(KeyCode.D))
            Buttons[5].OnInteract();
    }

    void Activate()
    {
        if (NonIgnored != 0)
        {
            Page = 1;
            StageIndicator.text = (Stage - PreActiveSolves + 1).ToString("000");
            Types = new List<int>();
            Stages = new List<int>();
            for (int i = 0; i < PreActiveSolves + 1; i++)
            {
                if (InformationTypes[Stage - PreActiveSolves + i] == 0)
                {
                    if (i == 0)
                    {
                        Display.text = "LISTENING\nPRESS 0 TO PLAY";
                        for (int j = 0; j < 10; j++)
                            LEDs[j].material.color = new Color(0, 0, 0);
                    }
                    Types.Add(0);
                    Stages.Add(Stage - PreActiveSolves + i);
                }
                else if (InformationTypes[Stage - PreActiveSolves + i] == 1)
                {
                    if (i == 0)
                    {
                        Display.text = "81\nPREV PRESSED: " + EightyOneInitialButtons[Stage - PreActiveSolves + i];
                        for (int j = 0; j < 9; j++)
                        {
                            if (!EightyOneGrids[Stage - PreActiveSolves + i, j])
                                LEDs[j].material.color = new Color(0, 0, 0);
                            else
                                LEDs[j].material.color = new Color(0, 1, 0);
                        }
                        LEDs[9].material.color = new Color(0, 0, 0);
                    }
                    Types.Add(1);
                    Stages.Add(Stage - PreActiveSolves + i);
                }
                else if (InformationTypes[Stage - PreActiveSolves + i] == 2)
                {
                    if (i == 0)
                    {
                        Display.text = "FORGET ME NOT\nPREV NUM: " + ForgetMeNotPrevious[Stage - PreActiveSolves + i];
                        for (int j = 0; j < 10; j++)
                            LEDs[j].material.color = new Color(0, 0, 0);
                        LEDs[(ForgetMeNotCurrent[Stage - PreActiveSolves + i] + 9) % 10].material.color = new Color(0, 1, 0);
                    }
                    Types.Add(2);
                    Stages.Add(Stage - PreActiveSolves + i);
                }
                else if (InformationTypes[Stage - PreActiveSolves + i] == 3)
                {
                    if (i == 0)
                    {
                        Display.text = "FORGET IT NOT\n";
                        for (int j = 0; j < 10; j++)
                            LEDs[j].material.color = new Color(0, 0, 0);
                        LEDs[(FinalAnswerExpected[Stage - PreActiveSolves + i] + 9) % 10].material.color = new Color(0, 1, 0);
                    }
                    Types.Add(3);
                    Stages.Add(Stage - PreActiveSolves + i);
                }
                else if (InformationTypes[Stage - PreActiveSolves + i] == 4)
                {
                    if (i == 0)
                    {
                        Display.text = "THE DECK OF\nMANY THINGS";
                        for (int j = 0; j < 10; j++)
                            LEDs[j].material.color = new Color(0, 0, 0);
                    }
                    if (DOMTCardTypes[Stage - PreActiveSolves + i] == 4)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            Types.Add(4);
                            Stages.Add(Stage - PreActiveSolves + i);
                        }
                    }
                    else
                        for (int j = 0; j < 2; j++)
                        {
                            Types.Add(4);
                            Stages.Add(Stage - PreActiveSolves + i);
                        }
                }
                else if (InformationTypes[Stage - PreActiveSolves + i] == 5)
                {
                    if (i == 0)
                    {
                        Display.text = "KEYPAD\nDIRECTIONALITY";
                        for (int j = 0; j < 10; j++)
                            LEDs[j].material.color = new Color(0, 0, 0);
                        LEDs[KeypadDirNumbers[Stage - PreActiveSolves + i]].material.color = new Color(0, 1, 0);
                    }
                    for (int j = 0; j < 2; j++)
                    {
                        Types.Add(5);
                        Stages.Add(Stage - PreActiveSolves + i);
                    }
                }
            }
            PreActiveSolves = 0;
            for (int i = 0; i < 2; i++)
                Arrows[i].SetActive(false);
            if (Types.Count() > 1)
                Arrows[1].SetActive(true);
        }
    }

    void CalcStages()
    {
        NonIgnored = Bomb.GetSolvableModuleNames().Where(x => !IgnoredModules.Contains(x)).Count();
        InformationTypes = new int[NonIgnored];

        #region Defining sub-module array variable lengths
        EightyOneInitialButtons = new int[NonIgnored];
        EightyOneGrids = new bool[NonIgnored, 9];
        EightyOneFinalValues = new int[NonIgnored];

        ListeningSounds = new int[NonIgnored];

        ForgetItNotValues = new int[NonIgnored];

        ForgetMeNotPrevious = new int[NonIgnored];
        ForgetMeNotCurrent = new int[NonIgnored];
        ForgetMeNotFinalValues = new int[NonIgnored];

        ForgetMeNotFinalValues = new int[NonIgnored];

        DOMTCardTypes = new int[NonIgnored];
        DOMTCardSuits = new int[NonIgnored];
        DOMTCardRanks = new int[NonIgnored];
        DOMTCardTropicColours = new int[NonIgnored];
        DOMTFinalValues = new int[NonIgnored];

        KeypadDirDirections = new int[NonIgnored][];
        for (int i = 0; i < NonIgnored; i++)
            KeypadDirDirections[i] = new int[4];
        KeypadDirNumbers = new int[NonIgnored];
        KeypadDirFinalValues = new int[NonIgnored];

        //Initialise sub-module variables above here.
        //A sub-module needs to have a list of final values.
        #endregion

        FinalAnswerExpected = new int[NonIgnored];

        if (NonIgnored == 0)
            Debug.LogFormat("[The Forget of Many Things #{0}] There aren't any non-ignored modules on the bomb. The module may now be autosolved.", _moduleID);
        for (int i = 0; i < NonIgnored; i++)
        {
            InformationTypes[i] = Rnd.Range(0, TypeAmount);
            Debug.LogFormat("[The Forget of Many Things #{0}] Stage {1}'s module is {2}.", _moduleID, i + 1, ModuleNames[InformationTypes[i]]);

            ListeningSounds[i] = Rnd.Range(0, 10);

            ForgetItNotValues[i] = Rnd.Range(0, 10);

            #region 81
            EightyOneInitialButtons[i] = Rnd.Range(1, 10);
            bool[] Cache = new bool[9];
            Cache[EightyOneInitialButtons[i] - 1] = true;
            for (int j = 0; j < 9; j++)
            {
                if (Rnd.Range(0, 2) == 0)
                    EightyOneGrids[i, j] = false;
                else
                    EightyOneGrids[i, j] = true;
            }
            for (int j = 0; j < 9; j++)
            {
                bool[] Cache2 = new bool[9];
                for (int k = 0; k < 9; k++)
                    Cache2[k] = Cache[k];
                if (!EightyOneGrids[i, j])
                {
                    Cache[0] = Cache2[6];
                    Cache[1] = Cache2[3];
                    Cache[2] = Cache2[0];
                    Cache[3] = Cache2[7];
                    Cache[5] = Cache2[1];
                    Cache[6] = Cache2[8];
                    Cache[7] = Cache2[5];
                    Cache[8] = Cache2[2];
                }
                else
                {
                    Cache[0] = Cache2[2];
                    Cache[1] = Cache2[0];
                    Cache[2] = Cache2[1];
                    Cache[3] = Cache2[5];
                    Cache[4] = Cache2[3];
                    Cache[5] = Cache2[4];
                    Cache[6] = Cache2[8];
                    Cache[7] = Cache2[6];
                    Cache[8] = Cache2[7];
                }
            }
            for (int j = 0; j < 9; j++)
            {
                if (Cache[j])
                    EightyOneFinalValues[i] = j + 1;
            }
            #endregion

            #region Forget Me Not
            ForgetMeNotPrevious[i] = Rnd.Range(0, 10);
            ForgetMeNotCurrent[i] = Rnd.Range(0, 10);
            if (ForgetMeNotPrevious[i] == 0 || ForgetMeNotCurrent[i] == 0)
            {
                int LargestSerial = 0;
                foreach (int c in Bomb.GetSerialNumberNumbers())
                    if (c > LargestSerial) LargestSerial = c;
                ForgetMeNotFinalValues[i] = LargestSerial;
            }
            else if (ForgetMeNotPrevious[i] % 2 == 0 && ForgetMeNotCurrent[i] % 2 == 0)
            {
                int SmallestOddSerial = 9;
                foreach (int c in Bomb.GetSerialNumberNumbers())
                    if (c % 2 == 1 && c < SmallestOddSerial) SmallestOddSerial = c;
                ForgetMeNotFinalValues[i] = SmallestOddSerial;
            }
            else
            {
                ForgetMeNotFinalValues[i] = ForgetMeNotPrevious[i] + ForgetMeNotCurrent[i];
                while (ForgetMeNotFinalValues[i] >= 10) ForgetMeNotFinalValues[i] /= 10;
            }
            #endregion

            #region The Deck of Many Things
            DOMTCardRanks[i] = Rnd.Range(1, 14);
            DOMTCardSuits[i] = Rnd.Range(0, 4);
            DOMTCardTypes[i] = Rnd.Range(0, DOMTCardTypeNames.Length);
            DOMTCardTropicColours[i] = Rnd.Range(0, 6);
            if (DOMTCardTypes[i] < 2)
                DOMTFinalValues[i] = DOMTCardRanks[i] % 10;
            else if (DOMTCardTypes[i] == 2)
            {
                DOMTCardRanks[i] = Rnd.Range(11, 19);
                DOMTFinalValues[i] = ((((DOMTCardRanks[i] * (Bomb.GetBatteryCount() + 1)) + (Bomb.GetPortPlates().Any(x => x.Length == 0) ? 10 : 0))
                    / (Bomb.GetOnIndicators().Count() + 1)) - (Bomb.GetSerialNumberLetters().Any(x => "AEIOU".Contains(x)) ? DOMTCardRanks[i] : 0) + 130) % 13 % 10;
            }
            else if (DOMTCardTypes[i] == 3)
            {
                int[] cacheDOMT;
                switch (Bomb.GetBatteryCount())
                {
                    case 0: cacheDOMT = new int[] { 2, 10, 12, 13, 5, 6, 11, 7, 8, 3, 9, 4, 1 }; break;
                    case 1:
                    case 2: cacheDOMT = new int[] { 9, 12, 2, 5, 4, 1, 7, 13, 3, 11, 8, 10, 6 }; break;
                    case 3:
                    case 4: cacheDOMT = new int[] { 12, 6, 10, 9, 4, 2, 11, 13, 8, 7, 1, 3, 5 }; break;
                    default: cacheDOMT = new int[] { 10, 11, 12, 13, 3, 9, 2, 8, 1, 7, 5, 6, 4 }; break;
                }
                DOMTFinalValues[i] = cacheDOMT[(Bomb.GetSerialNumberNumbers().Sum() + Array.FindIndex(cacheDOMT, x => x == DOMTCardRanks[i])) % cacheDOMT.Length] % 10;
            }
            else if (DOMTCardTypes[i] == 4)
            {
                int[] cacheRank = new int[6];
                switch (DOMTCardSuits[i])
                {
                    case 0:
                    {
                        switch (DOMTCardRanks[i])
                        {
                            case 1:
                            {
                                cacheRank = new int[] { 10, 10, 1, 1, 8, 1 };
                                break;
                            }
                            case 2:
                            {
                                cacheRank = new int[] { 7, 11, 2, 1, 6, 4 };
                                break;
                            }
                            case 3:
                            {
                                cacheRank = new int[] { 11, 12, 9, 4, 10, 7 };
                                break;
                            }
                            case 4:
                            {
                                cacheRank = new int[] { 3, 11, 3, 9, 3, 10 };
                                break;
                            }
                            case 5:
                            {
                                cacheRank = new int[] { 9, 13, 12, 11, 2, 9 };
                                break;
                            }
                            case 6:
                            {
                                cacheRank = new int[] { 1, 2, 3, 12, 9, 13 };
                                break;
                            }
                            case 7:
                            {
                                cacheRank = new int[] { 8, 8, 11, 12, 1, 2 };
                                break;
                            }
                            case 8:
                            {
                                cacheRank = new int[] { 4, 10, 8, 13, 12, 7 };
                                break;
                            }
                            case 9:
                            {
                                cacheRank = new int[] { 4, 3, 4, 9, 10, 3 };
                                break;
                            }
                            case 10:
                            {
                                cacheRank = new int[] { 6, 13, 6, 4, 1, 1 };
                                break;
                            }
                            case 11:
                            {
                                cacheRank = new int[] { 3, 10, 3, 5, 2, 4 };
                                break;
                            }
                            case 12:
                            {
                                cacheRank = new int[] { 13, 8, 7, 5, 9, 1 };
                                break;
                            }
                            case 13:
                            {
                                cacheRank = new int[] { 6, 4, 8, 12, 5, 12 };
                                break;
                            }
                        }
                        break;
                    }
                    case 1:
                    {
                        switch (DOMTCardRanks[i])
                        {
                            case 1:
                            {
                                cacheRank = new int[] { 12, 7, 2, 9, 12, 6 };
                                break;
                            }
                            case 2:
                            {
                                cacheRank = new int[] { 7, 6, 4, 6, 13, 13 };
                                break;
                            }
                            case 3:
                            {
                                cacheRank = new int[] { 10, 7, 5, 11, 4, 3 };
                                break;
                            }
                            case 4:
                            {
                                cacheRank = new int[] { 2, 11, 10, 8, 13, 8 };
                                break;
                            }
                            case 5:
                            {
                                cacheRank = new int[] { 13, 13, 5, 7, 9, 8 };
                                break;
                            }
                            case 6:
                            {
                                cacheRank = new int[] { 8, 1, 6, 7, 2, 4 };
                                break;
                            }
                            case 7:
                            {
                                cacheRank = new int[] { 3, 9, 10, 10, 3, 2 };
                                break;
                            }
                            case 8:
                            {
                                cacheRank = new int[] { 1, 1, 1, 2, 7, 6 };
                                break;
                            }
                            case 9:
                            {
                                cacheRank = new int[] { 7, 5, 8, 10, 8, 12 };
                                break;
                            }
                            case 10:
                            {
                                cacheRank = new int[] { 11, 7, 9, 4, 8, 8 };
                                break;
                            }
                            case 11:
                            {
                                cacheRank = new int[] { 8, 12, 5, 7, 2, 11 };
                                break;
                            }
                            case 12:
                            {
                                cacheRank = new int[] { 12, 6, 13, 1, 11, 7 };
                                break;
                            }
                            case 13:
                            {
                                cacheRank = new int[] { 2, 4, 1, 13, 5, 5 };
                                break;
                            }
                        }
                        break;
                    }
                    case 2:
                    {
                        switch (DOMTCardRanks[i])
                        {
                            case 1:
                            {
                                cacheRank = new int[] { 4, 10, 12, 13, 5, 8 };
                                break;
                            }
                            case 2:
                            {
                                cacheRank = new int[] { 7, 5, 9, 2, 3, 11 };
                                break;
                            }
                            case 3:
                            {
                                cacheRank = new int[] { 9, 4, 10, 10, 5, 11 };
                                break;
                            }
                            case 4:
                            {
                                cacheRank = new int[] { 6, 6, 10, 2, 13, 1 };
                                break;
                            }
                            case 5:
                            {
                                cacheRank = new int[] { 13, 1, 11, 5, 8, 7 };
                                break;
                            }
                            case 6:
                            {
                                cacheRank = new int[] { 8, 7, 2, 10, 12, 13 };
                                break;
                            }
                            case 7:
                            {
                                cacheRank = new int[] { 12, 13, 5, 11, 10, 9 };
                                break;
                            }
                            case 8:
                            {
                                cacheRank = new int[] { 10, 9, 12, 8, 7, 11 };
                                break;
                            }
                            case 9:
                            {
                                cacheRank = new int[] { 12, 6, 9, 3, 7, 5 };
                                break;
                            }
                            case 10:
                            {
                                cacheRank = new int[] { 5, 9, 3, 7, 1, 5 };
                                break;
                            }
                            case 11:
                            {
                                cacheRank = new int[] { 2, 9, 12, 3, 4, 3 };
                                break;
                            }
                            case 12:
                            {
                                cacheRank = new int[] { 9, 1, 6, 11, 3, 9 };
                                break;
                            }
                            case 13:
                            {
                                cacheRank = new int[] { 2, 4, 7, 6, 10, 2 };
                                break;
                            }
                        }
                        break;
                    }
                    case 3:
                    {
                        switch (DOMTCardRanks[i])
                        {
                            case 1:
                            {
                                cacheRank = new int[] { 1, 5, 2, 8, 6, 6 };
                                break;
                            }
                            case 2:
                            {
                                cacheRank = new int[] { 13, 11, 8, 1, 6, 13 };
                                break;
                            }
                            case 3:
                            {
                                cacheRank = new int[] { 3, 2, 11, 6, 9, 4 };
                                break;
                            }
                            case 4:
                            {
                                cacheRank = new int[] { 11, 3, 4, 4, 13, 9 };
                                break;
                            }
                            case 5:
                            {
                                cacheRank = new int[] { 10, 3, 11, 3, 1, 10 };
                                break;
                            }
                            case 6:
                            {
                                cacheRank = new int[] { 1, 2, 13, 6, 4, 10 };
                                break;
                            }
                            case 7:
                            {
                                cacheRank = new int[] { 5, 8, 6, 13, 6, 2 };
                                break;
                            }
                            case 8:
                            {
                                cacheRank = new int[] { 5, 5, 4, 9, 12, 6 };
                                break;
                            }
                            case 9:
                            {
                                cacheRank = new int[] { 5, 12, 1, 2, 11, 12 };
                                break;
                            }
                            case 10:
                            {
                                cacheRank = new int[] { 4, 3, 7, 3, 11, 3 };
                                break;
                            }
                            case 11:
                            {
                                cacheRank = new int[] { 11, 12, 7, 5, 4, 10 };
                                break;
                            }
                            case 12:
                            {
                                cacheRank = new int[] { 9, 8, 13, 8, 11, 12 };
                                break;
                            }
                            case 13:
                            {
                                cacheRank = new int[] { 6, 2, 13, 12, 7, 5 };
                                break;
                            }
                        }
                        break;
                    }
                }
                DOMTFinalValues[i] = cacheRank[DOMTCardTropicColours[i]] % 10;
            }
            else
            {
                int cache = DOMTCardRanks[i];
                DOMTCardSuits[i] += 4;
                switch (DOMTCardSuits[i])
                {
                    case 4: cache *= Bomb.GetPortCount(); break;
                    case 5: cache *= Bomb.GetSerialNumberNumbers().Sum(); break;
                    case 6: cache *= Bomb.GetIndicators().Count(); break;
                    case 7: cache *= Bomb.GetBatteryCount(); break;
                }
                switch (cache % 52)
                {
                    case 0: DOMTFinalValues[i] = 4; break;
                    case 1: DOMTFinalValues[i] = 4; break;
                    case 2: DOMTFinalValues[i] = 7; break;
                    case 3: DOMTFinalValues[i] = 2; break;
                    case 4: DOMTFinalValues[i] = 9; break;
                    case 5: DOMTFinalValues[i] = 2; break;
                    case 6: DOMTFinalValues[i] = 3; break;
                    case 7: DOMTFinalValues[i] = 7; break;
                    case 8: DOMTFinalValues[i] = 1; break;
                    case 9: DOMTFinalValues[i] = 3; break;
                    case 10: DOMTFinalValues[i] = 6; break;
                    case 11: DOMTFinalValues[i] = 9; break;
                    case 12: DOMTFinalValues[i] = 5; break;
                    case 13: DOMTFinalValues[i] = 3; break;
                    case 14: DOMTFinalValues[i] = 1; break;
                    case 15: DOMTFinalValues[i] = 3; break;
                    case 16: DOMTFinalValues[i] = 8; break;
                    case 17: DOMTFinalValues[i] = 8; break;
                    case 18: DOMTFinalValues[i] = 8; break;
                    case 19: DOMTFinalValues[i] = 2; break;
                    case 20: DOMTFinalValues[i] = 2; break;
                    case 21: DOMTFinalValues[i] = 6; break;
                    case 22: DOMTFinalValues[i] = 1; break;
                    case 23: DOMTFinalValues[i] = 5; break;
                    case 24: DOMTFinalValues[i] = 5; break;
                    case 25: DOMTFinalValues[i] = 0; break;
                    case 26: DOMTFinalValues[i] = 3; break;
                    case 27: DOMTFinalValues[i] = 7; break;
                    case 28: DOMTFinalValues[i] = 3; break;
                    case 29: DOMTFinalValues[i] = 1; break;
                    case 30: DOMTFinalValues[i] = 3; break;
                    case 31: DOMTFinalValues[i] = 0; break;
                    case 32: DOMTFinalValues[i] = 3; break;
                    case 33: DOMTFinalValues[i] = 2; break;
                    case 34: DOMTFinalValues[i] = 2; break;
                    case 35: DOMTFinalValues[i] = 4; break;
                    case 36: DOMTFinalValues[i] = 6; break;
                    case 37: DOMTFinalValues[i] = 2; break;
                    case 38: DOMTFinalValues[i] = 0; break;
                    case 39: DOMTFinalValues[i] = 1; break;
                    case 40: DOMTFinalValues[i] = 8; break;
                    case 41: DOMTFinalValues[i] = 1; break;
                    case 42: DOMTFinalValues[i] = 1; break;
                    case 43: DOMTFinalValues[i] = 6; break;
                    case 44: DOMTFinalValues[i] = 9; break;
                    case 45: DOMTFinalValues[i] = 3; break;
                    case 46: DOMTFinalValues[i] = 1; break;
                    case 47: DOMTFinalValues[i] = 4; break;
                    case 48: DOMTFinalValues[i] = 0; break;
                    case 49: DOMTFinalValues[i] = 5; break;
                    case 50: DOMTFinalValues[i] = 9; break;
                    case 51: DOMTFinalValues[i] = 7; break;
                }
            }
            #endregion

            #region Keypad Directionality
            KeypadDirNumbers[i] = Rnd.Range(0, 9);
            int cacheKeypadDir = KeypadDirNumbers[i];
            for (int j = 0; j < 4; j++)
            {
                KeypadDirDirections[i][j] = Rnd.Range(0, 8);
                switch (KeypadDirDirections[i][j])
                {
                    case 0:
                        cacheKeypadDir += 6;
                        cacheKeypadDir %= 9;
                        break;
                    case 1:
                        if (cacheKeypadDir % 3 == 2)
                            cacheKeypadDir -= 2;
                        else
                            cacheKeypadDir++;
                        cacheKeypadDir += 6;
                        cacheKeypadDir %= 9;
                        break;
                    case 2:
                        if (cacheKeypadDir % 3 == 2)
                            cacheKeypadDir -= 2;
                        else
                            cacheKeypadDir++;
                        break;
                    case 3:
                        if (cacheKeypadDir % 3 == 2)
                            cacheKeypadDir -= 2;
                        else
                            cacheKeypadDir++;
                        cacheKeypadDir += 3;
                        cacheKeypadDir %= 9;
                        break;
                    case 4:
                        cacheKeypadDir += 3;
                        cacheKeypadDir %= 9;
                        break;
                    case 5:
                        if (cacheKeypadDir % 3 == 0)
                            cacheKeypadDir += 2;
                        else
                            cacheKeypadDir--;
                        cacheKeypadDir += 3;
                        cacheKeypadDir %= 9;
                        break;
                    case 6:
                        if (cacheKeypadDir % 3 == 0)
                            cacheKeypadDir += 2;
                        else
                            cacheKeypadDir--;
                        break;
                    case 7:
                        if (cacheKeypadDir % 3 == 0)
                            cacheKeypadDir += 2;
                        else
                            cacheKeypadDir--;
                        cacheKeypadDir += 6;
                        cacheKeypadDir %= 9;
                        break;
                }
            }
            KeypadDirFinalValues[i] = cacheKeypadDir + 1;
            #endregion

            #region Adding digits to the main code and logging
            if (InformationTypes[i] == 0) //Add an entry for your sub-module. This will add your sub-module's digits to the main code.
            {
                FinalAnswerExpected[i] = ListeningSounds[i];
                Debug.LogFormat("[The Forget of Many Things #{0}] The sound played is sound #{1}, therefore the answer is {1}.", _moduleID, ListeningSounds[i]);
            }
            else if (InformationTypes[i] == 1)
            {
                FinalAnswerExpected[i] = EightyOneFinalValues[i];
                Debug.LogFormat("[The Forget of Many Things #{0}] The grid displayed:", _moduleID);
                Debug.LogFormat("[The Forget of Many Things #{0}] {1}", _moduleID, (EightyOneGrids[i, 0] ? "1" : "0") + " " + (EightyOneGrids[i, 1] ? "1" : "0") + " " + (EightyOneGrids[i, 2] ? "1" : "0"));
                Debug.LogFormat("[The Forget of Many Things #{0}] {1}", _moduleID, (EightyOneGrids[i, 3] ? "1" : "0") + " " + (EightyOneGrids[i, 4] ? "1" : "0") + " " + (EightyOneGrids[i, 5] ? "1" : "0"));
                Debug.LogFormat("[The Forget of Many Things #{0}] {1}", _moduleID, (EightyOneGrids[i, 6] ? "1" : "0") + " " + (EightyOneGrids[i, 7] ? "1" : "0") + " " + (EightyOneGrids[i, 8] ? "1" : "0"));
                Debug.LogFormat("[The Forget of Many Things #{0}] Since the starting digit is {1}, the answer is {2}.", _moduleID, EightyOneInitialButtons[i], EightyOneFinalValues[i]);
            }
            else if (InformationTypes[i] == 2)
            {
                FinalAnswerExpected[i] = ForgetMeNotFinalValues[i];
                Debug.LogFormat("[The Forget of Many Things #{0}] The previous digit is {1} and the current digit is {2}, therefore the answer is {3}.", _moduleID,
                    ForgetMeNotPrevious[i], ForgetMeNotCurrent[i], ForgetMeNotFinalValues[i]);
            }
            else if (InformationTypes[i] == 3)
            {
                FinalAnswerExpected[i] = ForgetItNotValues[i];
                Debug.LogFormat("[The Forget of Many Things #{0}] The displayed digit is {1}, therefore the answer is {1}.", _moduleID, ForgetItNotValues[i]);
            }
            else if (InformationTypes[i] == 4)
            {
                FinalAnswerExpected[i] = DOMTFinalValues[i];
                Debug.LogFormat("[The Forget of Many Things #{0}] The card displayed is a{1} {2} {3} of {4}{5}.", _moduleID, new int[] { 3, 5 }.Contains(DOMTCardTypes[i]) ? "n" : "", DOMTCardTypeNames[DOMTCardTypes[i]],
                    DOMTCardTypes[i] == 2 ? DOMTCardRanks[i].ToString() : DOMTCardRanks[i] == 1 ? "Ace" : DOMTCardRanks[i] == 11 ? "Jack" : DOMTCardRanks[i] == 12 ? "Queen"
                    : DOMTCardRanks[i] == 13 ? "King" : DOMTCardRanks[i].ToString(), DOMTSuitNames[DOMTCardSuits[i]], DOMTCardTypes[i] == 4 ? DOMTCardColourNames[DOMTCardTropicColours[i]] : "");
                Debug.LogFormat("[The Forget of Many Things #{0}] This card's rank, modulo 10, equals {1}, therefore the answer is {1}.", _moduleID, DOMTFinalValues[i]);
            }
            else if (InformationTypes[i] == 5)
            {
                FinalAnswerExpected[i] = KeypadDirFinalValues[i];
                Debug.LogFormat("[The Forget of Many Things #{0}] The translations given from button {1} are {2}.", _moduleID, KeypadDirNumbers[i] + 1,
                    KeypadDirDirections[i].Select(x => new string[] { "north", "north-east", "east", "south-east", "south", "south-west", "west", "north-west" }[x]).Join(", "));
                Debug.LogFormat("[The Forget of Many Things #{0}] Performing these transformations results in landing on button {1}, therefore the answer is {1}.", _moduleID, KeypadDirFinalValues[i]);
            }
            #endregion
        }
        if (NonIgnored != 0)
        {
            string log = "";
            for (int i = 0; i < NonIgnored; i++)
            {
                log += FinalAnswerExpected[i];
                if (i % 3 == 2 && i < NonIgnored)
                    log += " ";
            }
            if (NonIgnored % 3 == 0)
                log = log.Substring(0, log.Length - 1);
            Debug.LogFormat("[The Forget of Many Things #{0}] The final code: {1}.", _moduleID, log);
        }
    }

    void ButtonPress(int pos)
    {
        StartCoroutine(AnimateButton(pos));
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Buttons[pos].transform);
        Buttons[pos].AddInteractionPunch(0.5f);
        if (Active && !Solved)
        {
            if (NonIgnored == 0)
            {
                if (pos == 9)
                    StartCoroutine(Solve());
                else
                    Audio.PlaySoundAtTransform("buzzer", Buttons[pos].transform);
            }
            else if (!InputAllow)
            {
                if (pos == 9 && Types[Page - 1] == 0)
                    Audio.PlaySoundAtTransform("sound " + ListeningSounds[Stages[Page - 1]].ToString(), Buttons[9].transform);
                else if (pos == 3 && Page != 1)
                {
                    Audio.PlaySoundAtTransform("page", Buttons[pos].transform);
                    Page--;
                    Arrows[1].SetActive(true);
                    if (Page == 1)
                        Arrows[0].SetActive(false);
                }
                else if (pos == 5 && Page != Types.Count())
                {
                    Audio.PlaySoundAtTransform("page", Buttons[pos].transform);
                    Page++;
                    Arrows[0].SetActive(true);
                    if (Page == Types.Count())
                        Arrows[1].SetActive(false);
                }
                else
                    Audio.PlaySoundAtTransform("buzzer", Buttons[pos].transform);
                if (new int[] { 3, 5 }.Contains(pos))
                {
                    StageIndicator.text = (Stages[Page - 1] + 1).ToString("000");
                    switch (Types[Page - 1]) //Add pages here.
                    {
                        case 0:
                            Display.text = "LISTENING\nPRESS 0 TO PLAY";
                            for (int j = 0; j < 10; j++)
                                LEDs[j].material.color = new Color(0, 0, 0);
                            break;
                        case 1:
                            Display.text = "81\nPREV PRESSED: " + EightyOneInitialButtons[Stages[Page - 1]];
                            for (int j = 0; j < 9; j++)
                            {
                                if (!EightyOneGrids[Stages[Page - 1], j])
                                    LEDs[j].material.color = new Color(0, 0, 0);
                                else
                                    LEDs[j].material.color = new Color(0, 1, 0);
                            }
                            LEDs[9].material.color = new Color(0, 0, 0);
                            break;
                        case 2:
                            Display.text = "FORGET ME NOT\nPREV NUM: " + ForgetMeNotPrevious[Stages[Page - 1]];
                            for (int j = 0; j < 10; j++)
                                LEDs[j].material.color = new Color(0, 0, 0);
                            LEDs[(ForgetMeNotCurrent[Stages[Page - 1]] + 9) % 10].material.color = new Color(0, 1, 0);
                            break;
                        case 3:
                            Display.text = "FORGET IT NOT\n";
                            for (int j = 0; j < 10; j++)
                                LEDs[j].material.color = new Color(0, 0, 0);
                            LEDs[(FinalAnswerExpected[Stages[Page - 1]] + 9) % 10].material.color = new Color(0, 1, 0);
                            break;
                        case 4:
                            Display.text = "THE DECK OF\nMANY THINGS";
                            for (int j = 0; j < 10; j++)
                                LEDs[j].material.color = new Color(0, 0, 0);
                            try
                            {
                                if (Stages[Page - 2] == Stages[Page - 1])
                                    Display.text = DOMTCardTypeNames[DOMTCardTypes[Stages[Page - 1]]].ToUpper() + "\n"
                                        + (DOMTCardTypes[Stages[Page - 1]] == 2 ? DOMTCardRanks[Stages[Page - 1]].ToString() : DOMTCardRanks[Stages[Page - 1]] == 1 ? "A" : DOMTCardRanks[Stages[Page - 1]] == 11 ? "J"
                                        : DOMTCardRanks[Stages[Page - 1]] == 12 ? "Q" : DOMTCardRanks[Stages[Page - 1]] == 13 ? "K" : DOMTCardRanks[Stages[Page - 1]].ToString())
                                        + " OF " + DOMTSuitNames[DOMTCardSuits[Stages[Page - 1]]].ToUpper();
                            }
                            catch { }
                            try
                            {
                                if (Stages[Page - 3] == Stages[Page - 1])
                                    Display.text = DOMTCardColourNames[DOMTCardTropicColours[Stages[Page - 1]]].ToUpper() + "\n";
                            }
                            catch { }
                            break;
                        case 5:
                            Display.text = "KEYPAD\nDIRECTIONALITY";
                            for (int j = 0; j < 10; j++)
                                LEDs[j].material.color = new Color(0, 0, 0);
                            LEDs[KeypadDirNumbers[Stages[Page - 1]]].material.color = new Color(0, 1, 0);
                            try
                            {
                                if (Stages[Page - 2] == Stages[Page - 1])
                                    Display.text = KeypadDirDirections[Stages[Page - 1]].Select(x => KeypadDirDirectionNames[x]).Join(", ") + "\n";
                            }
                            catch { }
                            break;
                    }
                }
            }
            else
            {
                if (FinalAnswerExpected[CorrectlyInputStages] == (pos + 1) % 10)
                {
                    CorrectlyInputStages++;
                    Debug.LogFormat("[The Forget of Many Things #{0}] Button {1} correctly pressed.", _moduleID, (pos + 1) % 10);
                    for (int i = 0; i < 10; i++)
                        LEDs[i].material.color = new Color(0, 0, 0);
                    int CurrentStage = CorrectlyInputStages;
                    int StartingStage = 0;
                    string DisplayedText = "";
                    while (CurrentStage > 23)
                    {
                        CurrentStage -= 12;
                        StartingStage += 12;
                    }
                    for (int i = StartingStage; i < Math.Min(StartingStage + 24, NonIgnored); i++)
                    {
                        string Digit = "-";
                        if (i < CorrectlyInputStages)
                            Digit = FinalAnswerExpected[i].ToString();
                        if (i > StartingStage)
                            if (i % 3 == 0)
                            {
                                if (i % 12 == 0)
                                    DisplayedText += "\n";
                                else
                                    DisplayedText += " ";
                            }
                        DisplayedText += Digit;
                    }
                    DisplayInputs.text = DisplayedText;
                    if (CorrectlyInputStages == NonIgnored)
                    {
                        Debug.LogFormat("[The Forget of Many Things #{0}] All digits of the code have been received!", _moduleID);
                        StartCoroutine(Solve());
                    }
                    else
                        Audio.PlaySoundAtTransform("input", DisplayInputs.transform);
                }
                else
                {
                    string log = "";
                    if (CorrectlyInputStages != 0)
                    {
                        for (int i = 0; i < CorrectlyInputStages; i++)
                        {
                            log += FinalAnswerExpected[i];
                            if (i % 3 == 2 && i < CorrectlyInputStages)
                                log += " ";
                        }
                        if (CorrectlyInputStages % 3 == 0)
                            log = log.Substring(0, log.Length - 1);
                    }
                    Debug.LogFormat("[The Forget of Many Things #{0}] Button {1} incorrectly pressed. Strike! Current input is {2}", _moduleID, pos, log == "" ? "empty" : log);
                    Module.HandleStrike();
                    LEDs[(FinalAnswerExpected[CorrectlyInputStages] + 9) % 10].material.color = new Color(0, 1, 0);
                }
            }
        }
    }

    private IEnumerator AnimateButton(int pos)
    {
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition -= new Vector3(0, 0.005f / 3, 0);
            yield return null;
        }
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition += new Vector3(0, 0.005f / 3, 0);
            yield return null;
        }
    }

    private IEnumerator SolveCheck()
    {
        while (true)
        {
            if (SolvedCheck != Bomb.GetSolvedModuleNames().Count(x => !IgnoredModules.Contains(x)) && Stage + 1 < NonIgnored)
            {
                SolvedCheck = Bomb.GetSolvedModuleNames().Count(x => !IgnoredModules.Contains(x));
                Stage++;
                if (Active)
                    Activate();
                else
                    PreActiveSolves++;
            }
            else if (SolvedCheck != Bomb.GetSolvedModuleNames().Count(x => !IgnoredModules.Contains(x)))
            {
                Debug.LogFormat("[The Forget of Many Things #{0}] Initialising the input phase...", _moduleID);
                for (int i = 0; i < 2; i++)
                    Arrows[i].SetActive(false);
                try
                {
                    GameMusicControl.GameMusicVolume = 0.0f;
                }
                catch { }
                for (int i = 0; i < 10; i++)
                    LEDs[i].material.color = new Color(0, 0, 0);
                StageIndicator.text = "---";
                Display.text = "";
                DisplayInputs.text = "";
                int DashCount = 0;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            if (DashCount < NonIgnored)
                            {
                                DisplayInputs.text += "-";
                                if (_moduleID == HighestID)
                                    Audio.PlaySoundAtTransform("key", DisplayInputs.transform);
                                ModuleObject.AddInteractionPunch(0.5f);
                                DashCount++;
                                yield return new WaitForSecondsRealtime(0.05f);
                            }
                        }
                        if (j != 3 && DashCount < NonIgnored)
                            DisplayInputs.text += " ";
                    }
                    if (i == 0 && DashCount < NonIgnored)
                        DisplayInputs.text += "\n";
                }
                yield return new WaitForSecondsRealtime(0.05f);
                if (_moduleID == HighestID)
                    Sound = Audio.PlaySoundAtTransformWithRef("input music", transform);
                InputAllow = true;
                Debug.LogFormat("[The Forget of Many Things #{0}] Input enabled. Good luck!", _moduleID);
                break;
            }
            yield return null;
        }
    }

    private IEnumerator Solve()
    {
        Debug.LogFormat("[The Forget of Many Things #{0}] Module solved!", _moduleID);
        Module.HandlePass();
        Audio.PlaySoundAtTransform("solve", Buttons[4].transform);
        InputAllow = false;
        Solved = true;
        Display.characterSize *= 2;
        DisplayInputs.text = "";
        Display.text = "SOLVED";
        for (int i = 0; i < 50; i++)
        {
            int Rand1 = Rnd.Range(0, 10);
            int Rand2 = Rnd.Range(0, 10);
            if (i < 25)
                StageIndicator.text = Rand1 + "" + Rand2;
            else
                StageIndicator.text = "G" + Rand2;
            yield return new WaitForSecondsRealtime(0.025f);
        }
        StageIndicator.text = "GG";
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} 1234567890' to press the buttons with those labels.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string validcmds = "0123456789";
        if (command.Length == 0)
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        for (int i = 0; i < command.Length; i++)
        {
            if (!validcmds.Contains(command[i]))
            {
                yield return "sendtochaterror Invalid command.";
                yield break;
            }
        }
        yield return null;
        for (int i = 0; i < command.Length; i++)
        {
            Debug.Log(command[i]);
            Buttons[command[i] - '0'].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

    }
    IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat("[The Forget of Many Things #{0}] Twitch Plays autosolving...", _moduleID);
        if (NonIgnored == 0)
            Buttons[9].OnInteract();
        else
        {
            while (!InputAllow)
                yield return true;
            for (int i = CorrectlyInputStages; i < FinalAnswerExpected.Length; i++)
            {
                Buttons[(FinalAnswerExpected[i] + 9) % 10].OnInteract();
                yield return new WaitForSeconds(0.05f);
                yield return true;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class Towers : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable[] Buttons;
    public KMSelectable Switch;

    public TextMesh[] BottomClues;
    public TextMesh[] LeftClues;
    public TextMesh[] RightClues;
    public TextMesh[] TopClues;

    public GameObject[] Cubes;
    public GameObject[] TheHighlights;

    public Transform TheSwitchToFlip;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    int[][] TowerSizes = new int[5][] {
      new int[] {19, 23, 36, 47, 53},
      new int[] {19, 23, 36, 47, 53},
      new int[] {19, 23, 36, 47, 53},
      new int[] {19, 23, 36, 47, 53},
      new int[] {19, 23, 36, 47, 53}
    };
    int[] TowerChecking = new int[25];
    int[] TowerPlacements = new int[25];
    int ClueObtainer;
    int TallestTower;

    bool _togglingSwitch;
    bool Adding = true;

    void Awake () {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable Button in Buttons) {
            Button.OnInteract += delegate () { ButtonPress(Button); return false; };
        }
      Switch.OnInteract += delegate () { SwitchFlip(); return false; };
    }

    void Start () {
      for (int i = 0; i < Cubes.Length; i++)
        Cubes[i].gameObject.SetActive(false);
      for (int i = 0; i < 25; i++) {
        Cubes[i * 5].gameObject.SetActive(true);
        TowerPlacements[i] = 1;
        TowerChecking[i] = 1;
      }
      Reroll:
      for (int i = 0; i < 5; i++)
        TowerSizes[i].Shuffle();
      for (int i = 0; i < 5; i++) {
        if (TowerSizes[0][i] + TowerSizes[1][i] + TowerSizes[2][i] + TowerSizes[3][i] + TowerSizes[4][i] != 178)
          goto Reroll;
      }
      Debug.LogFormat("[Towers #{0}] The puzzle is as follows:", moduleId);
      Debug.LogFormat("[Towers #{0}] {1}", moduleId, (TowerSizes[0][0] / 10).ToString() + (TowerSizes[0][1] / 10).ToString() + (TowerSizes[0][2] / 10).ToString() + (TowerSizes[0][3] / 10).ToString() + (TowerSizes[0][4] / 10).ToString());
      Debug.LogFormat("[Towers #{0}] {1}", moduleId, (TowerSizes[1][0] / 10).ToString() + (TowerSizes[1][1] / 10).ToString() + (TowerSizes[1][2] / 10).ToString() + (TowerSizes[1][3] / 10).ToString() + (TowerSizes[1][4] / 10).ToString());
      Debug.LogFormat("[Towers #{0}] {1}", moduleId, (TowerSizes[2][0] / 10).ToString() + (TowerSizes[2][1] / 10).ToString() + (TowerSizes[2][2] / 10).ToString() + (TowerSizes[2][3] / 10).ToString() + (TowerSizes[2][4] / 10).ToString());
      Debug.LogFormat("[Towers #{0}] {1}", moduleId, (TowerSizes[3][0] / 10).ToString() + (TowerSizes[3][1] / 10).ToString() + (TowerSizes[3][2] / 10).ToString() + (TowerSizes[3][3] / 10).ToString() + (TowerSizes[3][4] / 10).ToString());
      Debug.LogFormat("[Towers #{0}] {1}", moduleId, (TowerSizes[4][0] / 10).ToString() + (TowerSizes[4][1] / 10).ToString() + (TowerSizes[4][2] / 10).ToString() + (TowerSizes[4][3] / 10).ToString() + (TowerSizes[4][4] / 10).ToString());
      for (int x = 0; x < 5; x++) {
        for (int i = 0; i < 5; i++) {
          if (TowerSizes[x][i] > TallestTower) {
            TallestTower = TowerSizes[x][i];
            ClueObtainer++;
          }
        }
        LeftClues[x].text = ClueObtainer.ToString();
        ClueObtainer = 0;
        TallestTower = 0;
      }

      for (int x = 0; x < 5; x++) {
        for (int i = 4; i > -1; i--) {
          if (TowerSizes[x][i] > TallestTower) {
            TallestTower = TowerSizes[x][i];
            ClueObtainer++;
          }
        }
        RightClues[x].text = ClueObtainer.ToString();
        ClueObtainer = 0;
        TallestTower = 0;
      }

      for (int x = 0; x < 5; x++) {
        for (int i = 0; i < 5; i++) {
          if (TowerSizes[i][x] > TallestTower) {
            TallestTower = TowerSizes[i][x];
            ClueObtainer++;
          }
        }
        TopClues[x].text = ClueObtainer.ToString();
        ClueObtainer = 0;
        TallestTower = 0;
      }

      for (int x = 0; x < 5; x++) {
        for (int i = 4; i > -1; i--) {
          if (TowerSizes[i][x] > TallestTower) {
            TallestTower = TowerSizes[i][x];
            ClueObtainer++;
          }
        }
        BottomClues[x].text = ClueObtainer.ToString();
        ClueObtainer = 0;
        TallestTower = 0;
      }
      Debug.LogFormat("[Towers #{0}] The clues along the top are {1} {2} {3} {4} {5}.", moduleId, TopClues[0].text, TopClues[1].text, TopClues[2].text, TopClues[3].text, TopClues[4].text);
      Debug.LogFormat("[Towers #{0}] The clues along the left are {1} {2} {3} {4} {5}.", moduleId, LeftClues[0].text, LeftClues[1].text, LeftClues[2].text, LeftClues[3].text, LeftClues[4].text);
      Debug.LogFormat("[Towers #{0}] The clues along the bottom are {1} {2} {3} {4} {5}.", moduleId, BottomClues[0].text, BottomClues[1].text, BottomClues[2].text, BottomClues[3].text, BottomClues[4].text);
      Debug.LogFormat("[Towers #{0}] The clues along the right are {1} {2} {3} {4} {5}.", moduleId, RightClues[0].text, RightClues[1].text, RightClues[2].text, RightClues[3].text, RightClues[4].text);
    }

    void ButtonPress (KMSelectable Button) {
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
      for (int i = 0; i < 25; i++) {
        if (Button == Buttons[i]) {
          if (Adding) {
            if (TowerPlacements[i] != 4 && Cubes[i * 5 + TowerPlacements[i]].gameObject.activeSelf)
              TowerPlacements[i]++;
            Cubes[i * 5 + TowerPlacements[i]].gameObject.SetActive(true);
            if (TowerPlacements[i] != 4) {
              TowerPlacements[i]++;
              TowerChecking[i] = TowerPlacements[i];
            }
            else
              TowerChecking[i] = 5;
          }
          else {
            if (TowerPlacements[i] != 1 && !Cubes[i * 5 + TowerPlacements[i]].gameObject.activeSelf)
              TowerPlacements[i]--;
            Cubes[i * 5 + TowerPlacements[i]].gameObject.SetActive(false);
            if (TowerPlacements[i] != 1) {
              TowerPlacements[i]--;
              TowerChecking[i] = TowerPlacements[i];
            }
            else
              TowerChecking[i] = 1;
          }
        }
      }
      int Check = 0;
      for (int i = 0; i < 5; i++)
        Check += TowerChecking[i];
      if (Check != 15)
        return;
      for (int i = 5; i < 10; i++)
        Check += TowerChecking[i];
      if (Check != 30)
        return;
      for (int i = 10; i < 15; i++)
        Check += TowerChecking[i];
      if (Check != 45)
        return;
      for (int i = 15; i < 20; i++)
        Check += TowerChecking[i];
      if (Check != 60)
        return;
      for (int i = 20; i < 25; i++)
        Check += TowerChecking[i];
      if (Check != 75)
        return;
      SolveChecker();
    }

    void SwitchFlip () {
      Adding = !Adding;
      if (Adding) {
        StartCoroutine(toggleSwitch(0, 30));
        Audio.PlaySoundAtTransform("Switch1", Switch.transform);
      }
      else {
        StartCoroutine(toggleSwitch(30, 0));
        Audio.PlaySoundAtTransform("Switch2", Switch.transform);
      }
    }

    private IEnumerator toggleSwitch (float from, float to) {
        while (_togglingSwitch)
            yield return null;
        _togglingSwitch = true;

        var cur = from;
        var stop = false;
        while (!stop)
        {
            cur += 250 * Time.deltaTime * (to > from ? 1 : -1);
            if ((to > from && cur >= to) || (to < from && cur <= to))
            {
                cur = to;
                stop = true;
            }
            TheSwitchToFlip.localEulerAngles = new Vector3(90, 330 + cur, 0);
            yield return null;
        }
        _togglingSwitch = false;
    }

    void SolveChecker () {
      if (moduleSolved)
        return;
      for (int x = 0; x < 5; x++) {
        for (int i = 0; i < 5; i++) {
          if (TowerChecking[x * 5 + i] > TallestTower) {
            TallestTower = TowerChecking[x * 5 + i];
            ClueObtainer++;
          }
        }
        if (LeftClues[x].text != ClueObtainer.ToString()) {
          ClueObtainer = 0;
          TallestTower = 0;
          return;
        }

        ClueObtainer = 0;
        TallestTower = 0;
      }

      for (int x = 0; x < 5; x++) {
        for (int i = 4; i > -1; i--) {
          if (TowerChecking[x * 5 + i] > TallestTower) {
            TallestTower = TowerChecking[x * 5 + i];
            ClueObtainer++;

          }
        }
        if (RightClues[x].text != ClueObtainer.ToString()) {
          ClueObtainer = 0;
          TallestTower = 0;
          return;
        }
        ClueObtainer = 0;
        TallestTower = 0;
      }

      for (int x = 0; x < 5; x++) {
        for (int i = 0; i < 5; i++) {
          if (TowerChecking[i * 5 + x] > TallestTower) {
            TallestTower = TowerChecking[i * 5 + x];
            ClueObtainer++;
          }
        }
        if (TopClues[x].text != ClueObtainer.ToString()) {
          ClueObtainer = 0;
          TallestTower = 0;
          return;
        }
        ClueObtainer = 0;
        TallestTower = 0;
      }

      for (int x = 0; x < 5; x++) {
        for (int i = 4; i > -1; i--) {
          if (TowerChecking[i * 5 + x] > TallestTower) {
            TallestTower = TowerChecking[i * 5 + x];
            ClueObtainer++;
          }
        }
        if (BottomClues[x].text != ClueObtainer.ToString()) {
          ClueObtainer = 0;
          TallestTower = 0;
          return;
        }
        ClueObtainer = 0;
        TallestTower = 0;
      }

      GetComponent<KMBombModule>().HandlePass();
      StartCoroutine(SolveAnimation());
      moduleSolved = true;
    }

    IEnumerator SolveAnimation () {
      for (int i = 0; i < 125; i++) {
        for (int j = 0; j < 125; j++)
          Cubes[i].transform.Translate(0.0f, -0.0005f, 0.0f);
        yield return new WaitForSeconds(.01f);
      }
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} A1-E5 to toggle that square. Use !{0} switch to switch between adding and subtracting. Chain with spaces.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string Command) {
      string[] Parameters = Command.Trim().ToUpper().Split(' ');
      yield return null;
      for (int i = 0; i < Parameters.Length; i++) {
        if (Parameters[i] == "SWITCH")
          Switch.OnInteract();
        else if (Parameters[i] == "A1")
          Buttons[0].OnInteract();
        else if (Parameters[i] == "B1")
          Buttons[1].OnInteract();
        else if (Parameters[i] == "C1")
          Buttons[2].OnInteract();
        else if (Parameters[i] == "D1")
          Buttons[3].OnInteract();
        else if (Parameters[i] == "E1")
          Buttons[4].OnInteract();
        else if (Parameters[i] == "A2")
          Buttons[5].OnInteract();
        else if (Parameters[i] == "B2")
          Buttons[6].OnInteract();
        else if (Parameters[i] == "C2")
          Buttons[7].OnInteract();
        else if (Parameters[i] == "D2")
          Buttons[8].OnInteract();
        else if (Parameters[i] == "E2")
          Buttons[9].OnInteract();
        else if (Parameters[i] == "A3")
          Buttons[10].OnInteract();
        else if (Parameters[i] == "B3")
          Buttons[11].OnInteract();
        else if (Parameters[i] == "C3")
          Buttons[12].OnInteract();
        else if (Parameters[i] == "D3")
          Buttons[13].OnInteract();
        else if (Parameters[i] == "E3")
          Buttons[14].OnInteract();
        else if (Parameters[i] == "A4")
          Buttons[15].OnInteract();
        else if (Parameters[i] == "B4")
          Buttons[16].OnInteract();
        else if (Parameters[i] == "C4")
          Buttons[17].OnInteract();
        else if (Parameters[i] == "D4")
          Buttons[18].OnInteract();
        else if (Parameters[i] == "E4")
          Buttons[19].OnInteract();
        else if (Parameters[i] == "A5")
          Buttons[20].OnInteract();
        else if (Parameters[i] == "B5")
          Buttons[21].OnInteract();
        else if (Parameters[i] == "C5")
          Buttons[22].OnInteract();
        else if (Parameters[i] == "D5")
          Buttons[23].OnInteract();
        else if (Parameters[i] == "E5")
          Buttons[24].OnInteract();
        else
          yield return "sendtochaterror I don't understand!";
        yield return new WaitForSecondsRealtime(.1f);
      }
    }

    IEnumerator TwitchHandleForcedSolve () {
      for (int i = 0; i < 5; i++) {
        for (int x = 0; x < 5; x++) {
          while (TowerChecking[i * 5 + x] != (TowerSizes[i][x] / 10)) {
            if (TowerChecking[i * 5 + x] < (TowerSizes[i][x] / 10)) {
              if (!Adding) {
                Switch.OnInteract();
                yield return new WaitForSecondsRealtime(.1f);
              }
              Buttons[i * 5 + x].OnInteract();
              yield return new WaitForSecondsRealtime(.1f);
            }
            else if (TowerChecking[i * 5 + x] > (TowerSizes[i][x] / 10)) {
              if (Adding) {
                Switch.OnInteract();
                yield return new WaitForSecondsRealtime(.1f);
              }
              Buttons[i * 5 + x].OnInteract();
            }
          }
        }
      }
    }
}

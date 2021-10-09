using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
//using ExGrid = TowersGenerations.TowersForGenerate;

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

   /*void GeneratePuzzle () {
      TowerSizes[0].Shuffle();
   }*/

   void Start () {
      for (int i = 0; i < Cubes.Length; i++) {
         Cubes[i].gameObject.SetActive(false);
      }

      TowersGenerations.TowersForGenerate Board = new TowersGenerations.TowersForGenerate(5);
      for (int i = 0; i < 15; i++) {
         Board.Permutate(Rnd.Range(0, 2) == 1);
      }
      for (int i = 0; i < 5; i++) {
         for (int j = 0; j < 5; j++) {
            TowerSizes[i][j] = Board.GetField(i, j);
         }
      }

      Debug.LogFormat("[Towers #{0}] The puzzle is as follows:", moduleId);
      Debug.LogFormat("[Towers #{0}] {1} {2} {3} {4} {5}", moduleId, TowerSizes[0][0], TowerSizes[0][1], TowerSizes[0][2], TowerSizes[0][3], TowerSizes[0][4]);
      Debug.LogFormat("[Towers #{0}] {1} {2} {3} {4} {5}", moduleId, TowerSizes[1][0], TowerSizes[1][1], TowerSizes[1][2], TowerSizes[1][3], TowerSizes[1][4]);
      Debug.LogFormat("[Towers #{0}] {1} {2} {3} {4} {5}", moduleId, TowerSizes[2][0], TowerSizes[2][1], TowerSizes[2][2], TowerSizes[2][3], TowerSizes[2][4]);
      Debug.LogFormat("[Towers #{0}] {1} {2} {3} {4} {5}", moduleId, TowerSizes[3][0], TowerSizes[3][1], TowerSizes[3][2], TowerSizes[3][3], TowerSizes[3][4]);
      Debug.LogFormat("[Towers #{0}] {1} {2} {3} {4} {5}", moduleId, TowerSizes[4][0], TowerSizes[4][1], TowerSizes[4][2], TowerSizes[4][3], TowerSizes[4][4]);

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
      if (moduleSolved) {
         return;
      }
      for (int i = 0; i < 25; i++) {
         if (Button == Buttons[i]) {
            if (Adding) {
               if (TowerPlacements[i] != 4 && Cubes[i * 5 + TowerPlacements[i]].gameObject.activeSelf) {
                  TowerPlacements[i]++;
               }
               Cubes[i * 5 + TowerPlacements[i]].gameObject.SetActive(true);
               if (TowerPlacements[i] != 4) {
                  TowerPlacements[i]++;
                  TowerChecking[i] = TowerPlacements[i];
               }
               else {
                  TowerChecking[i] = 5;
               }
            }
            else {
               if (TowerPlacements[i] == 0) {
                  return;
               }
               if (TowerPlacements[i] != 0 && !Cubes[i * 5 + TowerPlacements[i]].gameObject.activeSelf) {
                  TowerPlacements[i]--;
               }
               Cubes[i * 5 + TowerPlacements[i]].gameObject.SetActive(false);
               if (TowerPlacements[i] != 0) {
                  TowerChecking[i] = TowerPlacements[i];
               }
            }
         }
      }
      int Check = 0;
      for (int i = 0; i < 5; i++) {       //Each if statement checks if the row contains the sum of all numbers from 1-5 (15), while each for loop checks the column
         Check += TowerChecking[i];
      }
      if (Check != 15) {
         return;
      }
      for (int i = 5; i < 10; i++) {
         Check += TowerChecking[i];
      }
      if (Check != 30) {
         return;
      }
      for (int i = 10; i < 15; i++) {
         Check += TowerChecking[i];
      }
      if (Check != 45) {
         return;
      }
      for (int i = 15; i < 20; i++) {
         Check += TowerChecking[i];
      }
      if (Check != 60) {
         return;
      }
      for (int i = 20; i < 25; i++) {
         Check += TowerChecking[i];
      }
      if (Check != 75) {
         return;
      }
      SolveChecker();
   }

   void SwitchFlip () {
      Adding = !Adding;
      if (Adding) {
         StartCoroutine(toggleSwitch(0, 30));
         Audio.PlaySoundAtTransform("Switch1", Switch.transform);
         if (moduleSolved) {
            StartCoroutine(RiseAnimation());
         }
      }
      else {
         StartCoroutine(toggleSwitch(30, 0));
         Audio.PlaySoundAtTransform("Switch2", Switch.transform);
         if (moduleSolved) {
            StartCoroutine(FallAnimation());
         }
      }
   }

   private IEnumerator toggleSwitch (float from, float to) {   //Switch animation
      while (_togglingSwitch) {
         yield return null;
      }
      _togglingSwitch = true;

      var cur = from;
      var stop = false;
      while (!stop) {
         cur += 250 * Time.deltaTime * (to > from ? 1 : -1);
         if ((to > from && cur >= to) || (to < from && cur <= to)) {
            cur = to;
            stop = true;
         }
         TheSwitchToFlip.localEulerAngles = new Vector3(90, 330 + cur, 0);
         yield return null;
      }
      _togglingSwitch = false;
   }

   void SolveChecker () {                                      //Checks if the given answer can be valid. Remember that I am not good at this, so multiple solutions may be possible.
      if (moduleSolved) {
         return;
      }
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
      if (!Adding) {
         StartCoroutine(FallAnimation());
      }
      moduleSolved = true;
   }

   IEnumerator FallAnimation () {
      for (int i = 0; i < 125; i++) {
         for (int j = 0; j < 125; j++) {
            Cubes[j].transform.Translate(0.0f, -0.0005f, 0.0f);
         }
         yield return new WaitForSeconds(.01f);
      }
   }

   IEnumerator RiseAnimation () {
      for (int i = 0; i < 125; i++) {
         for (int j = 0; j < 125; j++) {
            Cubes[j].transform.Translate(0.0f, 0.0005f, 0.0f);
         }
         yield return new WaitForSeconds(.01f);
      }
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} A1-E5 to toggle that square. Use !{0} switch to switch between adding and subtracting. Chain with spaces.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      string[] Parameters = Command.Trim().ToUpper().Split(' ');
      string[] Coordinates = { "A1", "B1", "C1", "D1", "E1", "A2", "B2", "C2", "D2", "E2", "A3", "B3", "C3", "D3", "E3", "A4", "B4", "C4", "D4", "E4", "A5", "B5", "C5", "D5", "E5"};
      yield return null;
      for (int i = 0; i < Parameters.Length; i++) {
         if (Parameters[i] != "SWITCH" && !Coordinates.Contains(Parameters[i])) {
            yield return "sendtochaterror I don't understand!";
            yield break;
         }
      }
      for (int i = 0; i < Parameters.Length; i++) {
         if (Parameters[i] == "SWITCH") {
            Switch.OnInteract();
         }
         for (int j = 0; j < 25; j++) {
            if (Parameters[i] == Coordinates[j]) {
               Buttons[j].OnInteract();
            }
         }
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

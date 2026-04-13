using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colonThreeModule : MonoBehaviour
{

	public TextMesh activationsText, timerText, currentPointsText, goalPointsText;
	public KMSelectable colonThreeButton, colonBracketButton, angyColonThreeButton, angyColonBracketButton;
	public KMAudio Audio;
	public KMBombModule Module;
	public MeshRenderer[] ledRenderers;
	public KMBombInfo BombInfo;
	public AudioClip warnSound, pressSound, solveSound;
	
	int activationsCount;
	private int timer = 600;
	private bool canInput;
	private int currentPoints;
	private int goalPoints;
	private bool zeroTime;
	private bool ModuleSolved;
	private bool TwitchPlaysActive;

	
	bool? buttonCheck(int button)
	{
		if (!canInput || ModuleSolved) return null;
		switch (button)
		{
			case 0: return (activationsCount + 1) % 5 != 0;
			case 1: return (activationsCount + 1) % 5 == 0 && (activationsCount + 1) % 20 != 0;
			case 2: return (activationsCount + 1) % 20 == 0 && (activationsCount + 1) % 100 != 0;
			case 3: return (activationsCount + 1) % 100 == 0;
			default: return null;
		}
	}
	
	void Start () {
		zeroTime = BombInfo.GetTime() == 0f;
		goalPoints = zeroTime?6000:(int)(BombInfo.GetTime()*10);
		foreach (MeshRenderer ledRenderer in ledRenderers) ledRenderer.material.color =  Color.black;

		goalPointsText.text = goalPoints.ToString();
		activationsText.text = activationsCount.ToString();
		timerText.text = timer.ToString();
		currentPointsText.text = currentPoints.ToString();
		
		colonThreeButton.OnInteract += delegate { HandlePress(0); return false; };
		colonBracketButton.OnInteract += delegate { HandlePress(1); return false; };
		angyColonThreeButton.OnInteract += delegate { HandlePress(2); return false; };
		angyColonBracketButton.OnInteract += delegate { HandlePress(3); return false; };

		StartCoroutine(TimerRoutine());
	}
	
	IEnumerator TimerRoutine()
	{
		float lastBombTime = BombInfo.GetTime();

		while (!ModuleSolved)
		{
			float currentBombTime = BombInfo.GetTime();
			if (lastBombTime - currentBombTime >= 0.1f)
			{
				lastBombTime = currentBombTime;
				timer--;
				timerText.text = timer.ToString();
				if (timer <= 0)
				{
					if (canInput)
					{
						Module.HandleStrike();
						currentPoints -= 200;
						currentPointsText.text = currentPoints.ToString();
					}
					else {Audio.HandlePlaySoundAtTransform(warnSound.name, transform);}
					canInput = !canInput;
					SetTimer(200 * (TwitchPlaysActive?3:1));
				}
			}
			yield return null;
		}
	}
	
	void HandlePress(int button)
	{
		if (!canInput || ModuleSolved)
			return;

		bool? result = buttonCheck(button);

		if (result == null) return;
		currentPoints += timer;
		currentPointsText.text = currentPoints.ToString();
		canInput = false;
		SetTimer((int)(timer * 1.5f));
		if (result.Value)
		{
			activationsCount++;
			activationsText.text = activationsCount.ToString();
			Audio.HandlePlaySoundAtTransform(pressSound.name, transform);
			if (currentPoints < goalPoints) return;
			foreach (MeshRenderer ledRenderer in ledRenderers)
				ledRenderer.material.color = Color.green;
			Module.HandlePass();
			Audio.HandlePlaySoundAtTransform(solveSound.name, transform);
			ModuleSolved = true;
			timerText.text = ":3c";
		}
		else
		{
			Module.HandleStrike();
			currentPoints -= 200;
			currentPointsText.text = currentPoints.ToString();
		}
	}
	
	void SetTimer(int value)
	{
		if (!canInput) value = Mathf.Min(value, goalPoints/2);
		timer = value;
		timerText.text = timer.ToString();
		foreach (MeshRenderer ledRenderer in ledRenderers)
			ledRenderer.material.color = canInput ? Color.red : Color.black;
	}

	void Update()
	{
		if (ModuleSolved || (BombInfo.GetTime() == 0 && !zeroTime) || zeroTime) return;
		goalPoints = (int)(BombInfo.GetTime() * 10);
		goalPointsText.text = goalPoints.ToString();
	}
	
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} :3 / :] / >:3 / >:] to press corresponding button.";
#pragma warning restore 414
	
	public IEnumerator ProcessTwitchCommand(string Command)
	{
		yield return null;
		switch (Command)
		{
			case ":3": HandlePress(0); break;
			case ":]": HandlePress(1); break;
			case ">:3": HandlePress(2); break;
			case ">:]": HandlePress(3); break;
			default:
			{
				yield return "sendtochaterror Invalid command.";
				yield break;
			}
		}
	}

	public IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		goalPointsText.text = "0";
		ModuleSolved = true;
		Module.HandlePass();
	}
}

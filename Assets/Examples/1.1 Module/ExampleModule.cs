using System.Collections;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class ExampleModule : MonoBehaviour
{
    public KMSelectable[] buttons;

    int correctIndex;
    bool isActivated = false;

    void Start()
    {
        Init();

        GetComponent<KMBombModule>().OnActivate += ActivateModule;
    }

    void Init()
    {
        correctIndex = Rnd.Range(0, 4);

        for(int i = 0; i < buttons.Length; i++)
        {
            string label = i == correctIndex ? "O" : "X";

            TextMesh buttonText = buttons[i].GetComponentInChildren<TextMesh>();
            buttonText.text = label;
            int j = i;
            buttons[i].OnInteract += delegate () { OnPress(j == correctIndex); return false; };
        }
    }

    void ActivateModule()
    {
        isActivated = true;
    }

    void OnPress(bool correctButton)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();
        if (correctButton)
        {
            GetComponent<KMBombModule>().HandlePass();
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
        }
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "sus";
#pragma warning restore 414
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        buttons[correctIndex].OnInteract();
    }
}

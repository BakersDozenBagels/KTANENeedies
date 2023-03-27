using System;
using System.Collections;
using UnityEngine;

public class ThueMorse : MonoBehaviour
{
    private int _stage, _id = ++_idc;
    private KMAudio _audio;
    private KMNeedyModule _module;
    [SerializeField]
    private TextMesh _text;
    private bool _active;
    KMSelectable[] _children;
    private static int _idc;

    private void Start()
    {
        _audio = GetComponent<KMAudio>();
        _stage = 0;
        _children = GetComponent<KMSelectable>().Children;
        _children[0].OnInteract += () => { Press(0); _children[0].AddInteractionPunch(); _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Stamp, _children[0].transform); return false; };
        _children[1].OnInteract += () => { Press(1); _children[1].AddInteractionPunch(); _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Stamp, _children[1].transform); return false; };
        _module = GetComponent<KMNeedyModule>();
        _module.OnTimerExpired += Strike;
        _module.OnNeedyActivation += Activate;
        _module.OnNeedyDeactivation += () => { _active = false; };
        _text.text = "";
    }

    private void Activate()
    {
        Debug.LogFormat("[Thue Morse #{0}] Activated.", _id);
        _text.text = "";
        _active = true;
    }

    private void Press(int i)
    {
        if(!_active)
            return;

        if(i == CalculateThueMorse(_stage))
        {
            Debug.LogFormat("[Thue Morse #{0}] Pressed {1}. Correct.", _id, i == 0 ? "Thue" : "Morse");

            _audio.PlaySoundAtTransform("TMCorrect", transform);
            _module.HandlePass();
            _stage++;
            _active = false;
        }
        else
        {
            Debug.LogFormat("[Thue Morse #{0}] Pressed {1}. Wrong. Strike!", _id, i == 0 ? "Thue" : "Morse");
            Strike();
        }
    }

    private void Strike()
    {
        _audio.PlaySoundAtTransform("RuleBreak", transform);
        _module.HandleStrike();
        _module.HandlePass();
        _text.text = Convert.ToString(_stage, 2);
        _stage++;
        _active = false;
    }

    private static int CalculateThueMorse(int step, int playerCount = 2)
    {
        int tot = 0;
        while(step > 0)
            tot += DivMod(ref step, playerCount);
        return tot % playerCount;
    }

    private static int DivMod(ref int a, int b)
    {
        int res = a % b;
        a /= b;
        return res;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use ""!{0} thue|morse"" to press that button.";
#pragma warning restore 414
    private KMSelectable[] ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        if(command == "thue")
            return new KMSelectable[] { _children[0] };
        if(command == "morse")
            return new KMSelectable[] { _children[1] };
        return null;
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat("[Thue Morse #{0}] Autosolving...", _id);
        StartCoroutine(UpdateTimer());
        while(true)
        {
            yield return true;
            if(_active)
                _children[CalculateThueMorse(_stage)].OnInteract();
        }
    }

    private IEnumerator UpdateTimer()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            if(_active && _module.GetNeedyTimeRemaining() < 5f)
                _module.SetNeedyTimeRemaining(10f);
        }
    }
}

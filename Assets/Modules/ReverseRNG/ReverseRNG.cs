using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class ReverseRNG : MonoBehaviour
{
    private KMNeedyModule _module;
    private bool _active, _focused;
    private int _id = ++_idc;
    private static int _idc;
    private KMSelectable[] _buttons;
    private KMAudio _audio;

    private readonly List<int> _prevInputs = new List<int>(5);
    private readonly Dictionary<string, float[]> _weights = new Dictionary<string, float[]>();

    private void Start()
    {
        _module = GetComponent<KMNeedyModule>();
        _module.OnNeedyActivation += Begin;
        _module.OnTimerExpired += End;
        _audio = GetComponent<KMAudio>();
        KMSelectable sel = GetComponent<KMSelectable>();
        _buttons = sel.Children;
        _buttons[0].OnInteract += () => { Press(0); _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, _buttons[0].transform); _buttons[0].AddInteractionPunch(); return false; };
        _buttons[1].OnInteract += () => { Press(1); _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, _buttons[1].transform); _buttons[1].AddInteractionPunch(); return false; };
        sel.OnFocus += () => { _focused = true; };
        sel.OnDefocus += () => { _focused = false; };
        StartCoroutine(LogTime());
    }

    private IEnumerator LogTime()
    {
        while(true)
        {
            yield return new WaitForSeconds(10f);
            if(_active)
                Debug.LogFormat("[Reversed Random Number Generator #{0}] Current timer: {1}", _id, (int)_module.GetNeedyTimeRemaining());
        }
    }

    private void Update()
    {
        if(!_focused)
            return;
        if(Input.GetKeyDown(KeyCode.F))
            Press(0);
        if(Input.GetKeyDown(KeyCode.P))
            Press(1);
    }

    private void Press(int i)
    {
        int p = Predict();
        Debug.LogFormat("[Reversed Random Number Generator #{0}] Prediction: {1} Pressed: {2}", _id, p == 0 ? "p" : "f", i == 0 ? "p" : "f");
        if(i != p)
            _module.SetNeedyTimeRemaining(Mathf.Min(99f, _module.GetNeedyTimeRemaining() + 2f));
        else
            _module.SetNeedyTimeRemaining(_module.GetNeedyTimeRemaining() - 1f);
        UpdateDict(i);
    }

    private void UpdateDict(int i)
    {
        string s = _prevInputs.Join("");
        if(_prevInputs.Count >= 5)
            _prevInputs.RemoveAt(0);
        _prevInputs.Add(i);

        if(!_weights.ContainsKey(s))
            _weights[s] = new float[] { 0f, 0f };

        _weights[s][0] = (_weights[s][0] * _weights[s][1] + i) / ++_weights[s][1];
    }

    private int Predict()
    {
        if(_weights.ContainsKey(_prevInputs.Join("")))
            return _weights[_prevInputs.Join("")][0] > 0.5f ? 1 : 0;
        return Random.Range(0, 1);
    }

    private void End()
    {
        Debug.LogFormat("[Reversed Random Number Generator #{0}] Time ran out. Strike!", _id);
        _active = false;
        _module.HandleStrike();
        _module.HandlePass();
    }

    private void Begin()
    {
        if(_active)
            return;

        Debug.LogFormat("[Reversed Random Number Generator #{0}] Activating...", _id);
        _active = true;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use ""!{0} fpfppfffpfppfppppffpff"" to press those buttons.";
#pragma warning restore 414
    private KMSelectable[] ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Regex rx = new Regex(@"[fp]+");
        if(!rx.IsMatch(command))
            return null;
        return command.Select(c => c == 'f' ? 0 : 1).Select(i => _buttons[i]).ToArray();
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat("[Thue Morse #{0}] Autosolving...", _id);
        StartCoroutine(UpdateTimer());
        while(true)
        {
            yield return true;
            if(_active)
            {
                while(_module.GetNeedyTimeRemaining() < 90)
                {
                    _buttons[1 - Predict()].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
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

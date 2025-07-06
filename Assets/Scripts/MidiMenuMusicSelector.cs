// ===============================
// MidiMenuMusicSelector.cs
//
// Gerencia o dropdown de seleção de músicas MIDI no menu e controla a reprodução.
// ===============================

using UnityEngine;
using TMPro;
using MidiPlayerTK;
using System.Collections.Generic;

// Classe responsável por popular e controlar o dropdown de músicas MIDI
public class MidiMenuMusicSelector : MonoBehaviour
{
    // ===============================
    // Referências de UI e player
    // ===============================
    public TMP_Dropdown midiDropdown; // Dropdown de seleção de música
    public MidiFilePlayer midiFilePlayer; // Player MIDI para reprodução

    private List<MPTKListItem> midiItems; // Lista de arquivos MIDI disponíveis
    private List<string> midiNames;       // Lista de nomes para o dropdown

    // ===============================
    // Inicialização: popula o dropdown e seleciona música padrão
    // ===============================
    void Start()
    {
        // Pega a lista de arquivos MIDI do MidiPlayerTK
        midiItems = MidiPlayerGlobal.MPTK_ListMidi;
        midiNames = new List<string>();
        foreach (var item in midiItems)
            midiNames.Add(item.Label);

        midiDropdown.ClearOptions();
        midiDropdown.AddOptions(midiNames);

        midiDropdown.onValueChanged.AddListener(OnMidiSelected);

        // Seleciona e toca a música número 7 (índice 7) por padrão
        int defaultIndex = 7;
        if (midiItems.Count > defaultIndex)
        {
            midiDropdown.value = defaultIndex;
            PlayMidi(midiItems[defaultIndex].Label);
        }
        else if (midiItems.Count > 0)
        {
            midiDropdown.value = 0;
            PlayMidi(midiItems[0].Label);
        }
    }

    // ===============================
    // Callback ao selecionar uma música no dropdown
    // ===============================
    void OnMidiSelected(int index)
    {
        if (index >= 0 && index < midiItems.Count)
        {
            PlayMidi(midiItems[index].Label);
        }
    }

    // ===============================
    // Toca a música MIDI selecionada
    // ===============================
    void PlayMidi(string midiName)
    {
        if (midiFilePlayer != null)
        {
            midiFilePlayer.MPTK_Stop(); // Garante que a música anterior pare
            midiFilePlayer.MPTK_MidiName = midiName;
            midiFilePlayer.MPTK_Play();
        }
    }
}
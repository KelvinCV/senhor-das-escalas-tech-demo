// ===============================
// MidiSoundManager.cs
//
// Gerencia a reprodução de notas MIDI no jogo, troca de instrumentos,
// controle de canais e integração com o sistema ISoundManager.
// ===============================

using UnityEngine;
using MidiPlayerTK;
using System.Collections.Generic;
using System.Collections;

// Classe principal para gerenciamento de som MIDI
public class MidiSoundManager : MonoBehaviour, ISoundManager
{
    // ===============================
    // Referências e estado
    // ===============================
    public MidiStreamPlayer midiPlayer; // Player MIDI
    private Dictionary<int, MPTKEvent> notasAtivas = new Dictionary<int, MPTKEvent>(); // Notas ativas por canal
    private bool midiPronto = false; // Indica se o MIDI está pronto para tocar
    public bool IsMidiPronto() => midiPronto;

    // ===============================
    // Inicialização e ciclo de vida
    // ===============================
    void OnEnable()
    {
        ReiniciarMidiPlayer(); // Garante que o player esteja pronto ao ativar o objeto
    }

    void Start()
    {
        // Carrega o instrumento salvo ou usa o padrão (0 - Piano)
        int instrumentoSalvo = PlayerPrefs.GetInt("InstrumentoMIDI", 0);
        TrocarInstrumento(instrumentoSalvo);
    }

    // ===============================
    // Reinicia o player MIDI e espera ele ficar pronto
    // ===============================
    public void ReiniciarMidiPlayer()
    {
        if (midiPlayer != null)
        {
            Debug.Log("[MidiSoundManager] Reiniciando MidiStreamPlayer...");
            midiPlayer.MPTK_StartMidiStream(); // Reativa o MIDI
            StartCoroutine(EsperarMidiPronto()); // Espera o MIDI estar pronto
        }
    }

    private IEnumerator EsperarMidiPronto()
    {
        midiPronto = false;
        Debug.Log("[MidiSoundManager] Aguardando MIDI ficar pronto...");

        // Aguardando múltiplos frames como fallback (sem depender de MPTK_IsPlaying)
        for (int i = 0; i < 30; i++) // ~0.5 segundos
        {
            yield return new WaitForSeconds(0.02f);
        }

        midiPronto = true;
        Debug.Log("[MidiSoundManager] MIDI está presumivelmente pronto.");
    }

    // ===============================
    // Toca uma nota MIDI (com canal opcional)
    // ===============================
    public void TocarNota(int index, int channel = 0)
    {
        if (!midiPronto)
        {
            Debug.LogWarning($"[MidiSoundManager] Ignorando TocarNota({index}) - MIDI ainda não pronto.");
            return;
        }
        Debug.Log($"[MidiSoundManager] TocarNota chamada! index={index}, canal={channel}");

        int midiNote = ConverterNotaParaMidi(index);
        int key = midiNote + channel * 128;
        if (notasAtivas.ContainsKey(key)) return;

        MPTKEvent nota = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOn,
            Value = midiNote,
            Channel = channel,
            Velocity = 127,
            Delay = 0
        };

        notasAtivas[key] = nota;
        midiPlayer.MPTK_PlayEvent(nota);
    }

    // Implementação ISoundManager: canal padrão 0
    public void TocarNota(int index)
    {
        TocarNota(index, 0); // Canal padrão 0 para chamadas genéricas
    }

    // ===============================
    // Para uma nota MIDI (com canal opcional)
    // ===============================
    public void PararNota(int index, int channel = 0)
    {
        int midiNote = ConverterNotaParaMidi(index);
        int key = midiNote + channel * 128;
        if (notasAtivas.TryGetValue(key, out var notaParaParar))
        {
            midiPlayer.MPTK_StopEvent(notaParaParar);
            notasAtivas.Remove(key);
        }
    }

    // Implementação ISoundManager: canal padrão 0
    public void PararNota(int index)
    {
        PararNota(index, 0); // Canal padrão 0
    }

    // ===============================
    // Troca o instrumento MIDI do canal 0
    // ===============================
    public void TrocarInstrumento(int instrumentoIndex)
    {
        if (midiPlayer != null)
        {
            // Define o número do preset (instrumento) no canal 0
            midiPlayer.MPTK_Channels[0].PresetNum = instrumentoIndex;
            // Opcional: Defina o banco se necessário (por exemplo, banco 0 para instrumentos padrão)
            midiPlayer.MPTK_Channels[0].BankNum = 0;
            Debug.Log($"[MidiSoundManager] Instrumento alterado para índice {instrumentoIndex} no canal 0.");
        }
        else
        {
            Debug.LogError("[MidiSoundManager] MidiStreamPlayer não está configurado.");
        }
    }

    // ===============================
    // Converte índice de nota para valor MIDI
    // ===============================
    private int ConverterNotaParaMidi(int index)
    {
        string[] nomesNotas = new string[]
        {
            "C8vb", "C#8vb", "D8vb", "D#8vb", "E8vb", "F8vb", "F#8vb", "G8vb", "G#8vb", "A8vb", "A#8vb", "B8vb",
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B",
            "C8va"
        };

        if (index >= 0 && index < nomesNotas.Length)
        {
            string nota = nomesNotas[index];
            return ObterNotaMidi(nota);
        }
        return -1;
    }

    // ===============================
    // Mapeia nome de nota para valor MIDI
    // ===============================
    public static int ObterNotaMidi(string nomeNota)
    {
        Dictionary<string, int> mapa = new Dictionary<string, int>
        {
            { "C8vb", 48 }, { "C#8vb", 49 }, { "D8vb", 50 }, { "D#8vb", 51 },
            { "E8vb", 52 }, { "F8vb", 53 }, { "F#8vb", 54 }, { "G8vb", 55 },
            { "G#8vb", 56 }, { "A8vb", 57 }, { "A#8vb", 58 }, { "B8vb", 59 },
            { "C", 60 }, { "C#", 61 }, { "D", 62 }, { "D#", 63 },
            { "E", 64 }, { "F", 65 }, { "F#", 66 }, { "G", 67 },
            { "G#", 68 }, { "A", 69 }, { "A#", 70 }, { "B", 71 },
            { "C8va", 72 }
        };
        return mapa.TryGetValue(nomeNota, out int midi) ? midi : -1;
    }
}

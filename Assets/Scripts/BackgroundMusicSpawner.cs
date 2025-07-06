// ===============================
// BackgroundMusicSpawner.cs
//
// Responsável por agendar e tocar notas de acompanhamento de fundo
// usando MIDI, a partir de um arquivo JSON serializado.
// ===============================

using UnityEngine;
using MidiPlayerTK;
using System.Collections.Generic;
using System.Collections;

// ===============================
// Estruturas de dados para notas de fundo
// ===============================
[System.Serializable]
public class BackgroundNote
{
    public string noteName; // Nome da nota (ex: "C", "D#")
    public float time;      // Momento em segundos para tocar
    public float duration;  // Duração da nota em segundos
}

[System.Serializable]
public class BackgroundNoteList
{
    public List<BackgroundNote> notes; // Lista de notas
}

// ===============================
// Classe principal do spawner de música de fundo
// ===============================
public class BackgroundMusicSpawner : MonoBehaviour
{
    // ===============================
    // Configurações e referências
    // ===============================
    public TextAsset backgroundJsonFile; // Arquivo JSON com as notas
    public MidiStreamPlayer midiPlayer;  // Player MIDI
    public int midiChannel = 1;          // Canal MIDI configurável no Inspector

    private Dictionary<int, int> noteOnCounters = new Dictionary<int, int>(); // Controle de NoteOn/NoteOff
    private bool musicaTerminou = false; // Indica se a música terminou
    private bool isPaused = false;       // Indica se está pausado
    //private float pausedTime = 0f;     // (não utilizado)
    private List<Coroutine> noteCoroutines = new List<Coroutine>(); // Corrotinas ativas

    // ===============================
    // Inicialização
    // ===============================
    private void Start()
    {
        InicializarSpawner();
    }

    private void OnEnable()
    {
        if (gameObject.activeInHierarchy)
            InicializarSpawner();
    }

    // ===============================
    // Inicializa e agenda as notas
    // ===============================
    private void InicializarSpawner()
    {
        if (backgroundJsonFile == null || midiPlayer == null)
        {
            Debug.LogError("[BackgroundMusicSpawner] JSON ou MidiPlayer não atribuído.");
            enabled = false;
            return;
        }

        // Configura o canal e instrumento para acompanhamento
        int presetIndex = PlayerPrefs.GetInt("InstrumentoMIDI", 0);
        midiPlayer.MPTK_Channels[midiChannel].PresetNum = presetIndex;
        midiPlayer.MPTK_Channels[midiChannel].BankNum = 0;

        midiPlayer.MPTK_StartMidiStream();

        BackgroundNoteList noteList = JsonUtility.FromJson<BackgroundNoteList>(backgroundJsonFile.text);
        foreach (var note in noteList.notes)
        {
            int midiValue = MidiSoundManager.ObterNotaMidi(note.noteName);
            if (midiValue >= 0)
            {
                StartCoroutine(PlayNoteWithDelay(midiValue, note.time, note.duration));
            }
            else
            {
                Debug.LogWarning($"[BackgroundMusicSpawner] Nota inválida: {note.noteName}");
            }
        }

        Debug.Log($"[BackgroundMusicSpawner] Agendamento completo: {noteList.notes.Count} notas.");
        StartCoroutine(VerificarFimDaMusica(noteList));
    }

    // ===============================
    // Corrotina para tocar uma nota após um delay
    // ===============================
    private IEnumerator PlayNoteWithDelay(int midiValue, float delaySeconds, float durationSeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        if (isPaused)
        {
            // Se estiver pausado, espera despausar
            while (isPaused)
            {
                yield return null;
            }
        }

        // NoteOn
        if (!noteOnCounters.ContainsKey(midiValue))
            noteOnCounters[midiValue] = 0;
        noteOnCounters[midiValue]++;

        MPTKEvent noteOn = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOn,
            Value = midiValue,
            Channel = midiChannel,
            Velocity = 120
        };
        midiPlayer.MPTK_PlayEvent(noteOn);

        yield return new WaitForSeconds(durationSeconds);

        if (isPaused)
        {
            // Se estiver pausado, espera despausar
            while (isPaused)
            {
                yield return null;
            }
        }

        // NoteOff só se for o último NoteOn pendente
        noteOnCounters[midiValue]--;
        if (noteOnCounters[midiValue] <= 0)
        {
            noteOnCounters[midiValue] = 0;
            MPTKEvent noteOff = new MPTKEvent()
            {
                Command = MPTKCommand.NoteOff,
                Value = midiValue,
                Channel = midiChannel
            };
            midiPlayer.MPTK_PlayEvent(noteOff);
        }
    }

    // ===============================
    // Corrotina para detectar o fim da música
    // ===============================
    private IEnumerator VerificarFimDaMusica(BackgroundNoteList noteList)
    {
        if (noteList.notes.Count == 0)
        {
            musicaTerminou = true;
            yield break;
        }
        float ultimaNotaFim = 0f;
        foreach (var note in noteList.notes)
        {
            float fim = note.time + note.duration;
            if (fim > ultimaNotaFim) ultimaNotaFim = fim;
        }
        yield return new WaitForSeconds(ultimaNotaFim + 0.5f); // margem de segurança
        musicaTerminou = true;
    }

    // ===============================
    // Consulta se a música terminou
    // ===============================
    public bool MusicaTerminou()
    {
        return musicaTerminou;
    }

    // ===============================
    // Pausa e retoma a execução das notas
    // ===============================
    public void Pause()
    {
        isPaused = true;
        // Pausar corrotinas não é trivial, mas como Time.timeScale = 0, WaitForSeconds "congela"
    }

    public void ResumeFromPause()
    {
        isPaused = false;
        // Nada a fazer, corrotinas continuam com Time.timeScale = 1
    }

    // ===============================
    // Reseta o spawner para novo uso
    // ===============================
    public void ResetSpawner()
    {
        musicaTerminou = false;
        noteOnCounters.Clear();
        StopAllCoroutines();
        isPaused = false;
        //pausedTime = 0f;
        // Ao reativar, Start() será chamado novamente
    }
}

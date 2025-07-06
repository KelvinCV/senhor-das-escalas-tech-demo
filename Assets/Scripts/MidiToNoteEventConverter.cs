// ===============================
// MidiToNoteEventConverter.cs
//
// Converte arquivos MIDI em listas de eventos de nota (NoteEvent) e exporta para JSON.
// Útil para criar mapas de notas para jogos rítmicos a partir de arquivos MIDI.
// ===============================

using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System.IO;

// Classe principal para conversão de MIDI em eventos de nota
public class MidiToNoteEventConverter : MonoBehaviour
{
    // ===============================
    // Configurações
    // ===============================
    public string midiFileName = "nome_do_arquivo.mid"; // Nome do arquivo MIDI na pasta Resources/MidiDB
    public int channelToExtract = 0; // Canal MIDI a ser extraído (0 = todos)
    public string outputJsonPath = "Assets/MusicData/JSON/saida.json"; // Caminho de saída do JSON

    // ===============================
    // Método principal de conversão (chamável pelo Inspector)
    // ===============================
    [ContextMenu("Converter MIDI para NoteEvent JSON")]
    public void ConvertMidiToNoteEvents()
    {
        MidiFilePlayer midiFilePlayer = gameObject.AddComponent<MidiFilePlayer>();
        midiFilePlayer.MPTK_MidiName = midiFileName;
        midiFilePlayer.MPTK_Load();

        if (midiFilePlayer.MPTK_MidiEvents == null)
        {
            Debug.LogError("Falha ao carregar eventos MIDI. Verifique se o arquivo existe e é válido.");
            DestroyImmediate(midiFilePlayer);
            return;
        }

        List<NoteEvent> noteEvents = new List<NoteEvent>();

        // Percorre todos os eventos MIDI
        foreach (MPTKEvent midiEvent in midiFilePlayer.MPTK_MidiEvents)
        {
            // Filtra apenas NoteOn válidos (velocity > 0)
            if (midiEvent.Command == MPTKCommand.NoteOn && midiEvent.Velocity > 0)
            {
                // Filtra canal, se necessário
                if (channelToExtract == 0 || midiEvent.Channel == channelToExtract - 1)
                {
                    // Encontra o NoteOff correspondente para calcular a duração
                    float duration = 0.5f; // Valor padrão
                    foreach (MPTKEvent offEvent in midiFilePlayer.MPTK_MidiEvents)
                    {
                        bool isNoteOff = (offEvent.Command == MPTKCommand.NoteOff) ||
                                         (offEvent.Command == MPTKCommand.NoteOn && offEvent.Velocity == 0);
                        if (isNoteOff &&
                            offEvent.Value == midiEvent.Value &&
                            offEvent.Channel == midiEvent.Channel &&
                            offEvent.Tick > midiEvent.Tick)
                        {
                            duration = (float)(offEvent.RealTime - midiEvent.RealTime) / 1000f; // Corrigido para segundos
                            break;
                        }
                    }

                    noteEvents.Add(new NoteEvent
                    {
                        noteName = MidiNoteToName(midiEvent.Value),
                        time = (float)midiEvent.RealTime / 1000f,
                        duration = duration,
                        hasSpawned = false
                    });
                }
            }
        }

        // Salva como JSON
        NoteDataWrapper wrapper = new NoteDataWrapper { notes = noteEvents };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(outputJsonPath, json);

        Debug.Log($"Conversão concluída! {noteEvents.Count} notas exportadas para {outputJsonPath}");
        DestroyImmediate(midiFilePlayer);
    }

    // ===============================
    // Converte número MIDI para nome (ex: 60 -> "C", 61 -> "C#", etc.)
    // ===============================
    private string MidiNoteToName(int midiValue)
    {
        string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int octave = (midiValue / 12) - 1;
        string note = names[midiValue % 12];
        if (octave == 5) return note + "8va";
        if (octave == 3) return note + "8vb";
        return note;
    }

    // ===============================
    // Classes auxiliares para serialização
    // ===============================
    [System.Serializable]
    public class NoteDataWrapper
    {
        public List<NoteEvent> notes;
    }

    [System.Serializable]
    public class NoteEvent
    {
        public string noteName;
        public float time;
        public float duration;
        [System.NonSerialized] public bool hasSpawned;
    }
}
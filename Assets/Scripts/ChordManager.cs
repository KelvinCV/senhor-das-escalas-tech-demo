// ===============================
// ChordManager.cs
//
// Gerencia a detecção de acordes e exibição de notas pressionadas na interface.
// Detecta intervalos, identifica padrões de acordes e atualiza o texto na UI.
// ===============================

using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Classe principal do gerenciador de intervalos musicais e acordes
public class ChordManager : MonoBehaviour
{
    // ===============================
    // Referências e estado
    // ===============================
    public TextMeshProUGUI chordText; // Texto para exibir o acorde ou nota
    private HashSet<string> pressedNotes; // Notas atualmente pressionadas
    private Dictionary<string, int> noteToValue; // Mapeia notas para valores numéricos

    // ===============================
    // Inicialização
    // ===============================
    private void Start()
    {
        if (chordText == null)
        {
            Debug.LogError("ChordManager: O campo 'chordText' não foi atribuído no Inspector.");
        }

        // Mapeamento de nomes de notas para valores (inclui oitavas)
        noteToValue = new Dictionary<string, int>
        {
            // Oitava abaixo (8vb)
            { "C8vb", -12 }, { "C#8vb", -11 }, { "D8vb", -10 }, { "D#8vb", -9 },
            { "E8vb", -8 }, { "F8vb", -7 }, { "F#8vb", -6 }, { "G8vb", -5 },
            { "G#8vb", -4 }, { "A8vb", -3 }, { "A#8vb", -2 }, { "B8vb", -1 },

            // Oitava padrão
            { "C", 0 }, { "C#", 1 }, { "D", 2 }, { "D#", 3 },
            { "E", 4 }, { "F", 5 }, { "F#", 6 }, { "G", 7 },
            { "G#", 8 }, { "A", 9 }, { "A#", 10 }, { "B", 11 },

            // Oitava acima (8va)
            { "C8va", 12 }, { "C#8va", 13 }, { "D8va", 14 }, { "D#8va", 15 },
            { "E8va", 16 }, { "F8va", 17 }, { "F#8va", 18 }, { "G8va", 19 },
            { "G#8va", 20 }, { "A8va", 21 }, { "A#8va", 22 }, { "B8va", 23 }
        };

        pressedNotes = new HashSet<string>();
        UpdateChord();
    }

    // ===============================
    // Métodos públicos para pressionar/soltar notas
    // ===============================
    public void NotePressed(string note)
    {
        Debug.Log($"ChordManager: Nota pressionada: {note}");
        pressedNotes.Add(note);
        UpdateChord();
    }

    public void NoteReleased(string note)
    {
        Debug.Log($"ChordManager: Nota solta: {note}");
        pressedNotes.Remove(note);
        UpdateChord();
    }

    // ===============================
    // Timer para exibir "..." quando nenhuma nota está pressionada
    // ===============================
    private float noNoteTimer = 0f;
    private bool noNotesDisplayed = false;

    private void Update()
    {
        if (pressedNotes.Count == 0)
        {
            noNoteTimer += Time.deltaTime;
            if (noNoteTimer >= 0.5f && !noNotesDisplayed)
            {
                chordText.text = "...";
                Debug.Log("ChordManager: Nenhuma nota pressionada por 0.5 segundos.");
                noNotesDisplayed = true;
            }
        }
        else
        {
            noNoteTimer = 0f;
            noNotesDisplayed = false;
        }
    }

    // ===============================
    // Atualiza o texto do acorde/notas na UI
    // ===============================
    private void UpdateChord()
    {
        if (pressedNotes.Count == 0)
        {
            // Não mostra "..." imediatamente, deixa Update() cuidar disso
            Debug.Log("ChordManager: Nenhuma nota pressionada.");
            return;
        }

        // Monta lista de notas pressionadas e seus valores
        List<KeyValuePair<string, int>> notePairs = new List<KeyValuePair<string, int>>();
        foreach (string note in pressedNotes)
        {
            if (noteToValue.TryGetValue(note, out int value))
            {
                notePairs.Add(new KeyValuePair<string, int>(note, value));
            }
            else
            {
                Debug.LogWarning($"ChordManager: Nota desconhecida '{note}'.");
            }
        }

        // Ordena pela altura
        notePairs.Sort((a, b) => a.Value.CompareTo(b.Value));

        List<int> noteValues = new List<int>();
        foreach (var pair in notePairs)
        {
            noteValues.Add(pair.Value);
        }

        // Calcula os intervalos com base na nota mais grave
        int baseValue = noteValues[0];
        List<string> intervalNames = new List<string>();
        for (int i = 1; i < noteValues.Count; i++)
        {
            int semitons = noteValues[i] - baseValue;
            string name = GetIntervalName(semitons);
            intervalNames.Add(name);
        }

        Debug.Log($"ChordManager: Intervalos com altura: {string.Join(", ", intervalNames)}");

        string result = DetectChord(noteValues, intervalNames);
        chordText.text = result;
        Debug.Log($"ChordManager: Resultado detectado: {result}");
    }

    // ===============================
    // Retorna o nome do intervalo dado o número de semitons
    // ===============================
    private string GetIntervalName(int semitons)
    {
        int simple = semitons % 12;
        string label = simple switch
        {
            0 => "Uníssono",
            1 => "Segunda menor",
            2 => "Segunda maior",
            3 => "Terça menor",
            4 => "Terça maior",
            5 => "Quarta justa",
            6 => "Quarta aumentada / Quinta diminuta",
            7 => "Quinta justa",
            8 => "Sexta menor",
            9 => "Sexta maior",
            10 => "Sétima menor",
            11 => "Sétima maior",
            _ => "Intervalo desconhecido"
        };

        if (semitons >= 12)
        {
            int octaves = semitons / 12;
            return $"{label} (+{octaves} oitava{(octaves > 1 ? "s" : "")})";
        }

        return label;
    }

    // ===============================
    // Detecta o acorde com base nos intervalos
    // ===============================
    private string DetectChord(List<int> noteValues, List<string> intervalNames)
    {
        if (noteValues.Count == 1)
        {
            return $"Nota: {GetNoteName(noteValues[0])}";
        }

        // Dicionário de padrões de acordes
        Dictionary<string, List<string>> chordPatterns = new Dictionary<string, List<string>>
        {
            { "Maior com Sétima Maior", new List<string> { "Terça maior", "Quinta justa", "Sétima maior" } },
            { "Menor com Sétima Menor", new List<string> { "Terça menor", "Quinta justa", "Sétima menor" } },
            { "Dominante", new List<string> { "Terça maior", "Quinta justa", "Sétima menor" } },
            { "Maior", new List<string> { "Terça maior", "Quinta justa" } },
            { "Menor", new List<string> { "Terça menor", "Quinta justa" } },
            { "Diminuto", new List<string> { "Terça menor", "Quarta aumentada / Quinta diminuta" } },
            { "Aumentado", new List<string> { "Terça maior", "Quarta aumentada / Quinta diminuta" } },
            { "Sus2", new List<string> { "Segunda maior", "Quinta justa" } },
            { "Sus4", new List<string> { "Quarta justa", "Quinta justa" } },
            { "Meio Diminuto", new List<string> { "Terça menor", "Quarta aumentada / Quinta diminuta", "Sétima menor" } },
            { "Diminuto com Sétima Diminuta", new List<string> { "Terça menor", "Quarta aumentada / Quinta diminuta", "Sétima diminuta" } }
        };

        // Remove variações com oitava para comparar apenas os nomes básicos
        List<string> basicIntervals = new List<string>();
        foreach (string interval in intervalNames)
        {
            string baseName = interval.Split(" (+")[0]; // Remove "+1 oitava", etc.
            basicIntervals.Add(baseName);
        }

        Debug.Log($"ChordManager: Intervalos básicos detectados: {string.Join(", ", basicIntervals)}");

        // Prioriza padrões mais complexos (com mais intervalos)
        foreach (var chord in chordPatterns)
        {
            if (IntervalsMatch(basicIntervals, chord.Value))
            {
                string tonic = GetNoteName(noteValues[0]);
                Debug.Log($"ChordManager: Acorde detectado: {tonic} {chord.Key}");
                return $"{tonic} {chord.Key}";
            }
        }

        Debug.Log("ChordManager: Nenhum acorde correspondente encontrado.");
        return $"Intervalos: {string.Join(", ", intervalNames)}";
    }

    // ===============================
    // Verifica se os intervalos detectados batem com o padrão
    // ===============================
    private bool IntervalsMatch(List<string> intervals, List<string> pattern)
    {
        foreach (string interval in pattern)
        {
            if (!intervals.Contains(interval))
            {
                return false;
            }
        }
        return true;
    }

    // ===============================
    // Retorna o nome da nota dado o valor
    // ===============================
    private string GetNoteName(int value)
    {
        foreach (var pair in noteToValue)
        {
            if (pair.Value == value) // Verifica o valor exato da nota
            {
                return pair.Key; // Retorna o nome completo da nota (ex.: "C", "C8va", "C8vb")
            }
        }

        Debug.LogWarning($"ChordManager: Nota com valor {value} não encontrada no dicionário.");
        return "Desconhecido";
    }
}

// ===============================
// NoteSpawner.cs
//
// Responsável por spawnar notas na cena a partir de um arquivo JSON,
// controlar o andamento da música, detectar fim, pausar, resetar e acionar feedbacks.
// ===============================

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using MidiPlayerTK;
using UnityEngine.SceneManagement;

// Classe principal para spawn de notas e controle de música
public class NoteSpawner : MonoBehaviour
{
    // ===============================
    // Configurações e referências
    // ===============================
    [Header("Configurações de Arquivo")]
    [SerializeField] private TextAsset jsonFile;

    [Header("Configurações de Notas")]
    public Dictionary<string, GameObject> notePrefabs;
    public Transform[] spawnPoints;

    [HideInInspector] public List<NoteEvent> noteEvents;

    private float secondsPerBeat;
    private float songStartTime;

    public bool gameOver = false;
    private bool applausePlayed = false;

    private float pausedSongPosition = 0f;
    private bool isPaused = false;

    public ApplauseBarManager applauseBarManager; // Arraste no Inspector

    private bool gameOverTriggered = false;

    // ===============================
    // Inicialização
    // ===============================
    void Start()
    {
        // Inicializa o dicionário de prefabs de notas
        notePrefabs = new Dictionary<string, GameObject>
        {
            { "C", Resources.Load<GameObject>("Prefabs/Note_C") },
            { "C#", Resources.Load<GameObject>("Prefabs/Note_C#") },
            { "D", Resources.Load<GameObject>("Prefabs/Note_D") },
            { "D#", Resources.Load<GameObject>("Prefabs/Note_D#") },
            { "E", Resources.Load<GameObject>("Prefabs/Note_E") },
            { "F", Resources.Load<GameObject>("Prefabs/Note_F") },
            { "F#", Resources.Load<GameObject>("Prefabs/Note_F#") },
            { "G", Resources.Load<GameObject>("Prefabs/Note_G") },
            { "G#", Resources.Load<GameObject>("Prefabs/Note_G#") },
            { "A", Resources.Load<GameObject>("Prefabs/Note_A") },
            { "A#", Resources.Load<GameObject>("Prefabs/Note_A#") },
            { "B", Resources.Load<GameObject>("Prefabs/Note_B") },
            { "C8va", Resources.Load<GameObject>("Prefabs/Note_C_8va") },
            { "C8vb", Resources.Load<GameObject>("Prefabs/Note_C_8vb") },
            { "C#8vb", Resources.Load<GameObject>("Prefabs/Note_C#_8vb") },
            { "D8vb", Resources.Load<GameObject>("Prefabs/Note_D_8vb") },
            { "D#8vb", Resources.Load<GameObject>("Prefabs/Note_D#_8vb") },
            { "E8vb", Resources.Load<GameObject>("Prefabs/Note_E_8vb") },
            { "F8vb", Resources.Load<GameObject>("Prefabs/Note_F_8vb") },
            { "F#8vb", Resources.Load<GameObject>("Prefabs/Note_F#_8vb") },
            { "G8vb", Resources.Load<GameObject>("Prefabs/Note_G_8vb") },
            { "G#8vb", Resources.Load<GameObject>("Prefabs/Note_G#_8vb") },
            { "A8vb", Resources.Load<GameObject>("Prefabs/Note_A_8vb") },
            { "A#8vb", Resources.Load<GameObject>("Prefabs/Note_A#_8vb") },
            { "B8vb", Resources.Load<GameObject>("Prefabs/Note_B_8vb") }
        };

        songStartTime = Time.time;

        // Carrega notas do JSON
        LoadNotesFromJSON();
        
        // Define o tempo total de notas únicas
        var temposUnicos = noteEvents
            .Where(n => !string.IsNullOrEmpty(n.noteName))
            .Select(n => n.time)
            .Distinct()
            .Count();

        Object.FindFirstObjectByType<ScoreManager>()?.SetTotalNotes(temposUnicos);
    }

    // ===============================
    // Atualização por frame: spawn de notas, aplauso e game over
    // ===============================
    void Update()
    {
        if (gameOver || isPaused) return;

        float songPosition = Time.time - songStartTime;

        foreach (var noteEvent in noteEvents)
        {
            if (!noteEvent.hasSpawned && songPosition >= noteEvent.time)
            {
                SpawnNote(noteEvent);
                noteEvent.hasSpawned = true;
            }
        }

        // Corrigido: só toca o aplauso se não for GameOver
        if (!applausePlayed 
            && !gameOver
            && noteEvents.All(n => n.hasSpawned) 
            && Object.FindObjectsByType<FallingNote>(FindObjectsSortMode.None).Length == 0)
        {
            applausePlayed = true;
            var midiPlayer = Object.FindFirstObjectByType<MidiStreamPlayer>();
            if (midiPlayer != null)
                StartCoroutine(TocarAplausoMIDI(midiPlayer));
        }

        if (!gameOverTriggered && MusicaTerminou())
        {
            gameOverTriggered = true;
            SetGameOver();
        }
    }

    // ===============================
    // Spawna uma nota na posição correta
    // ===============================
    private void SpawnNote(NoteEvent noteEvent)
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.name == noteEvent.noteName)
            {
                if (notePrefabs.TryGetValue(noteEvent.noteName, out GameObject prefab))
                {
                    GameObject note = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
                    FallingNote fallingNote = note.GetComponent<FallingNote>();
                    fallingNote.noteName = noteEvent.noteName;
                    fallingNote.duration = noteEvent.duration; // Define a duração da nota
                    fallingNote.time = noteEvent.time;

                    // Ajusta apenas o eixo Y da escala com base no duration
                    float visualScaleFactor = 8f; // Fator de escala visual
                    Vector3 originalScale = note.transform.localScale;
                    note.transform.localScale = new Vector3(
                        originalScale.x, // Mantém o eixo X inalterado
                        noteEvent.duration * visualScaleFactor, // Ajusta o eixo Y
                        originalScale.z  // Mantém o eixo Z inalterado
                    );

                    // Calcula a velocidade de queda com base no tempo para alcançar o ponto de impacto
                    float currentSongTime = Time.time - songStartTime;
                    float timeToReach = noteEvent.time - currentSongTime;
                    if (timeToReach > 0)
                    {
                        fallingNote.fallSpeed = spawnPoints[0].position.y / timeToReach;
                    }

                    Debug.Log($"Nota {noteEvent.noteName} spawnada com duração {noteEvent.duration}!");
                }
                else
                {
                    Debug.LogWarning($"Prefab para a nota {noteEvent.noteName} não encontrado!");
                }
                break;
            }
        }
    }

    // ===============================
    // Carrega notas do arquivo JSON
    // ===============================
    private void LoadNotesFromJSON()
    {
        if (jsonFile == null)
        {
            Debug.LogError("NoteSpawner: Nenhum arquivo JSON atribuído.");
            return;
        }

        noteEvents = new List<NoteEvent>();
        NoteDataWrapper data = JsonUtility.FromJson<NoteDataWrapper>(jsonFile.text);

        foreach (var item in data.notes)
        {
            noteEvents.Add(new NoteEvent
            {
                noteName = item.noteName,
                time = item.time,
                duration = item.duration,
                hasSpawned = false
            });
        }

        // Filtra notas válidas (com nome de nota não vazio)
        List<NoteEvent> notasValidas = noteEvents.Where(n => !string.IsNullOrEmpty(n.noteName)).ToList();

        Debug.Log($"NoteSpawner: {notasValidas.Count} notas válidas carregadas do JSON.");
    }

    // ===============================
    // Corrotina para tocar e parar o aplauso MIDI
    // ===============================
    private System.Collections.IEnumerator TocarAplausoMIDI(MidiStreamPlayer midiPlayer)
    {
        int applausePatch = 126; // GM Applause
        int applauseNote = 39;   // Hand Clap (pode variar, mas 39 é padrão)
        int channel = 0;         // Canal MIDI 

        midiPlayer.MPTK_Channels[channel].PresetNum = applausePatch;
        
        // Toca o aplauso
        MPTKEvent applauseEvent = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOn,
            Value = applauseNote,
            Channel = channel,
            Velocity = 127
        };
        midiPlayer.MPTK_PlayEvent(applauseEvent);

        // Espera 3 segundos
        yield return new WaitForSeconds(2f);

        // Para o aplauso (NoteOff)
        MPTKEvent applauseOff = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOff,
            Value = applauseNote,
            Channel = channel,
            Velocity = 0
        };
        midiPlayer.MPTK_PlayEvent(applauseOff);
    }

    // ===============================
    // Wrapper para serialização de notas
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

    // ===============================
    // Lógica de Game Over e precisão
    // ===============================
    public void SetGameOver()
    {
        gameOver = true;

        var scoreManager = FindFirstObjectByType<ScoreManager>();
        float acuracia = scoreManager != null ? scoreManager.GetAccuracyPercent() : 0f;
        var gameOverManager = FindFirstObjectByType<GameOverManager>();

        if (acuracia >= 70f)
        {
            // Toca o aplauso só no sucesso
            if (!applausePlayed)
            {
                applausePlayed = true;
                var midiPlayer = Object.FindFirstObjectByType<MidiStreamPlayer>();
                if (midiPlayer != null)
                    StartCoroutine(TocarAplausoMIDI(midiPlayer));
            }
            gameOverManager?.ShowGameOverSuccess();
        }
        else
        {
            gameOverManager?.ShowGameOver();
        }
    }

    // ===============================
    // Detecta se a música terminou
    // ===============================
    public bool MusicaTerminou()
    {
        // Todas as notas spawnadas E não há mais notas caindo na cena
        bool todasNotasSpawnadas = noteEvents != null && noteEvents.All(n => n.hasSpawned);
        bool nenhumaNotaNaTela = GameObject.FindObjectsByType<FallingNote>(FindObjectsSortMode.None).Length == 0;
        return todasNotasSpawnadas && nenhumaNotaNaTela;
    }

    // ===============================
    // Pausa e retoma o andamento da música
    // ===============================
    public void Pause()
    {
        isPaused = true;
        pausedSongPosition = Time.time - songStartTime;
    }

    public void ResumeFromPause()
    {
        isPaused = false;
        songStartTime = Time.time - pausedSongPosition;
    }

    // ===============================
    // Reseta o spawner para novo uso
    // ===============================
    public void ResetSpawner()
    {
        gameOver = false;
        applausePlayed = false;
        isPaused = false;
        pausedSongPosition = 0f;
        LoadNotesFromJSON();
        songStartTime = Time.time;

        // Atualiza o ScoreManager
        var scoreManager = Object.FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null && noteEvents != null)
        {
            scoreManager.ResetScore();
            var temposUnicos = noteEvents
                .Where(n => !string.IsNullOrEmpty(n.noteName))
                .Select(n => n.time)
                .Distinct()
                .Count();
            scoreManager.SetTotalNotes(temposUnicos);
        }

        // Reseta a barra de aplauso ao valor padrão e visual
        if (applauseBarManager != null)
            applauseBarManager.ResetBar();
    }
}

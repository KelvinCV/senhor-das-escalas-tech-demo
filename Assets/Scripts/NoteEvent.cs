// ===============================
// NoteEvent.cs
//
// Representa um evento de nota para spawn/control em jogos rítmicos.
// Usado para armazenar informações de notas extraídas de arquivos MIDI ou mapas de música.
// ===============================

using System;

[Serializable]
public class NoteEvent
{
    public string noteName;   // Nome da nota (ex.: "C", "D#", "F 8va")
    public float time;        // Tempo em segundos para spawnar a nota
    public float duration;    // Duração da nota (se necessário)
    public bool hasSpawned;   // Indica se a nota já foi spawnada
}
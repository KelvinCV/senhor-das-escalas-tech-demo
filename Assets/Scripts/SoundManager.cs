/*
 * SoundManager.cs
 *
 * Gerencia a reprodução de sons de notas musicais, incluindo fade in/out, controle de fontes de áudio e execução de sequências de notas.
 * Implementa a interface ISoundManager para integração com o sistema do jogo.
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour, ISoundManager
{
    // Array de clipes de áudio das notas
    public AudioClip[] notas;
    // Duração do fade in/out em segundos
    public float fadeDuration = 0.1f;

    // Lista de eventos de notas para reprodução sequencial
    private List<NoteEvent> noteEvents;
    // Tempo de início da música
    private float songStartTime;
    // Lista para rastrear fontes de áudio criadas
    private List<AudioSource> fontesAtivas = new List<AudioSource>();

    // Toca uma nota pelo índice, criando um AudioSource temporário com fade in/out
    public void TocarNota(int index)
    {
        if (index < 0 || index >= notas.Length)
        {
            Debug.LogError($"SoundManager: Índice da nota ({index}) está fora do intervalo do array 'notas'.");
            return;
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = notas[index];
        newSource.volume = 0;
        fontesAtivas.Add(newSource);
        StartCoroutine(FadeInAndPlay(newSource));
    }

    // Não implementado: parada individual de nota não é necessária com fade automático
    public void PararNota(int index)
    {
        // Não necessário com fade automático + limpeza
    }

    // Coroutine para fade in, reprodução e fade out do áudio
    private IEnumerator FadeInAndPlay(AudioSource audioSource)
    {
        audioSource.Play();
        float startVolume = 0f;
        float targetVolume = 1f;
        float elapsedTime = 0f;

        // Fade in
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;

        // Aguarda o término do áudio
        yield return new WaitForSeconds(audioSource.clip.length);

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(targetVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        // Remove o AudioSource e limpa da lista
        fontesAtivas.Remove(audioSource);
        Destroy(audioSource);
    }

    // Inicia a reprodução de uma sequência de notas a partir de uma lista de eventos
    public void PlayNotesFromCSV(List<NoteEvent> loadedNotes)
    {
        noteEvents = loadedNotes;
        songStartTime = Time.time;
        StartCoroutine(PlayNotesCoroutine());
    }

    // Coroutine para tocar eventos de nota em sequência (placeholder)
    private IEnumerator PlayNotesCoroutine()
    {
        foreach (var noteEvent in noteEvents)
        {
            float timeToWait = noteEvent.time - (Time.time - songStartTime);
            if (timeToWait > 0)
                yield return new WaitForSeconds(timeToWait);
            // Aqui pode-se tocar a nota desejada
        }
    }

    // Para todos os sons em execução e destrói os AudioSources criados
    public void PararTodosOsSons()
    {
        foreach (var source in fontesAtivas)
        {
            if (source != null)
            {
                source.Stop();
                Destroy(source);
            }
        }
        fontesAtivas.Clear();
    }
}

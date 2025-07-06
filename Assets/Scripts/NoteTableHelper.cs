/*
 * NoteTableHelper.cs
 *
 * Utilitário estático para fornecer informações sobre notas musicais e gerar uma tabela formatada de notas, legendas e atalhos de teclado.
 * Usado para exibir ao usuário a relação entre notas, suas descrições e os atalhos configurados no jogo musical.
 *
 */

using System.Collections.Generic;
using UnityEngine;

public static class NoteTableHelper
{
    #region Tabela de Notas
    // Array de tuplas contendo o nome da nota e sua legenda/descritivo.
    public static (string nome, string legenda)[] notas = new (string, string)[]
    {
        ("C8vb", "Dó uma oitava abaixo"),
        ("C#8vb", "Dó sustenido ou Ré bemol uma oitava abaixo"),
        ("D8vb", "Ré uma oitava abaixo"),
        ("D#8vb", "Ré sustenido ou Mi bemol uma oitava abaixo"),
        ("E8vb", "Mi uma oitava abaixo"),
        ("F8vb", "Fá uma oitava abaixo"),
        ("F#8vb", "Fá sustenido ou Sol bemol uma oitava abaixo"),
        ("G8vb", "Sol uma oitava abaixo"),
        ("G#8vb", "Sol sustenido ou Lá bemol uma oitava abaixo"),
        ("A8vb", "Lá uma oitava abaixo"),
        ("A#8vb", "Lá sustenido ou Si bemol uma oitava abaixo"),
        ("B8vb", "Si uma oitava abaixo"),
        ("C", "<b>Dó central</b>"),
        ("C#", "Dó sustenido ou Ré bemol"),
        ("D", "Ré"),
        ("D#", "Ré sustenido ou Mi bemol"),
        ("E", "Mi"),
        ("F", "Fá"),
        ("F#", "Fá sustenido ou Sol bemol"),
        ("G", "Sol"),
        ("G#", "Sol sustenido ou Lá bemol"),
        ("A", "Lá"),
        ("A#", "Lá sustenido ou Si bemol"),
        ("B", "Si"),
        ("C8va", "<b>Dó uma oitava acima</b>")
    };
    #endregion

    #region Geração da Tabela de Notas
    /// <summary>
    /// Gera uma tabela formatada em texto, listando todas as notas, suas legendas e os atalhos de teclado associados.
    /// </summary>
    /// <param name="keyBindings">Dicionário de atalhos de teclado para cada nota (nome da nota como chave).</param>
    /// <returns>String formatada pronta para exibição (ex: em UI com Rich Text).</returns>
    public static string GerarTabelaNotasComAtalhos(Dictionary<string, KeyCode> keyBindings)
    {
        // Cabeçalho da tabela com formatação Rich Text.
        string tabela = "<align=justified><size=120%><b><color=#FFD700>Nota</color>         <color=#FFFFFF>Legenda</color>                <color=#00FF00>Atalho</color></b></size>\n";
        // Para cada nota, adiciona uma linha com nome, legenda e atalho (ou '-' se não houver atalho).
        foreach (var (nome, legenda) in notas)
        {
            string atalho = keyBindings != null && keyBindings.ContainsKey(nome) ? keyBindings[nome].ToString() : "-";
            tabela += $"<color=#FFD700>{nome,-7}</color>  <color=#FFFFFF>{legenda,-30}</color>  <color=#00FF00>{atalho}</color>\n";
        }
        return tabela;
    }
    #endregion
}
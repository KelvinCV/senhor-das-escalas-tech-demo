/*
 * NoteTablePanelToggle.cs
 *
 * Exibe e oculta o painel da tabela de notas musicais, atualizando o texto com os atalhos atuais.
 * Permite alternância via tecla Tab ou método público (ex: botão UI).
 *
 */

using UnityEngine;
using TMPro;

public class NoteTablePanelToggle : MonoBehaviour
{
    // Painel da tabela de notas
    public GameObject painelTabelaNotas;
    // Texto que exibe a tabela de notas e atalhos
    public TextMeshProUGUI textoTabelaNotas;

    // Garante que o painel começa oculto
    void Start()
    {
        if (painelTabelaNotas != null)
            painelTabelaNotas.SetActive(false);
    }

    // Alterna o painel ao pressionar Tab e atualiza o texto com os atalhos atuais
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && painelTabelaNotas != null)
        {
            bool novoEstado = !painelTabelaNotas.activeSelf;
            painelTabelaNotas.SetActive(novoEstado);
            if (novoEstado && textoTabelaNotas != null)
            {
                if (GameSettings.keyBindings.Count == 0)
                    GameSettings.LoadBindings();
                textoTabelaNotas.text = NoteTableHelper.GerarTabelaNotasComAtalhos(GameSettings.keyBindings);
            }
        }
    }

    // Alterna o painel via chamada de método (ex: botão UI) e atualiza o texto
    public void ToggleTabelaNotas()
    {
        if (painelTabelaNotas != null)
        {
            bool novoEstado = !painelTabelaNotas.activeSelf;
            painelTabelaNotas.SetActive(novoEstado);
            if (novoEstado && textoTabelaNotas != null)
            {
                if (GameSettings.keyBindings.Count == 0)
                    GameSettings.LoadBindings();
                textoTabelaNotas.text = NoteTableHelper.GerarTabelaNotasComAtalhos(GameSettings.keyBindings);
            }
        }
    }
}
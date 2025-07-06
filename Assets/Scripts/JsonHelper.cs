// ===============================
// JsonHelper.cs
//
// Utilitário para serialização de arrays em JSON usando UnityEngine.JsonUtility.
// Envolve arrays em um wrapper para compatibilidade com o formato do Unity.
// ===============================

using System;
using UnityEngine;

public static class JsonHelper
{
    // Serializa um array para JSON, usando um wrapper para compatibilidade
    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T> { notes = array };
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    // Wrapper serializável para arrays
    [Serializable]
    private class Wrapper<T>
    {
        public T[] notes;
    }
}

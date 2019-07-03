using System;
using UnityEngine;
using System.Collections;

/* 
 * Inserir aqui métodos úteis para depuração do código que não seriam utilizados em
 * situações rotineiras de desenvolvimento. O importante nesse caso é estabelecer uma
 * visão clara do que esta acontecendo em determinadas situações
 * Aqui, também devem ser implementados os métodos de inserção de LOG em arquivo
 * para sabermos os erros que ocorreram em fases de teste com o usuário!!!
 */ 

public static class DebugManager
{
    public static bool DEBUG_MODE = false;
    public static bool IA_DEBUG_MODE = true;
    public static bool NW_DEBUG_MODE = false;

    public static void PrintInfo(string info) {
        if(DEBUG_MODE) Debug.Log(info);
    }
    public static void PrintError(string info) {
        if(DEBUG_MODE) Debug.LogError(info);
    }
    public static void PrintException(Exception info)
    {
        if (DEBUG_MODE) Debug.LogException(info);
    }
    
    public static void IA_PrintInfo(string info) {
        if (IA_DEBUG_MODE) Debug.Log("(IA) - " + info);
    }
    public static void IA_PrintError(string info) {
        if (IA_DEBUG_MODE) Debug.LogError("(IA) - " + info);
    }
    public static void IA_PrintException(Exception info) {
        if (IA_DEBUG_MODE) Debug.LogException(info);
    } 
    
    public static void NW_PrintInfo(string info) {
        if (NW_DEBUG_MODE) Debug.Log("(Network) - " + info);
    }
    public static void NW_PrintError(string info) {
        if (NW_DEBUG_MODE) Debug.LogError("(Network) - " + info);
    }
    public static void NW_PrintException(Exception info)
    {
        if (NW_DEBUG_MODE) Debug.LogException(info);
    }
}

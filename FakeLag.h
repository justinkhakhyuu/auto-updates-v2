#pragma once

// ------------------------
// INCLUDES
// ------------------------
#include <Windows.h>
#include <atomic>
#include <thread>
#include <shlwapi.h>
#pragma comment(lib, "shlwapi.lib")

// ------------------------
// DEFINES
// ------------------------
#define MAX_PACKET_SIZE 9999
#define MAX_ALLOWED_SIZE 0

// ------------------------
// ENUMS Y CONSTANTES
// ------------------------
const char* filter = "outbound and udp.PayloadLength >= 30 and udp.PayloadLength <= 800";
WINDIVERT_LAYER layer = WINDIVERT_LAYER_NETWORK;

// NOTE: Fake lag WinDivert operations are now handled by the external app
// (which has admin privileges). The internal DLL no longer calls WinDivertOpen().

// ------------------------
// VARIABLES GLOBALES
// ------------------------

// GhostLag
HANDLE GhostHR = nullptr;
std::atomic<bool> GhostLagHR = false;

// Fake Lag v1
HANDLE FreHR2 = nullptr;
std::atomic<bool> FreezeHR2 = false;

// FreezeLag
HANDLE FreHR = nullptr;
std::atomic<bool> FreezeHR = false;

// FakeLagV2
BOOL isCutterActive = FALSE;
HANDLE handle;
std::atomic<bool> fakeLagV2Enabled = false;
bool fixLagThreadStarted = false;
int v2P = 0;
std::thread fixLagThread;
std::atomic<bool> runninglag = false;

#define FILTER_GHOST "outbound and udp.PayloadLength >= 50 and udp.PayloadLength <= 150"
std::atomic<bool> TeleportActive = false;
HANDLE hTeleport = nullptr;


// ------------------------
// DECLARACI�N DE FUNCIONES
// ------------------------

void checkKeyPress();
void cutInternet(HANDLE handle);
void allowInternet(HANDLE handle);
DWORD WINAPI FakeHR(LPVOID);
DWORD WINAPI FakeFreezeHR(LPVOID);
void FixlagV2();

// ------------------------
// IMPLEMENTACI�N DE FUNCIONES
// ------------------------

// GhostLag: Duplicador de paquetes salientes para simular jitter/lag spike
DWORD WINAPI FakeHR(LPVOID)
{
    GhostHR = WinDivertOpen("outbound and udp.PayloadLength >= 50 and udp.PayloadLength <= 300", WINDIVERT_LAYER_NETWORK, 0, 0);
    if (GhostHR == INVALID_HANDLE_VALUE) {
        DWORD error = GetLastError();
        char msg[256];
        BOOL isAdmin = FALSE;
        HANDLE token = NULL;
        if (OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &token)) {
            TOKEN_ELEVATION elevation;
            DWORD size;
            if (GetTokenInformation(token, TokenElevation, &elevation, sizeof(elevation), &size)) {
                isAdmin = elevation.TokenIsElevated;
            }
            CloseHandle(token);
        }
        if (!isAdmin) {
            sprintf_s(msg, sizeof(msg), "Error al iniciar GhostLag\nError Code: %d (0x%X)\n\nWinDivert requiere permisos de Administrador!\nEjecuta el programa como Administrador.", error, error);
        } else {
            sprintf_s(msg, sizeof(msg), "Error al iniciar GhostLag\nError Code: %d (0x%X)\n\nVerifica que WinDivert.dll este en la misma carpeta que el DLL.", error, error);
        }
        MessageBoxA(0, msg, "GhostLag Error", MB_ICONERROR);
        GhostLagHR = false; // Reset flag on error
        return 1;
    }

    char packet[0xFFFF];
    UINT packetLen;
    WINDIVERT_ADDRESS addr;

    while (GhostLagHR.load())
    {
        if (!WinDivertRecv(GhostHR, packet, sizeof(packet), &packetLen, &addr)) {
            DWORD error = GetLastError();
            if (error == ERROR_INVALID_HANDLE || error == ERROR_NO_DATA) {
                break; // Handle closed or no data, exit loop
            }
            continue;
        }

        WinDivertSend(GhostHR, packet, packetLen, nullptr, &addr); // Env�o original
        Sleep(30); // Delay que simula jitter
        WinDivertSend(GhostHR, packet, packetLen, nullptr, &addr); // Env�o duplicado
    }

    if (GhostHR != nullptr && GhostHR != INVALID_HANDLE_VALUE) {
        WinDivertClose(GhostHR);
    }
    GhostHR = nullptr;
    return 0;
}

// FreezeLag: Captura tr�fico entrante pero no lo reenv�a (efecto "congelado")
DWORD WINAPI FakeFreezeHR2(LPVOID)
{
    FreHR2 = WinDivertOpen("(inbound or outbound) and udp.PayloadLength >= 48", WINDIVERT_LAYER_NETWORK, 0, 0);
    if (FreHR2 == INVALID_HANDLE_VALUE)
    {
        MessageBoxA(0, "Error al iniciar", "Fake Lag", MB_ICONERROR);
        return 1;
    }

    char packet[MAX_PACKET_SIZE];
    UINT packetLen;
    WINDIVERT_ADDRESS addr{};

    while (FreezeHR2)
    {
        if (!WinDivertRecv(FreHR2, packet, sizeof(packet), &packetLen, &addr))
            continue;


    }

    WinDivertClose(FreHR2);
    FreHR2 = nullptr;
    return 0;
}

// FreezeLag: Captura tr�fico entrante pero no lo reenv�a (efecto "congelado")
DWORD WINAPI FakeFreezeHR(LPVOID)
{
    FreHR = WinDivertOpen("inbound and udp.PayloadLength >= 48", WINDIVERT_LAYER_NETWORK, 0, 0);
    if (FreHR == INVALID_HANDLE_VALUE)
    {
        MessageBoxA(0, "Error al iniciar", "FreezeLag", MB_ICONERROR);
        return 1;
    }

    char packet[0xFFFF];
    UINT packetLen;
    WINDIVERT_ADDRESS addr;

    while (FreezeHR)
    {
        if (!WinDivertRecv(FreHR, packet, sizeof(packet), &packetLen, &addr))
            continue;

        // No se reenv�a el paquete => efecto de "lag"
    }

    WinDivertClose(FreHR);
    FreHR = nullptr;
    return 0;
}


DWORD WINAPI ThreadTeleport(LPVOID) {
    hTeleport = WinDivertOpen(FILTER_GHOST, WINDIVERT_LAYER_NETWORK, 0, 0);
    if (hTeleport == INVALID_HANDLE_VALUE) {
        MessageBoxA(0, "Error al iniciar Teleport", "Teleport", MB_ICONERROR);
        return 1;
    }
    char packet[0xFFFF]; UINT packetLen; WINDIVERT_ADDRESS addr;
    while (TeleportActive) {
        WinDivertRecv(hTeleport, packet, sizeof(packet), &packetLen, &addr);
    }
    WinDivertClose(hTeleport); hTeleport = nullptr;
    return 0;
}

// Fake V2 Captura pulsaci�n de tecla para activar/desactivar corte de internet (tecla F)
void checkKeyPress()
{
    if (GetAsyncKeyState('F') & 0x8000)
    {
        isCutterActive = !isCutterActive;
        printf("Cortador de internet %s.\n", isCutterActive ? "activado" : "desactivado");
    }
}

// Corta internet descartando paquetes de salida mayores a cierto tama�o
void cutInternet(HANDLE handle)
{
    WINDIVERT_ADDRESS addr;
    char packet[MAX_PACKET_SIZE];
    UINT packetLen;

    if (WinDivertRecv(handle, packet, sizeof(packet), &packetLen, &addr))
    {
        if (packetLen > MAX_ALLOWED_SIZE)
        {
            printf("Paquete de salida descartado debido a que supera el tama�o m�ximo permitido.\n");
        }
        else
        {
            printf("Paquete de salida capturado y descartado\n");
        }
    }
    else
    {
        fprintf(stderr, "Error: No se puede recibir el paquete (%d)\n", GetLastError());
    }
}

// Permite tr�fico normal reenviando paquetes
void allowInternet(HANDLE handle)
{
    WINDIVERT_ADDRESS addr;
    char packet[MAX_PACKET_SIZE];
    UINT packetLen;

    if (WinDivertRecv(handle, packet, sizeof(packet), &packetLen, &addr))
    {
        if (!WinDivertSend(handle, packet, packetLen, NULL, &addr))
        {
            fprintf(stderr, "Error: No se puede enviar el paquete (%d)\n", GetLastError());
        }
    }
    else
    {
        fprintf(stderr, "Error: No se puede recibir el paquete (%d)\n", GetLastError());
    }
}

// Controla el flujo de corte/permiso seg�n el estado de fakeLagV2Enabled
void FixlagV2()
{
    while (runninglag)
    {
        if (fakeLagV2Enabled)
        {
            if (v2P == 2)
            {
                cutInternet(handle);
                v2P = 0;
            }
            else if (v2P == 1)
            {
                allowInternet(handle);
            }
        }
        else
        {
            v2P = 0;
        }
        Sleep(1);
    }
}
#include <Windows.h>
#include <atomic>

HANDLE AimFreHR = nullptr;
std::atomic<bool> AimLagHR = false;

DWORD WINAPI FakeAimGH(LPVOID)
{
    AimFreHR = WinDivertOpen("(inbound or outbound) and udp.PayloadLength >= 48", WINDIVERT_LAYER_NETWORK, 0, 0);
    if (AimFreHR == INVALID_HANDLE_VALUE) {
        MessageBoxA(0, "Error al iniciar", "AimLag", MB_ICONERROR);
        return 1;
    }

    char packet[0xFFFF];
    UINT packetLen;
    WINDIVERT_ADDRESS addr;

    while (AimLagHR)
    {
        if (!WinDivertRecv(AimFreHR, packet, sizeof(packet), &packetLen, &addr))
            continue;

        // Simulamos un pequeño lag (100 ms)
        Sleep(100);

        // Reenviamos el paquete después del delay
        WinDivertSend(AimFreHR, packet, packetLen, nullptr, &addr);
    }

    WinDivertClose(AimFreHR);
    AimFreHR = nullptr;
    return 0;
}
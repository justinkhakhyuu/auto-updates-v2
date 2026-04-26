HANDLE hGH = nullptr;
std::atomic<bool> fakeLagGH = false;


DWORD WINAPI FakeGH(LPVOID) {
    hGH = WinDivertOpen("outbound and udp.PayloadLength >= 50 and udp.PayloadLength <= 300", WINDIVERT_LAYER_NETWORK, 0, 0);
    if (hGH == INVALID_HANDLE_VALUE) {
        MessageBoxA(0, "Error al iniciar", "@elsrt_alex1", MB_ICONERROR);
        return 1;
    }

    char packet[0xFFFF];
    UINT packetLen;
    WINDIVERT_ADDRESS addr;

    while (fakeLagGH) {
        WinDivertRecv(hGH, packet, sizeof(packet), &packetLen, &addr);

    }

    WinDivertClose(hGH);
    hGH = nullptr;
    return 0;
}

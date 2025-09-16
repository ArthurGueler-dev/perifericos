using System.Runtime.InteropServices;

namespace Perifericos.Agent;

public class PopupNotifier
{
    private const int WTS_CURRENT_SERVER_HANDLE = 0;
    private const int WTS_BROADCAST = -1;

    public void ShowPopup(string title, string message)
    {
        WTSSendMessage(IntPtr.Zero, WTS_BROADCAST, title, title.Length * 2, message, message.Length * 2, 0, 15, out var resp, true);
    }

    [DllImport("Wtsapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool WTSSendMessage(
        IntPtr hServer,
        int SessionId,
        string pTitle,
        int TitleLength,
        string pMessage,
        int MessageLength,
        int Style,
        int Timeout,
        out int pResponse,
        bool bWait);
}



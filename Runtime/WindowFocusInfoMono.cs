using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class WindowFocusInfoMono : MonoBehaviour
{
    // Import the Windows API methods
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    public float m_timeBetweenCheck = 0.1f;

    public string m_currentWindowName = "";
    public uint m_currentProcessId = 0;

    public List<string> m_previous = new List<string>();

    public UnityEvent<string> m_onNewWindowTitle;
    public UnityEvent<uint> m_onNewProcessId;
    public UnityEvent<string, uint> m_onNewWindowInfo;

    public List<EventOnWindowTitle> m_onWindowTitleEvents = new List<EventOnWindowTitle>();

    [Serializable]
    public class EventOnWindowTitle {
        
        public string m_windowTitle;
        public enum EventType {Contains, StartsWith, EndsWith, Equals}
        public EventType m_eventType;
        public bool m_ignoreCase;
        public bool m_isFocused;
        public UnityEvent<bool> m_onisCurrentlyFocus;
        public void PushInChangedWindow(string title) {         
            bool isFocused = false;
            
            string windowTitle = m_windowTitle; 
            if (m_ignoreCase) title = title.ToLower();
            if (m_ignoreCase) windowTitle = windowTitle.ToLower();
            switch (m_eventType) {
                case EventType.Contains:
                    isFocused = title.Contains(windowTitle);
                    break;
                case EventType.StartsWith:
                    isFocused = title.StartsWith(windowTitle);
                    break;
                case EventType.EndsWith:
                    isFocused = title.EndsWith(windowTitle);
                    break;
                case EventType.Equals:
                    isFocused = title.Equals(windowTitle);
                    break;
            }
            m_isFocused = isFocused;
            m_onisCurrentlyFocus.Invoke(isFocused);
        }
    }

    

    public void OnEnable()
    {
        InvokeRepeating(nameof(CheckForWindowName), 0, m_timeBetweenCheck);
    }

    [ContextMenu("Check Window Name")]
    public void CheckForWindowName() {
        string previousWindowName = m_currentWindowName;
        uint previousProcessId = m_currentProcessId;
        GetCurrentWindowInfo(out m_currentWindowName, out m_currentProcessId);
        if (previousWindowName != m_currentWindowName || previousProcessId != m_currentProcessId) {

            m_onNewWindowTitle.Invoke(m_currentWindowName);
            m_onNewProcessId.Invoke(m_currentProcessId);
            m_onNewWindowInfo.Invoke(m_currentWindowName, m_currentProcessId);
            
            m_previous.Insert(0, m_currentWindowName + "|" + m_currentProcessId);
            while (m_previous.Count > 10) m_previous.RemoveAt(m_previous.Count - 1);

            foreach (var item in m_onWindowTitleEvents) {
                item.PushInChangedWindow(m_currentWindowName);
            }
        }

    }


    private void GetCurrentWindowInfo(out string windowTitle, out uint pid)
    {
        // Get handle of the currently focused window
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd == IntPtr.Zero) { 
        
            windowTitle = "No Window";
            pid = 0;
            return;
        }
        
        GetWindowThreadProcessId(hWnd, out uint processId);
        const int nChars = 256;
        System.Text.StringBuilder windowText = new System.Text.StringBuilder(nChars);
        GetWindowText(hWnd, windowText, nChars);

        windowTitle = windowText.ToString();
        pid = processId;

    }
}

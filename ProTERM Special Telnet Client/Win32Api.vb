Module Win32Api

    Public Declare Auto Function BitBlt Lib "GDI32.DLL" (
        ByVal hdcDest As IntPtr,
        ByVal nXDest As Integer,
        ByVal nYDest As Integer,
        ByVal nWidth As Integer,
        ByVal nHeight As Integer,
        ByVal hdcSrc As IntPtr,
        ByVal nXSrc As Integer,
        ByVal nYSrc As Integer,
        ByVal dwRop As Integer) As Boolean

    Public Declare Auto Function CreateCompatibleDC Lib "GDI32.DLL" (ByVal hdc As IntPtr) As IntPtr

    Public Declare Auto Function SelectObject Lib "GDI32.dll" (ByVal hdc As IntPtr, ByVal hgdiobj As IntPtr) As IntPtr

End Module

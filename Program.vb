Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Text
Imports System.Threading

Module MainModule
    Dim Blacklist As New Dictionary(Of String, Long)
    Dim Packet As String = "server|3.227.208.4" & vbCrLf & "port|17091" & vbCrLf & "type|1" & vbCrLf & "#maint|Protected By AntiDDoS" & vbCrLf & vbCrLf & "beta_server|127.0.0.1" & vbCrLf & "beta_port|17091" & vbCrLf & vbCrLf & "beta_type|1" & vbCrLf & "meta|localhost" & vbCrLf & "RTENDMARKERBS1001"
    Dim Listener As HttpListener

    Sub Main()
        Console.Title = "Anti-DDoS Server"
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("===================================")
        Console.WriteLine("   Welcome to Anti-DDoS Server")
        Console.WriteLine("===================================")
        Console.ResetColor()

        Dim prefix As String = "http://*:80/"
        Listener = New HttpListener()
        Listener.Prefixes.Add(prefix)
        Listener.Start()

        Console.ForegroundColor = ConsoleColor.Yellow
        Console.WriteLine("Server is now running...")
        Console.ResetColor()

        While True
            Dim context As HttpListenerContext = Listener.GetContext()
            Dim request As HttpListenerRequest = context.Request
            Dim response As HttpListenerResponse = context.Response

            If request.Url.AbsolutePath = "/growtopia/server_data.php" AndAlso request.HttpMethod.ToUpper() = "POST" Then
                Dim buffer() As Byte = Encoding.UTF8.GetBytes(Packet)
                response.ContentType = "text/html"
                response.ContentLength64 = buffer.Length
                response.OutputStream.Write(buffer, 0, buffer.Length)
                response.OutputStream.Close()
                response.Close()
            Else
                response.StatusCode = 404
                response.Close()
            End If
        End While
    End Sub

    Sub AddAddress(address As String)
        If Not Blacklist.ContainsKey(address) Then
            Blacklist(address) = Date.UtcNow.Ticks + TimeSpan.FromSeconds(5).Ticks
        End If
    End Sub

    Sub CheckBlacklist(socket As HttpListenerContext)
        Dim ip As String = socket.Request.RemoteEndPoint.Address.ToString()
        If Not Blacklist.ContainsKey(ip) Then
            AddAddress(ip)
        Else
            Dim notAllowed As Long = Blacklist(ip)
            If Date.UtcNow.Ticks > notAllowed Then
                Blacklist.Remove(ip)
            Else
                socket.Response.Close()
            End If
        End If
    End Sub
End Module

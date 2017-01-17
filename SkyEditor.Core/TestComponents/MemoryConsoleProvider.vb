Imports System.Text
Imports SkyEditor.Core.ConsoleCommands

Namespace TestComponents
    Public Class MemoryConsoleProvider
        Implements IConsoleProvider

        Public Sub New()
            BackgroundColor = ConsoleColor.Black
            ForegroundColor = ConsoleColor.White
            StdIn = New StringBuilder
            StdOut = New StringBuilder
        End Sub

        Public Property BackgroundColor As ConsoleColor Implements IConsoleProvider.BackgroundColor

        Public Property ForegroundColor As ConsoleColor Implements IConsoleProvider.ForegroundColor

        Public Property StdIn As StringBuilder
        Private _stdInLock As New Object

        Public Property StdOut As StringBuilder
        Private _stdOutLock As New Object

        Public Sub Write(value() As Char) Implements IConsoleProvider.Write
            SyncLock _stdOutLock
                For Each c In value
                    StdOut.Append(c)
                Next
            End SyncLock
        End Sub

        Public Sub Write(value As String) Implements IConsoleProvider.Write
            SyncLock _stdOutLock
                StdOut.Append(value)
            End SyncLock
        End Sub

        Public Sub Write(value As Object) Implements IConsoleProvider.Write
            SyncLock _stdOutLock
                StdOut.Append(value.ToString)
            End SyncLock
        End Sub

        Public Sub Write(value As Boolean) Implements IConsoleProvider.Write
            SyncLock _stdOutLock
                StdOut.Append(value.ToString)
            End SyncLock
        End Sub

        Public Sub Write(format As String, ParamArray arg() As Object) Implements IConsoleProvider.Write
            SyncLock _stdOutLock
                StdOut.AppendFormat(format, arg)
            End SyncLock
        End Sub

        Public Sub Write(value() As Char, index As Integer, count As Integer) Implements IConsoleProvider.Write
            SyncLock _stdOutLock
                StdOut.Append(value, index, count)
            End SyncLock
        End Sub

        Public Sub WriteLine() Implements IConsoleProvider.WriteLine
            SyncLock _stdOutLock
                StdOut.AppendLine()
            End SyncLock
        End Sub

        Public Sub WriteLine(value() As Char) Implements IConsoleProvider.WriteLine
            SyncLock _stdOutLock
                For Each c In value
                    StdOut.Append(c)
                Next
                StdOut.AppendLine()
            End SyncLock
        End Sub

        Public Sub WriteLine(value As Object) Implements IConsoleProvider.WriteLine
            SyncLock _stdOutLock
                StdOut.AppendLine(value.ToString)
            End SyncLock
        End Sub

        Public Sub WriteLine(value As String) Implements IConsoleProvider.WriteLine
            SyncLock _stdOutLock
                StdOut.AppendLine(value)
            End SyncLock
        End Sub

        Public Sub WriteLine(format As String, ParamArray arg() As Object) Implements IConsoleProvider.WriteLine
            SyncLock _stdOutLock
                StdOut.AppendFormat(format, arg)
                StdOut.AppendLine()
            End SyncLock
        End Sub

        Public Sub WriteLine(value() As Char, index As Integer, count As Integer) Implements IConsoleProvider.WriteLine
            SyncLock _stdOutLock
                StdOut.Append(value, index, count)
                StdOut.AppendLine()
            End SyncLock
        End Sub

        Public Function Read() As Integer Implements IConsoleProvider.Read
            Dim c As Integer
            SyncLock _stdInLock
                If StdIn.Length > 0 Then
                    c = Convert.ToInt32(StdIn(0))
                    StdIn.Remove(0, 1)
                Else
                    c = -1
                End If
            End SyncLock
            Return c
        End Function

        Public Function ReadLine() As String Implements IConsoleProvider.ReadLine
            Dim line As New StringBuilder
            SyncLock _stdInLock
                While StdIn.Length > 0 AndAlso (line.Length = 0 OrElse line(line.Length - 1) <> vbLf)
                    line.Append(StdIn(0))
                    StdIn.Remove(0, 1)
                End While
            End SyncLock
            Return line.ToString.Trim
        End Function

        Public Function GetStdOut() As String
            SyncLock _stdOutLock
                Return StdOut.ToString
            End SyncLock
        End Function

    End Class
End Namespace
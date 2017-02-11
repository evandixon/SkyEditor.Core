Imports SkyEditor.Core.ConsoleCommands
Imports SkyEditor.Core.Utilities

Namespace ConsoleCommands
    Public Class DistPrep
        Inherits ConsoleCommand

        Protected Overrides Sub Main(Arguments() As String)
            RedistributionHelpers.PrepareForDistribution(CurrentApplicationViewModel.CurrentPluginManager)
        End Sub
    End Class
End Namespace
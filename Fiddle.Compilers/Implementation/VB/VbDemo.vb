Module Hello
    Function Main() As Integer
        WriteHello()
		Return 500
    End Function
	
	Sub WriteHello
        System.Console.WriteLine("Hello world!")
	End Sub
End Module
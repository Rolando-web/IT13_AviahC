$conn = New-Object System.Data.SqlClient.SqlConnection("Server=REVISION-PC\SQLEXPRESS;Database=AviahCollectionDB;Trusted_Connection=True;Encrypt=False;")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT ProductID, ProductName, ImageUrl FROM Products"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) { 
    Write-Host "$($reader['ProductID']) - $($reader['ProductName']) - $($reader['ImageUrl'])" 
}
$conn.Close()

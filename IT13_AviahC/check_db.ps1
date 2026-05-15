$conn = New-Object System.Data.SqlClient.SqlConnection("Server=REVISION-PC\SQLEXPRESS;Database=AviahCollectionDB;Trusted_Connection=True;Encrypt=False;")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "EXEC sp_columns ProductionBatches"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) { 
    Write-Host "$($reader['COLUMN_NAME']) - $($reader['TYPE_NAME'])" 
}
$conn.Close()

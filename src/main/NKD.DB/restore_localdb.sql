USE [master]
RESTORE DATABASE [NKD] FROM  DISK = N'C:\Temp\nkd_ent\nkd_ent.bak' WITH  FILE = 1,  NOUNLOAD,  STATS = 5

GO
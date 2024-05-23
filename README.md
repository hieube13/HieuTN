#HIEUTN Project

##Application Urls:
- Exam API : https://localhost:5002
- Identity API : https://localhost:5005
- Identity STS : https://localhost:6004
- Identity Admin : https://localhost:6003
- Exam Admin : https://localhost:6001
- Portal Exam : https://localhost:6002

##Delete database
USE master
GO

ALTER DATABASE [Identity] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
DROP DATABASE [Identity]
GO

#MudBlazor
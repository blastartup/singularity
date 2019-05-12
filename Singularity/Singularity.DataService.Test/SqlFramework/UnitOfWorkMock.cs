using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Singularity.DataService.SqlFramework;

namespace Singularity.DataService.Test.SqlFramework
{
	public class UnitOfWorkMock : SqlUnitOfWork<SqlEntityContextMock>
	{
		// Do not use.
		private UnitOfWorkMock() : base()
		{
		}

		private UnitOfWorkMock(SqlConnectionStringBuilder sqlConnectionStringBuilder)
		{
			_sqlConnectionStringBuilder = sqlConnectionStringBuilder;
		}

		public UnitOfWorkMock(SqlConnection sqlConnection)
		{
			_sqlConnection = sqlConnection;
		}

		//public RepositoryMock RepositoryMock => _repositoryMock ?? (_repositoryMock = new RepositoryMock(Context));
		//private RepositoryMock _repositoryMock;


		protected override String CreateDatabaseQuery => @"CREATE DATABASE [Singularity]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Singularity', FILENAME = N'f:\Program Files\Microsoft SQL Server\MSSQL13.FUNKEY\MSSQL\DATA\Singularity.mdf' , SIZE = 8192KB , FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Singularity_log', FILENAME = N'f:\Program Files\Microsoft SQL Server\MSSQL13.FUNKEY\MSSQL\DATA\Singularity_log.ldf' , SIZE = 8192KB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [Singularity] SET COMPATIBILITY_LEVEL = 130
GO
ALTER DATABASE [Singularity] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Singularity] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Singularity] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Singularity] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Singularity] SET ARITHABORT OFF 
GO
ALTER DATABASE [Singularity] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Singularity] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Singularity] SET AUTO_CREATE_STATISTICS ON(INCREMENTAL = OFF)
GO
ALTER DATABASE [Singularity] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Singularity] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Singularity] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Singularity] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Singularity] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Singularity] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Singularity] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Singularity] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Singularity] SET AUTO_UPDATE_STATISTICS_ASYNC ON 
GO
ALTER DATABASE [Singularity] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Singularity] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Singularity] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Singularity] SET  READ_WRITE 
GO
ALTER DATABASE [Singularity] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Singularity] SET  MULTI_USER 
GO
ALTER DATABASE [Singularity] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Singularity] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Singularity] SET DELAYED_DURABILITY = DISABLED 
GO
USE [Singularity]
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = Off;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = Primary;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = On;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = Primary;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = Off;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = Primary;
GO
USE [Singularity]
GO
IF NOT EXISTS (SELECT name FROM sys.filegroups WHERE is_default=1 AND name = N'PRIMARY') ALTER DATABASE [Singularity] MODIFY FILEGROUP [PRIMARY] DEFAULT
GO
";

		protected override String DeleteDatabaseQuery => @"EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'Singularity'
GO
USE [master]
GO
ALTER DATABASE [Singularity] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
GO
DROP DATABASE [Singularity]
GO
";

		public TeeProjectRepository TeeProjectRepository => _teeProjectRepository ?? (_teeProjectRepository = new TeeProjectRepository(Context));
		private TeeProjectRepository _teeProjectRepository;


		protected override SqlEntityContextMock NewDbContext() => _sqlConnectionStringBuilder != null ? new SqlEntityContextMock(_sqlConnectionStringBuilder) : new SqlEntityContextMock(_sqlConnection);

		protected override void ResetRepositories()
		{
			_teeProjectRepository = null;
		}



		private readonly SqlConnectionStringBuilder _sqlConnectionStringBuilder;
		private readonly SqlConnection _sqlConnection;
	}
}

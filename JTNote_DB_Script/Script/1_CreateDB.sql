/**
 * IPD-16 DOTNET2 - FINAL PROJECT
 *
 * Create JTNotes database
 *	Script Date: May 1, 2019
 *  Developed by: Taylor and Jeonghoon
 */

-- switch to master database
use master
;
go

-- create a database
create database JTNotes
on primary
(
	-- logical rows data file name
	name = 'JTNotes',
	-- rows data path and file name
	filename = 'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\JTNotes.mdf',
	-- rows data file size
	size = 12MB,
	-- rows data file growth
	filegrowth = 10%,
	-- rows data maximum size
	maxsize = 100MB
)
log on
(
	-- logical log file name
	name = 'JTNotes_log',
	-- log path and file name
	filename = 'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\JTNotes.ldf',
	-- log file size
	size = 3MB,
	-- log file growth
	filegrowth = 10%,
	-- log maximum size
	maxsize = 25MB
)
;
go


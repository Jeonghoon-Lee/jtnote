/**
 * IPD-16 DOTNET2 - FINAL PROJECT
 *
 * Create JTNotes database
 *	Script Date: May 1, 2019
 *  Developed by: Taylor and Jeonghoon
 */

-- switch to JTNotes database
use JTNotes
;
go


if OBJECT_ID('Dbo.Users') is not null
	drop table Dbo.Users
;
go

create table Dbo.Users
(
	Id int identity(1, 1) not null,
	UserName nvarchar(30) null,		-- future use
	Email nvarchar(50) not null,	-- length 320 is recommended (64:local part + 255:domain part)
	Password nchar(33) not null,

	constraint pk_Users primary key clustered (Id asc)
)
;
go


if OBJECT_ID('Dbo.Notes') is not null
	drop table Dbo.Notes
;
go

create table Dbo.Notes
(
	Id int identity(1, 1) not null,
	UserId int not null,				-- foreign key from Users table
	Title nvarchar(128) not null,
	Content varbinary(MAX) not null,
	NotebookId int null,				-- foreign key from Categories table
	IsDeleted tinyint not null,			-- 0: Not deleted, 1: into Trash
	LastUpdatedDate datetime not null,

	constraint pk_Notes primary key clustered (Id asc)
)
;
go


if OBJECT_ID('Dbo.Notebooks') is not null
	drop table Dbo.Notebooks
;
go

create table Dbo.Notebooks
(
	Id int identity(1, 1) not null,
	Name nvarchar(20) not null,

	constraint pk_Notebooks primary key clustered (Id asc)
)
;
go


if OBJECT_ID('Dbo.Tags') is not null
	drop table Dbo.Tags
;
go

create table Dbo.Tags
(
	Id int identity(1, 1) not null,
	Name nvarchar(20) not null,
	UserId int not null,	-- foreign key from Users table

	constraint pk_Tags primary key clustered (Id asc)
)
;
go


-- Junction table for Notes and Tags
if OBJECT_ID('Dbo.NoteTag') is not null
	drop table Dbo.NoteTag
;
go

create table Dbo.NoteTag
(
	NoteId int not null,	-- foreign key from Notes table
	TagId int not null,		-- foreign key from Tags table

	constraint pk_NoteTag primary key clustered (NoteId asc, TagId asc)
)
;
go


if OBJECT_ID('Dbo.SharedNotes') is not null
	drop table Dbo.SharedNotes
;
go

create table Dbo.SharedNotes
(
	UserId int not null,	-- foreign key from Users table
	NoteId int not null,	-- foreign key from Notes table
	Permission tinyint not null,	-- 0: read-only 1: read and edit, 2: ??

	constraint pk_SharedNotes primary key clustered (UserId asc, NoteId asc)
)
;
go


if OBJECT_ID('Dbo.Messages') is not null
	drop table Dbo.Messages
;
go

create table Dbo.Messages
(
	Id int identity(1, 1) not null,
	UserId int not null,	-- foreign key from Users table
	Content nvarchar(1000) not null,

	constraint pk_Messages primary key clustered (Id asc)
)
;
go


-- Junction table between Messages and Users table
if OBJECT_ID('Dbo.MessageReceivers') is not null
	drop table Dbo.MessageReceivers
;
go

create table Dbo.MessageReceivers
(
	MessageId int not null,		-- foreign key from Messages table
	ReceiverId int not null,	-- foreign key from Users table

	constraint pk_MessageReceivers primary key clustered (MessageId asc, ReceiverId asc)
)
;
go

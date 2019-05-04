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

-- Foreign key between Notes and Users table
alter table Dbo.Notes
	add constraint fk_Notes_Users foreign key (UserId)
		references Dbo.Users (Id)
;
go

-- Foreign key between Notes and Notebooks table
alter table Dbo.Notes
	add constraint fk_Notes_Notebooks foreign key (NotebookId)
		references Dbo.Notebooks (Id)
;
go

-- Foreign key between Tags and Users table
alter table Dbo.Tags
	add constraint fk_Tags_Users foreign key (UserId)
		references Dbo.Users (Id)
;
go

-- Foreign key between NoteTag and Notes table
alter table Dbo.NoteTag
	add constraint fk_NoteTag_Notes foreign key (NoteId)
		references Dbo.Notes (Id)
;
go

-- Foreign key between NoteTag and Tags table
alter table Dbo.NoteTag
	add constraint fk_NoteTag_Tags foreign key (TagId)
		references Dbo.Tags (Id)
;
go

-- Foreign key between SharedNotes and Users table
alter table Dbo.SharedNotes
	add constraint fk_SharedNotes_Users foreign key (UserId)
		references Dbo.Users (Id)
;
go

-- Foreign key between SharedNotes and Notes table
alter table Dbo.SharedNotes
	add constraint fk_SharedNotes_Notes foreign key (NoteId)
		references Dbo.Notes (Id)
;
go

-- Foreign key between Messages and Users table
alter table Dbo.Messages
	add constraint fk_Messages_Users foreign key (UserId)
		references Dbo.Users (Id)
;
go

-- Foreign key between MessageReceivers and Messages table
alter table Dbo.MessageReceivers
	add constraint fk_MessageReceivers_Messages foreign key (MessageId)
		references Dbo.Messages (Id)
;
go

-- Foreign key between MessageReceivers and Users table
alter table Dbo.MessageReceivers
	add constraint fk_MessageReceivers_Users foreign key (ReceiverId)
		references Dbo.Users (Id)
;
go


-- Email in Users table is unique
alter table Dbo.Users
	add constraint uq_Users_Email
		unique (Email)
;
go
